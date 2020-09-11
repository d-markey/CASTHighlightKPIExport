using System.Collections.Generic;

namespace HighlightKPIExport.Client.DTO {
    public class AuditLog {
        public string CompanyId { get; set; }
        public IList<AuditLine> Result { get; set; }
    }

}
