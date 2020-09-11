using System;
using System.Linq;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Client;
using HighlightKPIExport.KPIs;
using HighlightKPIExport.Audit;

namespace HighlightKPIExport {
    // programme principal
    class Program {

        // point d'entrée CLI
        static async Task Main(string[] args) {
            ILogger logger = new Logger(false);
            var ts = DateTime.Now.Ticks;
            var arguments = new Args();
            try {
                // analyse des arguments
                arguments.Parse(args);
                if (arguments.Help.Value) {
                    // affichage de l'aide
                    Console.WriteLine(arguments.GetUsage());
                } else if (arguments.Symbols.Value) {
                    Console.WriteLine("List of available symbols:");
                    var symbols = AppInfoService.GetSymbolDescriptions();
                    var maxLen = symbols.Values.Max(_ => _.Length);
                    Func<string, string> format = (string x) => x + new string(' ', maxLen - x.Length);
                    foreach (var entry in symbols) {
                        if (string.IsNullOrWhiteSpace(entry.Key) || entry.Key.Contains("*")) {
                            Console.WriteLine();
                            Console.WriteLine($"   === {entry.Value} ===");
                        } else {
                            Console.WriteLine($"   {format(entry.Value)} = {{{entry.Key}}}");
                        }
                    }
                    Console.WriteLine();
                } else {
                    // lancement de l'export Highlight
                    logger = new Logger(arguments.Verbose.Value);
                    var program = new Program();
                    await program.Run(logger, arguments);
                }
            } catch (Exception ex) {
                logger.Error($"An error occurred: {ex.Message}");
            }
            var ticks = DateTime.Now.Ticks - ts;
            logger?.Log($"Finished in {new TimeSpan(ticks)}");
        }
       
        // traitement principal
        async Task Run(ILogger logger, Args args) {
            WebTaskMonitorFactory.MaxConcurrency = args.MaxConcurrency.Value;

            using (var client = new HighlightClient(args.Url.Value)) {
                try {
                    var credSrv = new CredentialService(logger);
                    await client.Authenticate(await credSrv.GetCredential(args));
                } catch (Exception ex) {
                    throw new UnauthorizedAccessException("Authentication failed", ex);
                }

                Exception appInfoServiceException = null;
                try {
                    var appInfoService = new AppInfoService(logger, client, args);
                    await appInfoService.Run();
                } catch (Exception ex) {
                    appInfoServiceException = ex;
                }

                Exception auditServiceException = null;
                try {
                    var auditService = new AuditService(logger, client, args);
                    await auditService.Run();
                } catch (Exception ex) {
                    auditServiceException = ex;
                }

                if (appInfoServiceException != null || auditServiceException != null) {
                    if (appInfoServiceException == null) throw auditServiceException;
                    if (auditServiceException == null) throw appInfoServiceException;
                    throw new AggregateException(appInfoServiceException, auditServiceException);
                }
            }
        }
    }
}