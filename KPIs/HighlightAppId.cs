using System;
using System.Collections.Generic;
using System.Linq;

namespace HighlightKPIExport.KPIs {
	// Modèle simplifié de l'entité HL pour l'API /domains/{domainId}/applications
    public class HighlightAppId {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DomainId { get; set; }
    }
}
