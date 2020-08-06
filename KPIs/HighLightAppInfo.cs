using System;
using System.Collections.Generic;
using System.Linq;

namespace HighlightKPIExport.KPIs {
	// Modèle simplifié de l'entité HL pour l'API /domains/{domainId}/applications
    public class HighlightAppId {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DomainId { get; set; }
    }

	// Modèle de l'entité HL pour l'API /domains/{domainId}/applications/{appId}
    public class HighlightAppInfo {
        public string Url { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public IList<HighlightMetric> Metrics { get; set; }

        // création des symboles utilisables dans les templates + valeurs pour la génération des documents
        public void AppendSymbols(Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "Url",   Url);
            symbols.Add(prefix + "Id",   Id);
            symbols.Add(prefix + "Name", Name);

            var metrics = CurrentMetrics;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix);
                metrics.AppendSymbols(symbols, prefix + "Current");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix);
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Current");
            }
            metrics = PreviousMetrics;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Previous");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Previous");
            }
            metrics = Trend;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Trend");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Trend");
            }
            metrics = TrendTwoWeeks;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Trend.2-weeks");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Trend.2-weeks");
            }
            metrics = TrendThreeWeeks;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Trend.3-weeks");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Trend.3-weeks");
            }
            metrics = TrendOneMonth;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Trend.1-month");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Trend.1-month");
            }
            metrics = TrendThreeMonths;
            if (metrics != null) {
                metrics.AppendSymbols(symbols, prefix + "Trend.3-months");
            } else {
                HighlightMetric.AppendEmptySymbols(symbols, prefix + "Trend.3-months");
            }
        }

        public HighlightMetric CurrentMetrics => (Metrics != null && Metrics.Count > 0) ? Metrics[0] : null;
        public HighlightMetric PreviousMetrics => (Metrics != null && Metrics.Count > 1) ? Metrics[1] : null;

        // Calcul des évolutions par rapport à l'analyse précédente 
        public HighlightMetric Trend { 
            get {
                var current = CurrentMetrics;
                if (current == null) {
                    return null; 
                }
                var previous = PreviousMetrics;
                return ComputeTrend(current, previous);
            }
        }

        public HighlightMetric TrendTwoWeeks { 
            get {
                var current = CurrentMetrics;
                if (current == null) {
                    return null; 
                }
                var twoWeeksAgo = current.SnapshotDate.AddDays(-2 * 7);
                return GetTrendForPeriod(current, twoWeeksAgo);
            }
        }

        public HighlightMetric TrendThreeWeeks { 
            get {
                var current = CurrentMetrics;
                if (current == null) {
                    return null; 
                }
                var threeWeeksAgo = current.SnapshotDate.AddDays(-3 * 7);
                return GetTrendForPeriod(current, threeWeeksAgo);
            }
        }

        public HighlightMetric TrendOneMonth { 
            get {
                var current = CurrentMetrics;
                if (current == null) {
                    return null; 
                }
                var oneMonthAgo = current.SnapshotDate.AddMonths(-1);
                return GetTrendForPeriod(current, oneMonthAgo);
            }
        }

        public HighlightMetric TrendThreeMonths { 
            get {
                var current = CurrentMetrics;
                if (current == null) {
                    return null; 
                }
                var threeMonthsAgo = current.SnapshotDate.AddMonths(-3);
                return GetTrendForPeriod(current, threeMonthsAgo);
            }
        }

        private HighlightMetric GetTrendForPeriod(HighlightMetric current, DateTime previousDate) {
            var previous = Metrics.Where(_ => _.SnapshotDate <= previousDate).OrderByDescending(_ => _.SnapshotDate).FirstOrDefault();
            return ComputeTrend(current, previous);
        }

        private HighlightMetric ComputeTrend(HighlightMetric current, HighlightMetric previous) {
            if (previous == null) {
                return null; 
            }
            var trend = new HighlightMetric();
            trend.SnapshotLabel = previous.SnapshotLabel;
            trend.snapshotDate = previous.snapshotDate;
            trend.SoftwareAgility = current.SoftwareAgility - previous.SoftwareAgility;
            trend.SoftwareElegance = current.SoftwareElegance - previous.SoftwareElegance;
            trend.SoftwareResiliency = current.SoftwareResiliency - previous.SoftwareResiliency;
            trend.OpenSourceSafety = current.OpenSourceSafety - previous.OpenSourceSafety;
            trend.CloudReady = current.CloudReady - previous.CloudReady;
            trend.CloudReadyScan = current.CloudReadyScan - previous.CloudReadyScan;
            trend.BusinessImpact = current.BusinessImpact - previous.BusinessImpact;
            trend.RoarIndex = current.RoarIndex - previous.RoarIndex;
            trend.Roadblocks = current.Roadblocks - previous.Roadblocks;
            trend.TotalLinesOfCode = current.TotalLinesOfCode - previous.TotalLinesOfCode;
            trend.TotalFiles = current.TotalFiles - previous.TotalFiles;
            trend.BackFiredFP = current.BackFiredFP - previous.BackFiredFP;
            trend.TechnicalDebt = current.TechnicalDebt - previous.TechnicalDebt;
            trend.TechnicalDebtDensity = current.TechnicalDebtDensity - previous.TechnicalDebtDensity;
            return trend;
        }
    }

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
