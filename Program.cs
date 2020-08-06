using System;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;
using HighlightKPIExport.KPIs;
using HighlightKPIExport.Audit;

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
            Logger.Log(arguments, $"Finished in {new TimeSpan(ticks)}");
        }
       
        // traitement principal
        async Task Run(Args args) {
            try {
                var appService = new AppInfoService(args);
                await appService.Run();
            } catch (Exception ex) {
                Logger.Log(args, $"An error occurred while loading app info: {ex.Message}");
            }

            try {
                var auditService = new AuditService(args);
                await auditService.Run();
            } catch (Exception ex) {
                Logger.Log(args, $"An error occurred while loading audit info: {ex.Message}");
            }
        }
    }
}
