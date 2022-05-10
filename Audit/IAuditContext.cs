using System.Collections.Generic;

namespace HighlightKPIExport.Audit {
    public interface IAuditContext {
        string AuditFile { get; }
        IEnumerable<string> CompanyIds { get; }
    }
}