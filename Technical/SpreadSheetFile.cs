// HighlightKPIExport
// Copyright (C) 2020-2022 David MARKEY

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using DocumentFormat.OpenXml;  
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Spreadsheet; 

namespace HighlightKPIExport.Technical {
    // wrapper autour d'un SpreadsheetDocument OpenXML
    public class SpreadSheetFile : IDisposable {

        public SpreadSheetFile(string fileName) {
            FileName = fileName;
            if (File.Exists(fileName)) {
                // ouverture d'un fichier existant
                xlsx = SpreadsheetDocument.Open(fileName, true);
                dirty = false;
            } else {
                // création d'un nouveau fichier
                xlsx = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
                var wbPart = xlsx.AddWorkbookPart();  
                wbPart.Workbook = new Workbook();
                dirty = true;
            }
        }

        public string FileName { get; private set; }

        public void Dispose() {
            xlsx.Dispose();
        }

        private SpreadsheetDocument xlsx;
        private bool dirty;

        private WorkbookPart wbPart => xlsx.WorkbookPart;

        #region "Private -- Shared Strings"

        private SharedStringTablePart _ssPart = null;
        private SharedStringTablePart ssPart {
            get {
                if (_ssPart == null) {
                    _ssPart = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    if (_ssPart == null) {
                        // création d'un SharedStringTablePart si le fichier n'en contenait pas
                        _ssPart = wbPart.AddNewPart<SharedStringTablePart>();
                        _ssPart.SharedStringTable = new SharedStringTable();
                        dirty = true;
                    }
                }
                return _ssPart;
            }
        }

        // récupération d'une chaîne partagée
        private string GetSharedStringItem(int idx) {
            if (idx < 0 || idx >= ssPart.SharedStringTable.Count()) return null;
            return ssPart.SharedStringTable.ElementAt(idx).InnerText;
        }

        // ajout d'une chaîne partagée
        private int InsertSharedStringItem(string text) {
            var ssIdx = 0;
            foreach (SharedStringItem item in ssPart.SharedStringTable.Elements<SharedStringItem>()) {
                if (item.InnerText == text) {
                    return ssIdx;
                }
                ssIdx++;
            }

            ssPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            ssPart.SharedStringTable.Save();
            dirty = true;
            return ssIdx;
        }

        #endregion

        // sauvegarde avec forçage des calculs si nécessaire
        public void Save() {
            if (dirty) {
                var calculationProperties = wbPart.Workbook.CalculationProperties;
                if (calculationProperties == null) {
                    calculationProperties = new CalculationProperties();
                    wbPart.Workbook.CalculationProperties = calculationProperties;
                }
                calculationProperties.ForceFullCalculation = true;
                calculationProperties.FullCalculationOnLoad = true;
                wbPart.Workbook.Save();
                dirty = false;
            }
        }

        // récupération d'une feuille de calcul par son nom
        public Worksheet FindSheet(string name) {
            var sheet =  wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == name).FirstOrDefault();
            if (sheet == null) {
                return null;
            }
            var wsPart =  (WorksheetPart)(wbPart.GetPartById(sheet.Id));
            return wsPart.Worksheet;
        }

        // récupération ou création d'une feuille de calcul par son nom
        public Worksheet FindOrCreateSheet(string name) {
            var worksheet = FindSheet(name);
            if (worksheet != null) {
                return worksheet;
            } else {
                var wsPart = wbPart.AddNewPart<WorksheetPart>();
                wsPart.Worksheet = new Worksheet(new SheetData());
                wsPart.Worksheet.Save();

                var sheets = wbPart.Workbook.GetFirstChild<Sheets>();
                if (sheets == null) {
                    sheets = new Sheets();
                    wbPart.Workbook.AppendChild(sheets);
                }
                var rId = wbPart.GetIdOfPart(wsPart);

                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Any()) {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                var sheet = new Sheet() { Id = rId, SheetId = sheetId, Name = name };
                sheets.Append(sheet);
                wbPart.Workbook.Save();

                dirty = true;
                return wsPart.Worksheet;
            }
        }

        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // calcul du nom de colonne (A, B... , Y, Z, AA, AB...) en fonction de son index
        private static string ColumnName(int idx) {
            if (idx < letters.Length) return letters[idx].ToString();
            return ColumnName(idx / letters.Length - 1) + letters[idx % letters.Length];
        }

        // calcul du nom de colonne (A, B... , Y, Z, AA, AB...) en fonction de son index
        public string GetColumnName(int colIdx) {
            return ColumnName(colIdx - 1);
        }

        // vérification qu'une cellule appartient à une colonne
        static bool IsColumn(StringValue cellRef, string colName) {
            if (!cellRef.Value.StartsWith(colName, StringComparison.InvariantCultureIgnoreCase)) return false;
            if (cellRef.Value.Length <= colName.Length || !char.IsNumber(cellRef.Value[colName.Length])) return false;
            return true;
        }

        // chargement des cellules d'une colonne
        public List<Cell> LoadColumnCells(Worksheet sheet, string colName) {
            return sheet.Descendants<Cell>().Where(_ => IsColumn(_.CellReference, colName)).ToList();
        }

