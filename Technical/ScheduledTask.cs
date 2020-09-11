using System;
using System.Threading;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    public abstract class ScheduledTask<T> {

        private static int SEQUENCE = 0;

        public ScheduledTask(Func<Task<T>> taskBuilder) {
            Id = Interlocked.Increment(ref SEQUENCE);
            _taskBuilder = taskBuilder;
        }

        private Func<Task<T>> _taskBuilder;

        public Task<T> Task { get; private set; }
        public int Id { get; private set; }

        public bool IsStarted => Task != null;

        public bool IsCompleted => Task?.IsCompleted ?? false;

        public void Start() {
            if (Task == null) {
                Task = _taskBuilder();
                _taskBuilder = null;
            }
        }

        public virtual T GetResult() {
            return Task.Result;
        }

        public abstract string Reference { get; }
    }
}