using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace HighlightKPIExport.Technical {
    public static class Converters {
        private readonly static Dictionary<Type, Func<string, object>> converters = new Dictionary<Type, Func<string, object>>();

        static Converters() {
            converters.Add(typeof(bool), ConvertBool);
            converters.Add(typeof(bool?), ConvertBool);

            converters.Add(typeof(long), ConvertLong);
            converters.Add(typeof(int), ConvertInt);
            converters.Add(typeof(short), ConvertShort);
            converters.Add(typeof(sbyte), ConvertByte);
            converters.Add(typeof(long?), ConvertLong);
            converters.Add(typeof(int?), ConvertInt);
            converters.Add(typeof(short?), ConvertShort);
            converters.Add(typeof(sbyte?), ConvertByte);

            converters.Add(typeof(ulong), ConvertULong);
            converters.Add(typeof(uint), ConvertUInt);
            converters.Add(typeof(ushort), ConvertUShort);
            converters.Add(typeof(byte), ConvertUByte);
            converters.Add(typeof(ulong?), ConvertULong);
            converters.Add(typeof(uint?), ConvertUInt);
            converters.Add(typeof(ushort?), ConvertUShort);
            converters.Add(typeof(byte?), ConvertUByte);

            converters.Add(typeof(string), ConvertString);

            converters.Add(typeof(Uri), ConvertUri);

            converters.Add(typeof(MailAddress), ConvertMailAddress);
        }

        public static T GetValue<T>(string option, string value) {
            if (!converters.TryGetValue(typeof(T), out Func<string, object> converter)) {
                throw new NotSupportedException($"Unsupported conversion to ${typeof(T)}");
            }
            try {
                value = ConvertString(value);
                if (typeof(T).IsSubclassOf(typeof(Nullable<>)) && value.Length == 0) {
                    return default(T);
                }
                return (T)converter(value);
            } catch (Exception ex) {
                throw new ArgumentException($"Invalid value for \"{option}\": {(string.IsNullOrWhiteSpace(value) ? "<Empty>" : value)}", ex);
            }
        }

        private static object ConvertLong(string value) => long.Parse(value);
        private static object ConvertInt(string value) => int.Parse(value);
        private static object ConvertShort(string value) => short.Parse(value);
        private static object ConvertByte(string value) => sbyte.Parse(value);

        private static object ConvertULong(string value) => ulong.Parse(value);
        private static object ConvertUInt(string value) => uint.Parse(value);
        private static object ConvertUShort(string value) => ushort.Parse(value);
        private static object ConvertUByte(string value) => byte.Parse(value);

        private static object ConvertBool(string value) {
            value = value.ToLowerInvariant();
            if (value == "false" || value == "f" || value == "no" || value == "n" || (int.TryParse(value, out int no) && no == 0)) {
                return false;
            }
            if (value == "true" || value == "t" || value == "yes" || value == "y" || (int.TryParse(value, out int yes) && yes != 0)) {
                return true;
            }
            throw new FormatException();
        }

        private static string ConvertString(string value) => (value ?? string.Empty).Trim();
 
        private static Uri ConvertUri(string value) {
            if (!value.Contains("://")) {
                value = "https://" + value; 
            }
            var uri = new Uri(value);
            if (uri.Scheme != "http" && uri.Scheme != "https") {
                throw new FormatException();
            }
            return uri;
        }

        private static MailAddress ConvertMailAddress(string value) {
            return new MailAddress(value);
        }
    }
}