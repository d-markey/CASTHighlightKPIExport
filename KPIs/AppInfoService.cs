using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;

namespace HighlightKPIExport.KPIs {
    public class AppInfoService {

        public AppInfoService(Args args) {
            Args = args;
        }

        private DateTime _date = DateTime.Now;

        public Args Args { get; private set; }

        public async Task Run() {
            // 1. chargement des données du portfolio
            var apps = await LoadAppInfo();
            if (apps.Count > 0) {
                // 2. chargement du template
                var template = await LoadTemplate(apps);
                if (template != null) {
                    // 3. génération du fichier de sortie
                    await WriteAppInfo(template, apps);
                } else {
                    Logger.Log(Args, "Could not find any template to write results");
                }
            }
        }

        // chargement des résultats d'applications
        async Task<List<HighlightAppInfo>> LoadAppInfo() {
            var domainIds = Args.DomainIds.Values.ToList();
            var appIds = Args.AppIds.Values.ToList();
            if (!domainIds.Any() && !appIds.Any()) {
                return new List<HighlightAppInfo>();
            }

            var credentialService = new CredentialService(Args);
            var highlightAPi = HighlightClient.Build(Args);
            highlightAPi.Credential = await credentialService.GetCredential();

            // identification des applis portées par les domaines spécifiés
            var allAppIds = new Dictionary<string, HighlightAppId>();
            if (domainIds.Any()) {
                Logger.Log(Args, $"Fetching {domainIds.Count} domain(s) from {highlightAPi.BaseUrl}");
                foreach (var domainId in domainIds) {
                    Logger.Log(Args, $"   Fetching domain {domainId}...");
                    foreach (var app in await highlightAPi.GetAppIdsForDomain<HighlightAppId>(domainId)) {
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
            if (!appIds.Any()) {
                appIds = allAppIds.Keys.ToList();
            }
            Logger.Log(Args, $"Fetching {appIds.Count} applications from {highlightAPi.BaseUrl}");
            var webTaskMonitor = new WebTaskMonitor<HighlightAppInfo>(1);
            foreach (var app in allAppIds.Values) {
                if (appIds.Contains(app.Id)) {
                    Logger.Log(Args, $"   Fetching application {app.Id} / {app.Name}...");
                    // appel asynchrone des API Highlight
                    webTaskMonitor.Add(new HighlightAppTask(highlightAPi.BaseUrl, app, highlightAPi.GetAppInfoForApp<HighlightAppInfo>(app.DomainId, app.Id)));
                }
            }

            // attente de la fin des chargements
            webTaskMonitor.WaitAll();

            // récupération des résultats
            return webTaskMonitor.Results.OrderByDescending(app => app.CurrentMetrics?.BusinessImpact ?? 0).ToList();
        }

        // chargement du template
        async Task<Template> LoadTemplate(List<HighlightAppInfo> apps) {
            var templateFile = Args.Template.Value;
            if (templateFile.Length == 0) {
                Logger.Log(Args, $"Template file name is empty");
                return null;
            }
            if (!File.Exists(templateFile)) {
                Logger.Log(Args, $"Template file {templateFile} not found");
                return null;
            }
            string format = null;
            using (var reader = new StreamReader(templateFile)) {
                format = (await reader.ReadToEndAsync()).Trim();
            }
            if (string.IsNullOrWhiteSpace(format)) {
                Logger.Log(Args, $"Template is empty");
                return null;
            }
            return new Template(format);
        }

        // génération du fichier de sortie
        async Task WriteAppInfo(Template template, List<HighlightAppInfo> apps) {
            StreamWriter stream = null;
            Func<string, Task> write = null;

            var symbols = new Dictionary<string, string>() { 
                { "timestamp", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") } 
            };
            var outputFileName = Template.ApplySymbols(Args.Output.Value, symbols);

            if (string.IsNullOrWhiteSpace(outputFileName)) {
                // génération sur la sortie standard
                Logger.Log(Args, $"Writing portfolio results to console");
                Logger.Log(Args, $"------");
                write = (data) => {
                    Console.WriteLine(data);
                    return Task.CompletedTask;
                };
            } else {
                // génération dans un fichier
                Logger.Log(Args, $"Writing results to {outputFileName}");
                stream = new StreamWriter(outputFileName);
                write = (data) => {
                    return stream.WriteLineAsync(data);
                };
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
                    app.AppendSymbols(symbols);
                    app.AppendSymbols(symbols, "App");
                    appData.Length = 0;
                    appData.Append(template.ApplyHeader(symbols));
                    await write(appData.ToString());
                }
                // génération des résultats
                for (var i = 0; i < apps.Count; i++) {
                    var app = apps[i];
                    symbols.Clear();
                    symbols.Add("#", (i+1).ToString());
                    symbols.Add("Date", _date.ToShortDateString());
                    symbols.Add("Time", _date.ToShortTimeString());
                    app.AppendSymbols(symbols);
                    app.AppendSymbols(symbols, "App");
                    appData.Length = 0;
                    appData.Append(template.Apply(symbols));
                    await write(appData.ToString());
                }
            } finally {
                // fermeture du fichier
                if (stream != null) {
                    stream.Dispose();
                }
            }

            if (string.IsNullOrWhiteSpace(outputFileName)) {
                Logger.Log(Args, $"------");
            }
        }
    }
}