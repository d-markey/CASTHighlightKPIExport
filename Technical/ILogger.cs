namespace HighlightKPIExport.Technical {
    // basic console logger
    public interface ILogger {
        void Log(string message);
        void Error(string message);
    }
}