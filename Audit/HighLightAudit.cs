using System;
using System.Collections.Generic;

namespace HighlightKPIExport {
    public class HighlightAudit {
        public string CompanyId { get; set; }
        public IList<HighlightAuditLine> Result { get; set; }
    }

}
