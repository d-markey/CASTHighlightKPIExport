using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Spreadsheet;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Client;
using HighlightKPIExport.Client.DTO;

namespace HighlightKPIExport.Audit {
    public class AuditService {

        public AuditService(ILogger logger, HighlightClient client, IAuditContext context) {
            _logger = logger;
            _client = client;
            _auditFile = context.AuditFile;
            _companyIds = context.CompanyIds.ToArray();
        }

        private readonly ILogger _logger;
        private readonly HighlightClient _client;
        private readonly string _auditFile;
        private readonly string[] _companyIds;

        private static class SheetNames {
            public const string Graph = "Graph";
            public const string Summary = "Summary {year}";
            public const string AuditTrail = "AuditTrail";
        } 

        public async Task Run() {
            // 1. chargement des données d'audit
            var audits = await LoadAuditInfo();
            // 2. création du fichier de sortie
            if (audits.Count > 0) {
                await WriteAuditInfo(audits);
            }
        }

        async Task<List<AuditLog>> LoadAuditInfo() {
            // chargement asynchrone des traces d'audit des sociétés
            if (_companyIds.Length == 0) {
                return new List<AuditLog>();
            }

            _logger.Log($"Fetching {_companyIds.Length} audit info from {_client.BaseUrl}");
            var webTaskMonitor = WebTaskMonitorFactory.Build<AuditLog>(_logger);
            for (var i = 0; i < _companyIds.Length; i++) {
                var companyId = _companyIds[i];
                // appel asynchrone des API Highlight
                var task = new HighlightAuditTask(companyId, () => _client.GetAuditForCompany(companyId));
                _logger.Log($"   Starting task #{task.Id} for audit info for company {companyId}...");
                webTaskMonitor.Add(task);
            }

            // récupération des résultats
            var results = await webTaskMonitor.GetResults();
            return results.ToList();
        }
 
        // génération du fichier de sortie
        async Task WriteAuditInfo(List<AuditLog> audits) {
            if (audits != null && audits.Count > 0) {
                var guids = new Dictionary<string, string>();
                for (var i = 0; i < audits.Count; i++) {
                    guids.Clear();
                    var audit = audits[i];

                    var symbols = new Dictionary<string, string>() { 
                        { "companyid", audit.CompanyId },
                        { "timestamp", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") } 
                    };
                    var outputFileName = Template.ApplySymbols(_auditFile, symbols);

                    var dirName = Path.GetDirectoryName(outputFileName);
                    if (string.IsNullOrWhiteSpace(dirName)) dirName = ".";
                    outputFileName = Path.GetFullPath(Path.Combine(dirName, outputFileName));

                    if (!File.Exists(outputFileName)) {
                        if (!await ReusePreviousSpreadsheet(outputFileName)) {
                            CreateSpreadSheet(outputFileName);
                        }
                    }
                    _logger.Log($"Saving result to {outputFileName}");

                    using (var spreadSheet = new SpreadSheetFile(outputFileName)) {
                        var auditTrail = CreateAuditTrailSheet(spreadSheet, SheetNames.AuditTrail);
                        var colA = spreadSheet.LoadColumnCells(auditTrail, "A");
                        for (var j = 0; j < colA.Count; j++) {
                            var c = colA[j];
                            var guid = spreadSheet.GetCellValue(c)?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(guid)) {
                                guids.Add(guid, null);
                            }
                        }
                        var maxRow = spreadSheet.GetMaxRow(colA);
                        foreach (var log in audit.Result.OrderBy(_ => _.date)) {
                            if (!guids.ContainsKey(log.Guid)) {
                                maxRow += 1;
                                spreadSheet.SetCellValue(auditTrail, "A", maxRow, log.Guid);
                                spreadSheet.SetCellValue(auditTrail, "B", maxRow, log.Date);
                                spreadSheet.SetCellValue(auditTrail, "C", maxRow, log.UserId);
                                spreadSheet.SetCellValue(auditTrail, "D", maxRow, log.Action);
                                auditTrail.Save();
                            }
                        }

                        spreadSheet.Save();
                    }
                }
            }
        }

