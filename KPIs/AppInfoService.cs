using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Client;
using HighlightKPIExport.Client.DTO;

namespace HighlightKPIExport.KPIs {
    public class AppInfoService {

        public AppInfoService(ILogger logger, HighlightClient client, IAppInfoContext context) {
            _logger = logger;
            _client = client;
            _templateFileName = context.TemplateFileName;
            _outputFileName = context.OutputFileName;
            _domainIds = context.DomainIds.ToArray();
            _appIds = new HashSet<string>(context.AppIds);
        }

        private readonly ILogger _logger;
        private readonly HighlightClient _client;
        private readonly string _templateFileName;
        private readonly string _outputFileName;
        private readonly string[] _domainIds;
        private readonly HashSet<string> _appIds;

        private DateTime _date;

        public async Task Run() {
            // 1. chargement des données du portfolio
            _date = DateTime.Now;
            var apps = await LoadAppInfo();
            if (apps.Count > 0) {
                // 2. chargement du template
                var template = await LoadTemplate(apps);
                if (template != null) {
                    // 3. génération du fichier de sortie
                    await WriteAppInfo(template, apps);
                } else {
                    _logger.Log("Could not find any template to write results");
                }
            }
        }

        // chargement des résultats d'applications
        async Task<List<AppInfo>> LoadAppInfo() {
            if (_domainIds.Length == 0 && _appIds.Count == 0) {
                return new List<AppInfo>();
            }

            // identification des applis portées par les domaines spécifiés
            var allAppIds = new Dictionary<string, AppId>();
            if (_domainIds.Length > 0) {
                _logger.Log($"Fetching {_domainIds.Length} domain(s) from {_client.BaseUrl}");
                for (var i = 0; i < _domainIds.Length; i++) {
                    var domainId = _domainIds[i];
                    _logger.Log($"   Fetching domain {domainId}...");
                    var domainAppIds = await _client.GetAppIdsForDomain(domainId);
                    for (var j = 0; j < domainAppIds.Count; j++) {
                        var app = domainAppIds[j];
                        if (!allAppIds.ContainsKey(app.Id)) {
                            app.DomainId = domainId;
                            allAppIds.Add(app.Id, app);
                        }
                    }
                }
            }

            // chargement asynchrone des résultats d'analyse des applications
            // si aucun ID d'application n'a été spécifié, toutes les applis des domaines spécifiés seront chargées
            // sinon, uniquement les applications spécifiées seront chargées si elles appartiennent bien aux domaines spécifiés
            if (_appIds.Count == 0) {
                _appIds.UnionWith(allAppIds.Keys);
            }

            _logger.Log($"Fetching {_appIds.Count} applications from {_client.BaseUrl}");
            var webTaskMonitor = WebTaskMonitorFactory.Build<AppInfo>(_logger);
            foreach (var app in allAppIds.Values.Where(_ => _appIds.Contains(_.Id))) {
                    // appel asynchrone des API Highlight
                var task = new HighlightAppInfoTask(_client.BaseUrl, app, () => _client.GetAppInfoForApp(app.DomainId, app.Id));
                _logger.Log($"   Starting task #{task.Id} for application {app.Id} / {app.Name}...");
                webTaskMonitor.Add(task);
                }

            // récupération des résultats
            var results = await webTaskMonitor.GetResults();
            return results.OrderByDescending(app => app.CurrentMetrics?.BusinessImpact ?? 0).ToList();
        }

        // chargement du template
        async Task<Template> LoadTemplate(List<AppInfo> apps) {
            if (string.IsNullOrWhiteSpace(_templateFileName)) {
                _logger.Log($"Template file name is empty");
                return null;
            }
            if (!File.Exists(_templateFileName)) {
                _logger.Log($"Template file {_templateFileName} not found");
                return null;
            }
            string format = null;
            using (var reader = new StreamReader(_templateFileName)) {
                format = (await reader.ReadToEndAsync()).Trim();
            }
            if (string.IsNullOrWhiteSpace(format)) {
                _logger.Log($"Template is empty");
                return null;
            }
            return new Template(format);
        }

