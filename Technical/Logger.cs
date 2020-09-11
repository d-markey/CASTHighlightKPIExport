using System;

namespace HighlightKPIExport.Technical {
    // basic console logger
    public class Logger : ILogger {
        public Logger(bool enabled) {
            _enabled = enabled;
        }

        private bool _enabled = true;

        public void Log(string message) {
            if (_enabled) {
                Console.WriteLine(message);
            }
        }

        public void Error(string message) {
            Console.Error.WriteLine(message);
        }
    }
}