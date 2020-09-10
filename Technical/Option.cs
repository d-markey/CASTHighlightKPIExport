using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightKPIExport.Technical {
    // description d'une option CLI
    public class Option {
        public Option(string name, string shortName, string description, bool flag, Func<string> getDefaultValue) {
            Name = name;
            ShortName = shortName;
            Flag = flag;
            Description = description;
            GetDefaultValue = getDefaultValue;
        }

        public Option(string name, string shortName, string description) : this(name, shortName, description, false, null) {
        }

        public Option(string name, string shortName, string description, bool flag) : this(name, shortName, description, flag, () => "false")  {
        }

        public Option(string name, string shortName, string description, string defaultValue) : this(name, shortName, description, false, () => defaultValue)  {
        }

        public Option(string name, string shortName, string description, Func<string> getDefaultValue) : this(name, shortName, description, false, getDefaultValue)  {
        }

        public bool Flag { get; protected set; }
        public string Name { get; protected set; }
        public string ShortName { get; protected set; }
        public string Description { get; protected set; }
        public Func<string> GetDefaultValue { get; protected set; }

        protected readonly internal List<string> _values = new List<string>();
        public IEnumerable<string> Values {
            get {
                if (_values.Any() || GetDefaultValue == null) return _values;
                return new [] { GetDefaultValue() };
            }
        }

        public string Value {
            get {
                if (Values.Any()) return Values.Last();
                if (GetDefaultValue != null) return GetDefaultValue();
                return string.Empty;
            }
        }

        public virtual StringBuilder AppendUsage(StringBuilder sb) {
            sb.Append("--").Append(Name);
            if (!Flag) {
                sb.Append(" [value]");
            }
            if (!string.IsNullOrWhiteSpace(ShortName)) {
                sb.Append(", -").Append(ShortName);
                if (!Flag) {
                    sb.Append(" [value]");
                }
            }
            sb.Append(": ").Append(Description);
            return sb;
        }
    }
}