using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightKPIExport.Technical {
    // description d'une option CLI
    public class Option<T> : IArgument {
        public Option(string name, string shortName, string description, Func<T> getDefaultValue) {
            Name = name;
            ShortName = shortName;
            Description = description;
            GetDefaultValue = getDefaultValue;
        }

        public Option(string name, string shortName, string description) : this(name, shortName, description, (Func<T>)null) {
        }

        public Option(string name, string shortName, string description, T defaultValue) : this(name, shortName, description, () => defaultValue)  {
        }

        public bool Flag => typeof(T) == typeof(bool) || typeof(T) == typeof(Boolean) || typeof(T) == typeof(Boolean?);

        private string _name;
        public string Name { 
            get { 
                return _name;
            }
            protected set {
                value = (value ?? string.Empty).Trim();
                if (value.Length == 0) throw new InvalidOperationException("Option must have a name.");
                _name = value;
            }
        }
        public string ShortName { get; protected set; }
        public string Description { get; protected set; }
        public Func<T> GetDefaultValue { get; protected set; }

        protected readonly internal List<T> _values = new List<T>();
        public IEnumerable<T> Values {
            get {
                if (_values.Any() || GetDefaultValue == null) return _values;
                return new [] { GetDefaultValue() };
            }
        }

        public T Value {
            get {
                if (Values.Any()) return Values.Last();
                if (GetDefaultValue != null) return GetDefaultValue();
                return default(T);
            }
        }

        public void SetValue(string value) {
            _values.Add(Converters.GetValue<T>("--" + Name, value));
        }

        public StringBuilder AppendUsage(StringBuilder sb) {
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