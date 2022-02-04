// HighlightKPIExport
// Copyright (C) 2020-2022 David MARKEY

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


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