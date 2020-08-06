using System.Threading.Tasks;

using HighlightKPIExport.Technical;

namespace HighlightKPIExport.KPIs {
    public class HighlightAppTask : HighlightTask<HighlightAppInfo> {
        // wrapper pour les tâches asynchrones d'appel aux API Highlight
        public HighlightAppTask(string baseUrl, HighlightAppId app, Task<HighlightAppInfo> task) : base(task) {
            App = app;
            BaseUrl = baseUrl;
        }

        public HighlightAppId App { get; private set; }
        public string BaseUrl { get; private set; }

        public override string Reference => $"{App.Id} / {App.Name}";

        public override HighlightAppInfo GetResult() {
            var result = base.GetResult();
            result.Url = $"{BaseUrl}/#Explore/Applications/{App.Id}/Detail";
            return result;
        }
    }
}