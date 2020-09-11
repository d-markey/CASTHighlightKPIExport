using System;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Client.DTO;

namespace HighlightKPIExport.KPIs {
    public class HighlightAppInfoTask : ScheduledTask<AppInfo> {
        // wrapper pour les tâches asynchrones d'appel aux API Highlight
        public HighlightAppInfoTask(Uri baseUrl, AppId app, Func<Task<AppInfo>> taskBuilder) : base(taskBuilder) {
            App = app;
            BaseUrl = baseUrl;
        }

        public AppId App { get; private set; }
        public Uri BaseUrl { get; private set; }

        public override string Reference => $"{App.Id} / {App.Name}";

        public override AppInfo GetResult() {
            var result = base.GetResult();
            result.Url = $"{BaseUrl}/#Explore/Applications/{App.Id}/Detail";
            return result;
        }
    }
}