        // calcul de l'index de la dernière ligne de données
        public uint GetMaxRow(List<Cell> cells) {
            uint maxRow = 0;
            for (var i = 0; i < cells.Count; i++) {
                var cell = cells[i];
                var idx = 0;
                var len = cell.CellReference.Value.Length;
                while (idx < len && !char.IsNumber(cell.CellReference.Value[idx])) {
                    idx++;
                }
                if (idx < len && uint.TryParse(cell.CellReference.Value.Substring(idx), out uint row) && row > maxRow) {
                    var v = GetCellValue(cell)?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(v)) {
                        maxRow = row;
                    }
                }
            }
            return maxRow;
        }

        // récupération ou création d'une cellule
        public Cell FindOrCreateCell(Worksheet worksheet, string columnName, uint rowIndex) {
            var cellReference = $"{columnName}{rowIndex}";
            var sheetData = worksheet.GetFirstChild<SheetData>();

            var row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).FirstOrDefault();
            if (row == null) {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            var cell = row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).FirstOrDefault();
            if (cell == null) {
                Cell refCell = null;
                foreach (Cell c in row.Elements<Cell>()) {
                    if (c.CellReference.Value.Length == cellReference.Length) {
                        if (string.Compare(c.CellReference.Value, cellReference, StringComparison.InvariantCultureIgnoreCase) > 0) {
                            refCell = c;
                            break;
                        }
                    }
                }

                cell = new Cell() { CellReference = cellReference };
                row.InsertBefore(cell, refCell);
                worksheet.Save();
                dirty = true;
            }
            return cell;
        }

        // récupération ou création d'une cellule
        public Cell FindOrCreateCell(Worksheet worksheet, int colIdx, uint rowIndex) {
            return FindOrCreateCell(worksheet, GetColumnName(colIdx), rowIndex);
        }

        private static readonly Nullable<double> NULL_NUMERIC = new Nullable<double>();
        private static readonly Nullable<bool> NULL_BOOLEAN = new Nullable<bool>();
        private static readonly Nullable<DateTime> NULL_DATETIME = new Nullable<DateTime>();

        // récupération de la valeur numérique d'une cellule
        private static Nullable<double> GetNumericValue(string value) {
            if (!string.IsNullOrWhiteSpace(value)) {
                double number;
                var res = double.TryParse(value, out number)
                        || double.TryParse(value.Replace(",", "."), out number)
                        || double.TryParse(value.Replace(".", ","), out number);
                if (res) return number;
            }
            return NULL_NUMERIC;
        }

        // récupération de la valeur d'une cellule
        public dynamic GetCellValue(Cell cell) {
            var value = cell?.CellValue?.InnerText?.Trim() ?? string.Empty;
            var hasValue = !string.IsNullOrWhiteSpace(value);
            var nval = hasValue ? GetNumericValue(value) : NULL_NUMERIC;
            var dataType = cell.DataType?.Value ?? CellValues.Number;
            switch (dataType) {
                case CellValues.String:
                case CellValues.InlineString:
                    // chaîne
                    // TOTO: à tester
                    return value;
                case CellValues.SharedString:
                    // chaîne partagée
                    if (nval.HasValue) {
                        return GetSharedStringItem((int)nval.Value);
                    } else {
                        return string.Empty;
                    }
                case CellValues.Boolean:
                    // booléen
                    if (nval.HasValue) {
                        return (nval.Value != 0);
                    } else {
                        return NULL_BOOLEAN;
                    }
                case CellValues.Date:
                    // date/heure
                    if (nval.HasValue) {
                        return DateTime.FromOADate(nval.Value);
                    } else {
                        return NULL_DATETIME;
                    }
                case CellValues.Number:
                    // numérique
                    if (nval.HasValue) {
                        return nval.Value;
                    } else {
                        return NULL_NUMERIC;
                    }
                default:
                    // autre type
                    // TOTO: à tester
                    if (nval.HasValue) {
                        return nval.Value;
                    } else {
                        return NULL_NUMERIC;
                    }
            }
        }

        // affectation d'une valeur à une cellule
        public void SetCellValue(Cell cell, dynamic value) {
            dirty = true;
            value = value ?? string.Empty;
            if (value is string sval) {
                // chaîne partagée
                var idx = InsertSharedStringItem(sval);
                cell.CellValue = new CellValue(idx.ToString());
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            } else if (value is bool bval) {
                // booléen
                cell.CellValue = new CellValue(bval ? "1" : "0");
                cell.DataType = new EnumValue<CellValues>(CellValues.Boolean);
            } else if (value is DateTime dval) {
                // date/heure
                var d = dval.ToOADate();
                cell.CellValue = new CellValue(d.ToString(CultureInfo.InvariantCulture));
                cell.DataType = null;
            } else {
                // numérique par défaut
                // TODO: il faudrait tester le type de value
                cell.CellValue = new CellValue(value.ToString());
                cell.DataType = null;
            }
        }

        // affectation d'une valeur à une cellule
        public void SetCellValue(Worksheet worksheet, string columnName, uint rowIndex, dynamic value) {
            SetCellValue(FindOrCreateCell(worksheet, columnName, rowIndex), value);
        }

        // affectation d'une formule à une cellule
        public void SetCellFormula(Cell cell, string formula) {
            dirty = true;
            cell.CellFormula = new CellFormula(formula);
        }

        // affectation d'une formule à une cellule
        public void SetCellFormula(Worksheet worksheet, string columnName, uint rowIndex, string formula) {
            SetCellFormula(FindOrCreateCell(worksheet, columnName, rowIndex), formula);
        }
    }
}