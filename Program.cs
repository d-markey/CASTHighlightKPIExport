using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HighlightKPIExport {
    // programme principal
    class Program {

        // point d'entrée CLI
        static void Main(string[] args) {
            var ts = DateTime.Now.Ticks;
            var arguments = new Args();
            try {
                // analyse des arguments
                arguments.Parse(args);
                if (arguments.Help.Value == "true") {
                    // affichage de l'aide
                    Console.WriteLine(arguments.GetUsage());
                } else {
                    // lancement de l'export Highlight
                    var program = new Program();
                    program.Run(arguments).GetAwaiter().GetResult();
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("An error occurred: " + ex.Message);
            }
            var ticks = DateTime.Now.Ticks - ts;
            Log(arguments, $"Finished in {new TimeSpan(ticks)}");
        }

        // Logging basique
        static void Log(Args args, string message) {
            if (args.Verbose.Value != null) {
                Console.WriteLine(message);
            }
        }

        private Program() {
            _date = DateTime.Now;
        }

        private DateTime _date;

        // récupération des credentials (userid/mot de passe)
        async Task<NetworkCredential> GetCredential(Args args) {
            var userId = "";
            var password = "";
            // l'option "--credentials" a priorité
            var credentialFileName = args.Credentials.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(credentialFileName)) {
                // fichier contenant les credentials sous la forme "userid:password"
                if (File.Exists(credentialFileName)) {
                    Log(args, $"Loading credentials from file {credentialFileName}");
                    using (var stream = new StreamReader(password)) {
                        var parts = (await stream.ReadToEndAsync()).Split(':');
                        // récupération de l'identifiant
                        userId = parts.Length > 0 ? parts[0].Trim() : "";
                        // récupération du mot de passe
                        password = parts.Length > 1 ? parts[1].Trim() : "";
                    }
                } else {
                    Log(args, $"Credentials file {credentialFileName} not found");
                }
            }
            if (string.IsNullOrWhiteSpace(userId)) {
                // récupération de l'identifiant
                userId = args.User.Value?.Trim();
            }
            if (string.IsNullOrWhiteSpace(password)) {
                // récupération du mot de passe
                password = args.Password.Value?.Trim();
                if (File.Exists(password)) {
                    // récupération du mot de passe à partir du fchier
                    Log(args, $"Loading password from file {password}");
                    using (var stream = new StreamReader(password)) {
                        password = (await stream.ReadToEndAsync()).Trim();
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password)) {
                Log(args, $"Missing credentials");
            }
            return new NetworkCredential(userId, password);
        }

        // génération du fichier de sortie
        async Task WriteResults(Args args, Template template, List<HighlightAppInfo> apps) {
            StreamWriter stream = null;
            Func<StringBuilder, Task> write = null;

            var outputFileName = (args.Output.Value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(outputFileName)) {
                // génération sur la sortie standard
                Log(args, $"Writing results to console");
                Log(args, $"------");
                write = (data) => {
                    Console.WriteLine(data.ToString());
                    return Task.CompletedTask;
                };
            } else {
                // génération dans un fichier
                Log(args, $"Writing results to {outputFileName}");
                stream = new StreamWriter(outputFileName);
                write = (data) => {
                    return stream.WriteLineAsync(data.ToString());
                };
            }

            var symbols = new Dictionary<string, string>();
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
                    await write(appData);
                }
                // génération des résultats
                for (int i = 0; i < apps.Count; i++) {
                    var app = apps[i];
                    symbols.Clear();
                    symbols.Add("#", (i+1).ToString());
                    symbols.Add("Date", _date.ToShortDateString());
                    symbols.Add("Time", _date.ToShortTimeString());
                    app.AppendSymbols(symbols);
                    app.AppendSymbols(symbols, "App");
                    appData.Length = 0;
                    appData.Append(template.Apply(symbols));
                    await write(appData);
                }
            } finally {
                // fermeture du fichier
                if (stream != null) {
                    stream.Dispose();
                }
            }

            if (string.IsNullOrWhiteSpace(outputFileName)) {
                Log(args, $"------");
            }
        }
        
        // chargement du template
        async Task<Template> LoadTemplate(Args args, List<HighlightAppInfo> apps) {
            var templateFile = (args.Template.Value ?? "").Trim();
            if (templateFile.Length == 0) {
                // template par défaut: "ScoreCard" si une seule application concernée, sinon "CSV"
                templateFile = (apps.Count == 1) ? Template.ScoreCard : Template.Csv;
            }
            string format = null;
            if (templateFile.Length > 0) {
                if (File.Exists(templateFile)) {
                    using (var reader = new StreamReader(templateFile)) {
                        format = (await reader.ReadToEndAsync()).Trim();
                    }
                } else {
                    Log(args, $"Template file {templateFile} not found");
                    return null;
                }
            }
            if (string.IsNullOrWhiteSpace(format)) {
                Log(args, $"Template is empty");
                return null;
            }
            return new Template(format);
        }

        // chargement des résultats d'applications
        async Task<List<HighlightAppInfo>> LoadAppInfo(Args args) {
            var highlightAPi = new HighlightClient(args.Url.Value);
            highlightAPi.Credential = await GetCredential(args);

            // identification des applis portées par les domaines spécifiés
            var domainIds = args.DomainIds.Values.ToList();
            Log(args, $"Fetching {domainIds.Count} domain(s) from {highlightAPi.BaseUrl}");
            var allAppIds = new Dictionary<string, HighlightAppId>();
            foreach (var domainId in domainIds) {
                Log(args, $"   Fetching domain {domainId}...");
                foreach (var app in await highlightAPi.GetAppIdsForDomain(domainId)) {
                    if (!allAppIds.ContainsKey(app.Id)) {
                        allAppIds.Add(app.Id, app);
                    }
                }
            }

            // chargement asynchrone des résultats d'analyse des applications
            // si aucun ID d'application n'a été spécifié, toutes les applis des domaines spécifiés seront chargées
            // sinon, uniquement les applications spécifiées seront chargées si elles appartiennent bien aux domaines spécifiés
            var appIds = (args.AppIds.Values.Any() ? args.AppIds.Values : allAppIds.Keys).ToList();
            Log(args, $"Fetching {appIds.Count} applications from {highlightAPi.BaseUrl}");
            var webTaskMonitor = new WebTaskMonitor<HighlightAppInfo>(1);
            foreach (var app in allAppIds.Values) {
                if (appIds.Contains(app.Id)) {
                    Log(args, $"   Fetching application {app.Id} / {app.Name}...");
                    // appel asynchrone des API Highlight
                    webTaskMonitor.Add(new HighlightTask<HighlightAppInfo>(app, highlightAPi.GetAppInfoForApp(app.DomainId, app.Id)));
                }
            }

            // attente de la fin des chargements
            webTaskMonitor.WaitAll();

            // récupération des résultats
            return webTaskMonitor.Results.OrderByDescending(app => app.CurrentMetrics?.BusinessImpact ?? 0).ToList();
        }

        // tâche d'export
        async Task Run(Args args) {
            // 1. chargement des données
            var apps = await LoadAppInfo(args);
            if (apps.Count > 0) {
                // 2. chargement du template
                var template = await LoadTemplate(args, apps);
                if (template != null) {
                    // 3. génération du fichier de sortie
                    await WriteResults(args, template, apps);
                } else {
                    Log(args, "Could not find any template to write results");
                }
            } else {
                Log(args, "Could not find any application to process");
            }
        }
    }
}