        public static Dictionary<string, string> GetSymbolDescriptions() {
            var symbols = new Dictionary<string, string>();
            symbols.Add("", "Report information");
            symbols.Add("#", " - Report Line Number");
            symbols.Add("Date", " - Report Date");
            symbols.Add("Time", " - Report Time");
            AppendSymbolDescriptions(symbols);

            return symbols;
        }

        private static void AppendSymbolDescriptions(Dictionary<string, string> symbols, string prefix = null) {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "App.*", "Application information");
            symbols.Add(prefix + "App.Url",   " - Application URL");
            symbols.Add(prefix + "App.Id",    " - Application Id");
            symbols.Add(prefix + "App.Name",  " - Application Name");

            symbols.Add(prefix + "*", "Current analysis results");
            AppendMetricSymbolDescriptions(symbols, prefix + "");
            symbols.Add(prefix + "Current.*", "Current analysis results (synonym)");
            AppendMetricSymbolDescriptions(symbols, prefix + "Current");
            symbols.Add(prefix + "Previous.*", "Previous analysis results");
            AppendMetricSymbolDescriptions(symbols, prefix + "Previous");
            symbols.Add(prefix + "Trend.*", "Trend vs. previous analysis");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend");
            symbols.Add(prefix + "Trend.1-week.*", "Trend vs. previous analysis older than 1 week");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend.1-week");
            symbols.Add(prefix + "Trend.2-weeks.*", "Trend vs. previous analysis older than 2 weeks");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend.2-weeks");
            symbols.Add(prefix + "Trend.3-weeks.*", "Trend vs. previous analysis older than 3 weeks");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend.3-weeks");
            symbols.Add(prefix + "Trend.1-month.*", "Trend vs. previous analysis older than 1 month");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend.1-month");
            symbols.Add(prefix + "Trend.3-months.*", "Trend vs. previous analysis older than 3 months");
            AppendTrendSymbolDescriptions(symbols, prefix + "Trend.3-months");
        }

