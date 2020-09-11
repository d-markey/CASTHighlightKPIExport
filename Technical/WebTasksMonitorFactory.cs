namespace HighlightKPIExport.Technical {
    // factory class
    public static class WebTaskMonitorFactory {
        public static readonly byte DefaultConcurrency = 3;
        public static byte MaxConcurrency = DefaultConcurrency;

        public static WebTaskMonitor<T> Build<T>(ILogger logger) {
            return new WebTaskMonitor<T>(logger, MaxConcurrency);
        }
    }
}