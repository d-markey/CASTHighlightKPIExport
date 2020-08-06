using System.Threading.Tasks;

using HighlightKPIExport.Technical;

namespace HighlightKPIExport.Audit {
    public class HighlightAuditTask : HighlightTask<HighlightAudit> {
        // wrapper pour les tâches asynchrones d'appel aux API Highlight
        public HighlightAuditTask(string companyId, Task<HighlightAudit> task) : base(task) {
            CompanyId = companyId;
        }

        public string CompanyId { get; private set; }
 
        public override string Reference => $"{CompanyId}";

        public override HighlightAudit GetResult() {
            var result = base.GetResult();
            result.CompanyId = CompanyId;
            return result;
        }
   }
}