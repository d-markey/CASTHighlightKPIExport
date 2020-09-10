using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightKPIExport.Technical {
    // separateur (fausse option CLI permettant d'améliorer la lisibilité de l'aide)
    public class Separator : Option {
        public Separator() : this(string.Empty) {
        }

        public Separator(string label) : base(char.MinValue.ToString(), char.MinValue.ToString(), label) {
        }

        public override StringBuilder AppendUsage(StringBuilder sb) {
            if (!string.IsNullOrWhiteSpace(Description)) {
                sb.AppendLine();
                sb.Append("      *** ").Append(Description.ToUpper()).Append(" ***");
            }
            return sb;
        }
    }
}