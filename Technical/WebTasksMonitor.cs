using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    // moniteur des tâches d'appel aux API Highlight
    public class WebTaskMonitor<T> {

        public WebTaskMonitor(ILogger logger, int maxConcurrency) {
            _logger = logger;
            MaxConcurrency = maxConcurrency;
        }

        public int MaxConcurrency { get; private set; }

        private ILogger _logger;
        private List<ScheduledTask<T>> _tasks = new List<ScheduledTask<T>>();
        private List<T> _results = new List<T>();
        private bool _locked = false;

        // prise en charge d'une nouvelle tâche
        public void Add(ScheduledTask<T> task) {
            if (_locked) throw new InvalidOperationException();
            _tasks.Add(task);
        }

        private void LoadResults(IEnumerable<ScheduledTask<T>> completedTasks) {
            var completed = completedTasks.ToArray();
            for (var i = 0; i < completed.Length; i++) {
                var task = completed[i];
                _tasks.Remove(task);
                    try {
                    _results.Add(task.GetResult());
                    _logger.Log($"      Task #{task.Id} completed");
                    } catch (Exception ex) {
                    _logger.Log($"      Task #{task.Id} failed for {task.Reference} : {ex.Message}");
                    }
                }
            }

        // planification des tâches d'appel aux API Highlight et récupération des résultats
        public async Task<IEnumerable<T>> GetResults() {
            _locked = true;
            var waitingTasks = _tasks.Where(_ => !_.IsStarted);
            var runningTasks = _tasks.Where(_ => _.IsStarted).Select(_ => _.Task);
            var activeTasks = _tasks.Where(_ => _.IsStarted).Select(_ => _.Task).Where(_ => !_.IsCompleted);
            var completedTasks = _tasks.Where(_ => _.IsCompleted);

            // planification à concurrence de MaxConcurrency tâches actives à la fois
            while (waitingTasks.Any()) {
                var availableSlots = MaxConcurrency - activeTasks.Count();
                if (availableSlots > 0) {
                    foreach (var task in waitingTasks.Take(availableSlots)) {
                        task.Start();
        }

            }
                await Task.WhenAny(runningTasks);
                LoadResults(completedTasks);
        }

            // attente de la fin des tâches
            await Task.WhenAll(runningTasks);
            LoadResults(completedTasks);
            return _results;
        }
    }
}