        private static void AppendMetricSymbolDescriptions(Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "SnapshotLabel",           " - Snapshot Label");
            symbols.Add(prefix + "SnapshotDate",            " - Snapshot Date");
            symbols.Add(prefix + "SnapshotTime",            " - Snapshot Time");
            symbols.Add(prefix + "SoftwareHealth",          " - Software Health Score");
            symbols.Add(prefix + "SoftwareAgility",         " - Software Agility Score");
            symbols.Add(prefix + "SoftwareElegance",        " - Software Elegance Score");
            symbols.Add(prefix + "SoftwareResiliency",      " - Software Resiliency Score");
            symbols.Add(prefix + "OpenSourceSafety",        " - Open-Source Safety Score");
            symbols.Add(prefix + "CloudReady",              " - Cloud-Ready Score");
            symbols.Add(prefix + "CloudReadyScan",          " - Cloud-Ready Scan Score");
            symbols.Add(prefix + "Roadblocks",              " - Number of Roadblocks");
            symbols.Add(prefix + "TotalLinesOfCode",        " - Total Number of Lines of Code");
            symbols.Add(prefix + "TotalFiles",              " - Total Number of Files");
            symbols.Add(prefix + "BackFiredFP",             " - Back-Fired Function Points");
            symbols.Add(prefix + "BusinessImpact",          " - Business Impact Score");
            symbols.Add(prefix + "RoarIndex",               " - Highlight ROAR Index");
            symbols.Add(prefix + "TechnicalDebt",           " - Technical Debt");
            symbols.Add(prefix + "TechnicalDebtDensity",    " - Technical Debt Density");
        }

        private static void AppendTrendSymbolDescriptions(Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "SnapshotLabel",           " - Label of the previous snapshot");
            symbols.Add(prefix + "SnapshotDate",            " - Date of the previous snapshot ");
            symbols.Add(prefix + "SnapshotTime",            " - Time of the previous snapshot");
            symbols.Add(prefix + "SoftwareHealth",          " - Software Health Score Evolution");
            symbols.Add(prefix + "SoftwareAgility",         " - Software Agility Score Evolution");
            symbols.Add(prefix + "SoftwareElegance",        " - Software Elegance Score Evolution");
            symbols.Add(prefix + "SoftwareResiliency",      " - Software Resiliency Score Evolution");
            symbols.Add(prefix + "OpenSourceSafety",        " - Open-Source Safety Score Evolution");
            symbols.Add(prefix + "CloudReady",              " - Cloud-Ready Score Evolution");
            symbols.Add(prefix + "CloudReadyScan",          " - Cloud-Ready Scan Score Evolution");
            symbols.Add(prefix + "Roadblocks",              " - Evolution of Number of Roadblocks");
            symbols.Add(prefix + "TotalLinesOfCode",        " - Evolution of Number of Lines of Code");
            symbols.Add(prefix + "TotalFiles",              " - Evolution of Number of Files");
            symbols.Add(prefix + "BackFiredFP",             " - Back-Fired Function Points Evolution");
            symbols.Add(prefix + "BusinessImpact",          " - Business Impact Evolution");
            symbols.Add(prefix + "RoarIndex",               " - Highlight ROAR Index Evolution");
            symbols.Add(prefix + "TechnicalDebt",           " - Technical Debt Evolution");
            symbols.Add(prefix + "TechnicalDebtDensity",    " - Technical Debt Density Evolution");
        }

        // génération du fichier de sortie
        async Task WriteAppInfo(Template template, List<AppInfo> apps) {
            StreamWriter stream = null;
            Func<string, Task> asyncWrite = null;

            var symbols = new Dictionary<string, string>() { 
                { "timestamp", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") } 
            };
            var outputFileName = Template.ApplySymbols(_outputFileName, symbols);

            if (string.IsNullOrWhiteSpace(outputFileName)) {
                // génération sur la sortie standard
                _logger.Log($"Writing portfolio results to console");
                _logger.Log($"------");
                asyncWrite = (data) => {
                    Console.WriteLine(data);
                    return Task.CompletedTask;
                };
            } else {
                // génération dans un fichier
                _logger.Log($"Writing results to {outputFileName}");
                stream = new StreamWriter(outputFileName);
                asyncWrite = stream.WriteLineAsync;
            }

            symbols.Clear();
            var appData = new StringBuilder();
            try {
                if (apps.Count > 1) {
                    // génération d'un entête si les KPI de plusieurs applications ont été chargés
                    var app = apps[0];
                    symbols.Add("#", "0");
                    symbols.Add("Date", _date.ToShortDateString());
                    symbols.Add("Time", _date.ToShortTimeString());
                    AppendSymbols(app, symbols);
                    AppendSymbols(app, symbols, "App");
                    appData.Length = 0;
                    appData.Append(template.ApplyHeader(symbols));
                    await asyncWrite(appData.ToString());
                }
                // génération des résultats
                for (var i = 0; i < apps.Count; i++) {
                    var app = apps[i];
                    symbols.Clear();
                    symbols.Add("#", (i+1).ToString());
                    symbols.Add("Date", _date.ToShortDateString());
                    symbols.Add("Time", _date.ToShortTimeString());
                    AppendSymbols(app, symbols);
                    AppendSymbols(app, symbols, "App");
                    appData.Length = 0;
                    appData.Append(template.Apply(symbols));
                    await asyncWrite(appData.ToString());
                }
            } finally {
                // fermeture du fichier
                if (stream != null) {
                    stream.Dispose();
                }
            }

            if (string.IsNullOrWhiteSpace(outputFileName)) {
                _logger.Log($"------");
            }
        }

        private static void AppendSymbols(AppInfo app, Dictionary<string, string> symbols, string prefix = null) {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            symbols.Add(prefix + "Url",   app.Url);
            symbols.Add(prefix + "Id",   app.Id);
            symbols.Add(prefix + "Name", app.Name);
            symbols.Add(prefix + "App.Url",   app.Url);
            symbols.Add(prefix + "App.Id",   app.Id);
            symbols.Add(prefix + "App.Name", app.Name);

            AppendMetricSymbols(app.CurrentMetrics, symbols, prefix);
            AppendMetricSymbols(app.CurrentMetrics, symbols, prefix + "Current");
            AppendMetricSymbols(app.PreviousMetrics, symbols, prefix + "Previous");
            AppendMetricSymbols(app.Trend, symbols, prefix + "Trend");
            AppendMetricSymbols(app.TrendOneWeek, symbols, prefix + "Trend.1-week");
            AppendMetricSymbols(app.TrendTwoWeeks, symbols, prefix + "Trend.2-weeks");
            AppendMetricSymbols(app.TrendThreeWeeks, symbols, prefix + "Trend.3-weeks");
            AppendMetricSymbols(app.TrendOneMonth, symbols, prefix + "Trend.1-month");
            AppendMetricSymbols(app.TrendThreeMonths, symbols, prefix + "Trend.3-months");
        }

        // création des symboles utilisables dans les templates + valeurs pour la génération des documents
        private static void AppendMetricSymbols(Metric metrics, Dictionary<string, string> symbols, string prefix = "") {
            prefix = (prefix ?? "").Trim();
            if (prefix.Length > 0) prefix += ".";
            if (metrics == null) {
                symbols.Add(prefix + "SnapshotLabel",           "-");
                symbols.Add(prefix + "SnapshotDate",            "-");
                symbols.Add(prefix + "SnapshotTime",            "-");
                symbols.Add(prefix + "SoftwareHealth",          "-");
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
            } else {
                var sign = prefix.Contains("Trend") ? "'+'" : "";
                symbols.Add(prefix + "SnapshotLabel",           metrics.SnapshotLabel);
                symbols.Add(prefix + "SnapshotDate",            metrics.SnapshotDate.ToShortDateString());
                symbols.Add(prefix + "SnapshotTime",            metrics.SnapshotDate.ToShortTimeString());
                symbols.Add(prefix + "SoftwareHealth",          (metrics.SoftwareHealth * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "SoftwareAgility",         (metrics.SoftwareAgility * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "SoftwareElegance",        (metrics.SoftwareElegance * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "SoftwareResiliency",      (metrics.SoftwareResiliency * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "OpenSourceSafety",        (metrics.OpenSourceSafety * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "CloudReady",              (metrics.CloudReady * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "CloudReadyScan",          (metrics.CloudReadyScan * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "Roadblocks",              metrics.Roadblocks.ToString(sign + "###0;'-'###0"));
                symbols.Add(prefix + "TotalLinesOfCode",        metrics.TotalLinesOfCode.ToString(sign + "###0;'-'###0"));
                symbols.Add(prefix + "TotalFiles",              metrics.TotalFiles.ToString(sign + "###0;'-'###0"));
                symbols.Add(prefix + "BackFiredFP",             metrics.BackFiredFP.ToString(sign + "###0;'-'###0;'-'"));
                symbols.Add(prefix + "BusinessImpact",          (metrics.BusinessImpact * 100).ToString(sign + "###0;'-'###0"));
                symbols.Add(prefix + "RoarIndex",               (metrics.RoarIndex * 100).ToString(sign + "###0.0;'-'###0.0"));
                symbols.Add(prefix + "TechnicalDebt",           metrics.TechnicalDebt.ToString(sign + "###0;'-'###0"));
                symbols.Add(prefix + "TechnicalDebtDensity",    metrics.TechnicalDebtDensity.ToString(sign + "###0.00;'-'###0.00"));
            }
        }
    }
}