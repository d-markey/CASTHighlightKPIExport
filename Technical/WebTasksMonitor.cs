using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    // moniteur des tâches d'appel aux API Highlight
    public class WebTaskMonitor<T> {

        public WebTaskMonitor(int maxRunning) {
            MaxRunning = maxRunning;
        }

        private List<HighlightTask<T>> _tasks = new List<HighlightTask<T>>();
        private List<T> _results = new List<T>();

        public int MaxRunning { get; private set; }
        public IEnumerable<T> Results => _results;

        // récupération des résultats pour les tâches terminées
        private void Harvest() {
            var i = _tasks.Count - 1;
            while (i >= 0) {
                var task = _tasks[i];
                if (task.IsCompleted) {
                    T result;
                    try {
                        result = task.GetResult();
                        _results.Add(result);
                    } catch (Exception ex) {
                        Console.WriteLine($"      Task failed for {task.Reference} : {ex.Message}");
                    }
                    _tasks.RemoveAt(i);
                }
                i--;
            }
        }

        // gestion d'une nouvelle tâche
        public void Add(HighlightTask<T> task) {
            _tasks.Add(task);
            // gestion de la concurrence: si le nombre de tâches lancées en parallèle est supérieur au seuil, mise en attente jusqu'à ce qu'une tâche au moins ait fini
            // le firewall des API Highlight blackliste les adresses IP qui envoie des rafales de requêtes
            while (_tasks.Count > MaxRunning) {
                Task.WaitAny(_tasks.Select(t => t.Task).ToArray());
                Harvest();
            }
        }

        // attente de la fin des tâches d'appel aux API Highlight
        public void WaitAll() {
            while (_tasks.Count > 0) {
                Task.WaitAny(_tasks.Select(t => t.Task).ToArray());
                Harvest();
            }
        }
    }
}