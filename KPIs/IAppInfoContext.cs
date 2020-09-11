using System.Collections.Generic;

namespace HighlightKPIExport.KPIs {
    public interface IAppInfoContext {
        string TemplateFileName { get; }
        string OutputFileName { get; }
        IEnumerable<string> DomainIds { get; }
        IEnumerable<string> AppIds { get; }
    }
}