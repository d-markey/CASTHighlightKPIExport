using System.Threading.Tasks;

namespace HighlightKPIExport {
    public class HighlightTask<T> {
        // wrapper pour les tâches asynchrones d'appel aux API Highlight
        public HighlightTask(HighlightAppId app, Task<T> task) {
            App = app;
            Task = task;
        }

        public HighlightAppId App { get; private set; }
        public Task<T> Task { get; private set; }

        public bool IsCompleted => Task.IsCompleted;
        public T Result => Task.Result;
    }
}