using System;
using System.Threading.Tasks;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Client.DTO;

namespace HighlightKPIExport.Audit {
    public class HighlightAuditTask : ScheduledTask<AuditLog> {
        // wrapper pour les tâches asynchrones d'appel aux API Highlight
        public HighlightAuditTask(string companyId, Func<Task<AuditLog>> taskBuilder) : base(taskBuilder) {
            CompanyId = companyId;
        }

        public string CompanyId { get; private set; }
 
        public override string Reference => $"{CompanyId}";

        public override AuditLog GetResult() {
            var result = base.GetResult();
            result.CompanyId = CompanyId;
            for (var i = 0; i < result.Result.Count; i++) {
                var log = result.Result[i];
                if (log.Action.StartsWith("Login: Incorrect password")) {
                    log.Action = "Login: Incorrect password";
                }
            }
            return result;
        }
   }
}