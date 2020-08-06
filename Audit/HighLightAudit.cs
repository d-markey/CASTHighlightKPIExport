using System;
using System.Collections.Generic;

namespace HighlightKPIExport {
    public class HighlightAudit {
        public string CompanyId { get; set; }
        public IList<HighlightAuditLine> Result { get; set; }
    }

    public class HighlightAuditLine {
        static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string Guid { get; set; }
        public long date { get; set; }
        public DateTime Date => EPOCH + new TimeSpan(0, 0, (int)(date / 1000));
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string Action { get; set; }
        public string IpSource { get; set; }
        public string Params { get; set; }
    }
}
