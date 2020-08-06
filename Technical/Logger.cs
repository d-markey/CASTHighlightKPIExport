using System;

namespace HighlightKPIExport.Technical {
    // basic console logger
    public class Logger {
        public static void Log(Args args, string message) {
            if (args.Verbose.Value != null) {
                Console.WriteLine(message);
            }
        }
    }
}