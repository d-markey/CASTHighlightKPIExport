using System.Text;

namespace HighlightKPIExport.Technical {
    // separateur (fausse option CLI permettant d'améliorer la lisibilité de l'aide)
    public class Separator : IArgument {
        public Separator() : this(string.Empty) {
        }

        public Separator(string label){
            Description = label;
        }

        public bool Flag => false;
        public string Name => string.Empty;
        public string ShortName => string.Empty;
        public string Description { get; protected set; }

        public void SetValue(string value) {
        }

        public StringBuilder AppendUsage(StringBuilder sb) {
            if (!string.IsNullOrWhiteSpace(Description)) {
                sb.AppendLine();
                sb.Append("      *** ").Append(Description.ToUpper()).Append(" ***");
            }
            return sb;
        }
    }
}