        // recherche du précédent fichier d'audit
        // l'API ne renvoie pas tous les évènements à chaque appel, p.ex. les évènements de login ne sont renvoyés que sur le mois en cours
        // il est possible de conserver l'historique en réutilisant le fichier généré précédemment
        async Task<bool> ReusePreviousSpreadsheet(string outputFileName) {
            var baseFileName = Path.GetFileName(_auditFile);
            var fileRegEx = new Regex(baseFileName.Replace(".", "\\.").Replace("{companyid}", "\\d+").Replace("{timestamp}", "(\\d{8})_(\\d{6})"));

            var dir = new DirectoryInfo(Path.GetDirectoryName(outputFileName));
            var pattern = "*" + Path.GetExtension(outputFileName);

            var maxD = -1;
            var maxT = -1;
            var lastFile = string.Empty;
            foreach (var fi in dir.EnumerateFiles(pattern)) {
                var match = fileRegEx.Match(fi.Name);
                if (match.Success && match.Groups.Count == 3) {
                    var d = int.Parse(match.Groups[1].Value);
                    var t = int.Parse(match.Groups[2].Value);
                    if (d > maxD || (d == maxD && t > maxT)) {
                        maxD = d;
                        maxT = t;
                        lastFile = fi.FullName;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(lastFile)) {
                _logger.Log($"Appending result to {lastFile}");
                var bytes = await File.ReadAllBytesAsync(lastFile);
                await File.WriteAllBytesAsync(outputFileName, bytes);
                return true;
            } else {
                return false;
            }
        }

        // création du premier fichier d'audit
        void CreateSpreadSheet(string outputFileName) {
            using (var spreadSheet = new SpreadSheetFile(outputFileName)) {
                CreateGraphSheet(spreadSheet, SheetNames.Graph);
                CreateSummarySheet(spreadSheet, SheetNames.Summary, DateTime.UtcNow.Year);
                CreateAuditTrailSheet(spreadSheet, SheetNames.AuditTrail);
                spreadSheet.Save();
            }
        }

        // création de l'onglet "Graph"
        void CreateGraphSheet(SpreadSheetFile spreadSheet, string name) {
            spreadSheet.FindOrCreateSheet(name);
        }

        private class EventInfo {
            public string Category;
            public string Label;
        }

        // création de l'onglet "Summary" (tableau qui consolide le nombre d'évènements mois par mois)
        void CreateSummarySheet(SpreadSheetFile spreadSheet, string name, int year) {
            name = name.Replace("{year}", year.ToString(), StringComparison.InvariantCultureIgnoreCase);
            if (spreadSheet.FindSheet(name) == null) {
                var summary = spreadSheet.FindOrCreateSheet(name);
                spreadSheet.SetCellValue(summary, "B", 2, "Automation UserId");
                spreadSheet.SetCellValue(summary, "E", 2, "0000");
                spreadSheet.SetCellValue(summary, "B", 3, "Administration UserId");
                spreadSheet.SetCellValue(summary, "E", 3, "0000");
                spreadSheet.SetCellValue(summary, "B", 4, "CAST UserId");
                spreadSheet.SetCellValue(summary, "E", 4, "0000");

                var events = new [] {
                    new EventInfo() { Category = "LOGIN", Label = "Login: Successfull connection" },
                    new EventInfo() { Category = "LOGIN", Label = "Login: Incorrect password" },
                    new EventInfo() { Category = "LOGIN", Label = "Switch Company Context" },
                    new EventInfo() { Category = "LOGIN", Label = "Disconnect" },
                    new EventInfo() { Category = "ADMIN", Label = "Create Domain" },
                    new EventInfo() { Category = "ADMIN", Label = "Delete Domain" },
                    new EventInfo() { Category = "ADMIN", Label = "Create or Update Custom Dashboard" },
                    new EventInfo() { Category = "ADMIN", Label = "Update Computed Indicator" },
                    new EventInfo() { Category = "ADMIN", Label = "Create or Update Tag" },
                    new EventInfo() { Category = "APPLICATION", Label = "Create Application" },
                    new EventInfo() { Category = "APPLICATION", Label = "Update Application" },
                    new EventInfo() { Category = "APPLICATION", Label = "Add Tag to application" },
                    new EventInfo() { Category = "APPLICATION", Label = "Remove Tag from application" },
                    new EventInfo() { Category = "APPLICATION", Label = "Delete Application" },
                    new EventInfo() { Category = "CAMPAIGN", Label = "Create Campaign" },
                    new EventInfo() { Category = "CAMPAIGN", Label = "Update Campaign" },
                    new EventInfo() { Category = "RESULT", Label = "Upload Files" },
                    new EventInfo() { Category = "RESULT", Label = "Remove Result Files" },
                    new EventInfo() { Category = "RESULT", Label = "Create Result" },
                    new EventInfo() { Category = "RESULT", Label = "Update Result" },
                    new EventInfo() { Category = "RESULT", Label = "Update Survey Answers" },
                    new EventInfo() { Category = "USER", Label = "Create User" },
                    new EventInfo() { Category = "USER", Label = "Update User" },
                    new EventInfo() { Category = "USER", Label = "Update User Password" },
                };

                var prevCat = string.Empty;
                for (var i = 0; i < events.Length; i++) {
                    var eventInfo = events[i];
                    if (eventInfo.Category != prevCat) {
                        var col = spreadSheet.GetColumnName(7 + i);
                        spreadSheet.SetCellValue(summary, col, 5, eventInfo.Category);
                        prevCat = eventInfo.Category;
                    }
                }

                spreadSheet.SetCellValue(summary, "B", 6, "Année");
                spreadSheet.SetCellValue(summary, "C", 6, "Mois");
                spreadSheet.SetCellValue(summary, "D", 6, "Début");
                spreadSheet.SetCellValue(summary, "E", 6, "Fin");
                spreadSheet.SetCellValue(summary, "F", 6, "TOTAL");
                for (var i = 0; i < events.Length; i++) {
                    var eventInfo = events[i];
                    var col = spreadSheet.GetColumnName(7 + i);
                    spreadSheet.SetCellValue(summary, col, 6, eventInfo.Label);
                }

                for (uint month = 1; month <= 12; month++) {
                    var row = 6 + month;
                    spreadSheet.SetCellValue(summary, "B", row, year);
                    spreadSheet.SetCellValue(summary, "C", row, month);
                    spreadSheet.SetCellFormula(summary, "D", row, $"DATE(B{row},C{row},1)");
                    spreadSheet.SetCellFormula(summary, "E", row, $"DATE(B{row},C{row}+1,1)");
                    spreadSheet.SetCellFormula(summary, "F", row, $"SUM(G{row}:AC{row})");
                    for (var i = 0; i < events.Length; i++) {
                        var eventInfo = events[i];
                        var col = spreadSheet.GetColumnName(7 + i);
                        spreadSheet.SetCellFormula(summary, col, row, $@"COUNTIFS({SheetNames.AuditTrail}!$B:$B,"">=""&$D{row},{SheetNames.AuditTrail}!$B:$B,""<""&$E{row},{SheetNames.AuditTrail}!$D:$D,{col}$6,{SheetNames.AuditTrail}!$C:$C,""<>""&$E$2,{SheetNames.AuditTrail}!$C:$C,""<>""&$E$3,{SheetNames.AuditTrail}!$C:$C,""<>""&$E$4)");
                    }
                }

                spreadSheet.SetCellFormula(summary, "F", 19, $"SUM(F7:F18)");
                for (var i = 0; i < events.Length; i++) {
                    var eventInfo = events[i];
                    var col = spreadSheet.GetColumnName(7 + i);
                    spreadSheet.SetCellFormula(summary, col, 19, $"SUM({col}7:{col}18)");
                }
            }
        }

        // création de l'onglet AuditTrail
        // cet onglet est utilisé pour stocker l'historique
        Worksheet CreateAuditTrailSheet(SpreadSheetFile spreadSheet, string name) {
            var auditTrail = spreadSheet.FindSheet(name);
            if (auditTrail == null) {
                auditTrail = spreadSheet.FindOrCreateSheet(name);
                spreadSheet.SetCellValue(auditTrail, "A", 1, "Event ID");
                spreadSheet.SetCellValue(auditTrail, "B", 1, "TimeStamp (UTC)");
                spreadSheet.SetCellValue(auditTrail, "C", 1, "User Id");
                spreadSheet.SetCellValue(auditTrail, "D", 1, "Action");
            }
            return auditTrail;
        }
    }
}