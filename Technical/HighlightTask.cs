using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    public abstract class HighlightTask<T> {

        public HighlightTask(Task<T> task) {
            Task = task;
        }

        public Task<T> Task { get; private set; }

        public bool IsCompleted => Task.IsCompleted;

        public virtual T GetResult() {
            return Task.Result;
        }

        public abstract string Reference { get; }
    }
}