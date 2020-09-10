using System;
using System.Collections.Generic;
using System.Linq;

namespace HighlightKPIExport.KPIs {
    // Modèle de l'entité HL correspondant à un snapshot d'analyse (contient les KPI HL) 
    public class HighlightMetric {
        static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string SnapshotLabel { get; set; }
        public long snapshotDate { get; set; }
        public DateTime SnapshotDate => EPOCH + new TimeSpan(0, 0, (int)(snapshotDate / 1000));
        public double SoftwareAgility { get; set; }
        public double SoftwareElegance { get; set; }
        public double SoftwareResiliency { get; set; }
        public double OpenSourceSafety { get; set; }
        public double CloudReady { get; set; }
        public double CloudReadyScan { get; set; }
        public double Roadblocks { get; set; }
        public int TotalLinesOfCode { get; set; }
        public int TotalFiles { get; set; }
        public double BackFiredFP { get; set; }
        public double BusinessImpact { get; set; }
        public double RoarIndex { get; set; }
        public double TechnicalDebt { get; set; }
        
        // calcul de la densité de dette technique par ligne de code
        private double? _technicalDebtDensity = null;
        public double TechnicalDebtDensity {
            get {
                if (!_technicalDebtDensity.HasValue && TotalLinesOfCode > 0) {
                    _technicalDebtDensity = TechnicalDebt / TotalLinesOfCode;
                }
                return _technicalDebtDensity.HasValue ? _technicalDebtDensity.Value : 0;
            }
            set {
                _technicalDebtDensity = value;
            }
        }

        // création des symboles utilisables dans les templates + valeurs pour la génération des documents
        public void AppendSymbols(Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            var sign = prefix.Contains("Trend") ? "'+'" : "";
            symbols.Add(prefix + "SnapshotLabel",           SnapshotLabel);
            symbols.Add(prefix + "SnapshotDate",            SnapshotDate.ToShortDateString());
            symbols.Add(prefix + "SnapshotTime",            SnapshotDate.ToShortTimeString());
            symbols.Add(prefix + "SoftwareAgility",         (SoftwareAgility * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "SoftwareElegance",        (SoftwareElegance * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "SoftwareResiliency",      (SoftwareResiliency * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "OpenSourceSafety",        (OpenSourceSafety * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "CloudReady",              (CloudReady * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "CloudReadyScan",          (CloudReadyScan * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "Roadblocks",              Roadblocks.ToString(sign + "###0;'-'###0"));
            symbols.Add(prefix + "TotalLinesOfCode",        TotalLinesOfCode.ToString(sign + "###0;'-'###0"));
            symbols.Add(prefix + "TotalFiles",              TotalFiles.ToString(sign + "###0;'-'###0"));
            symbols.Add(prefix + "BackFiredFP",             BackFiredFP.ToString(sign + "###0;'-'###0;'-'"));
            symbols.Add(prefix + "BusinessImpact",          (BusinessImpact * 100).ToString(sign + "###0;'-'###0"));
            symbols.Add(prefix + "RoarIndex",               (RoarIndex * 100).ToString(sign + "###0.0;'-'###0.0"));
            symbols.Add(prefix + "TechnicalDebt",           TechnicalDebt.ToString(sign + "###0;'-'###0"));
            symbols.Add(prefix + "TechnicalDebtDensity",    TechnicalDebtDensity.ToString(sign + "###0.00;'-'###0.00"));
        }

        // création des symboles utilisables dans les templates + valeurs pour la génération des documents
        // (cas où les résultats ne sont pas présents p.ex. pas d'analyse précédente)
        public static void AppendEmptySymbols(Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "SnapshotLabel",           "-");
            symbols.Add(prefix + "SnapshotDate",            "-");
            symbols.Add(prefix + "SnapshotTime",            "-");
            symbols.Add(prefix + "SoftwareAgility",         "-");
            symbols.Add(prefix + "SoftwareElegance",        "-");
            symbols.Add(prefix + "SoftwareResiliency",      "-");
            symbols.Add(prefix + "OpenSourceSafety",        "-");
            symbols.Add(prefix + "CloudReady",              "-");
            symbols.Add(prefix + "CloudReadyScan",          "-");
            symbols.Add(prefix + "Roadblocks",              "-");
            symbols.Add(prefix + "TotalLinesOfCode",        "-");
            symbols.Add(prefix + "TotalFiles",              "-");
            symbols.Add(prefix + "BackFiredFP",             "-");
            symbols.Add(prefix + "BusinessImpact",          "-");
            symbols.Add(prefix + "RoarIndex",               "-");
            symbols.Add(prefix + "TechnicalDebt",           "-");
            symbols.Add(prefix + "TechnicalDebtDensity",    "-");
        }
    }
}
