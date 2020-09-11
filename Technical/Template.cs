using System;
using System.Collections.Generic;
using System.Linq;

namespace HighlightKPIExport.Technical {
    // template
    public class Template {

        public Template(string format) {
            Format = format;
        }

        public string Format { get; private set; }

        // génération d'un enttête ("{Keyword}" ==> "Keyword")
        public string ApplyHeader(Dictionary<string, string> symbols) {
            return ApplySymbols(Format, symbols.Keys.ToDictionary(_ => _));
        } 

        // génération des données ("{Keyword}" ==> "Data")
        public string Apply(Dictionary<string, string> symbols) {
            return ApplySymbols(Format, symbols);
        }

        // application des symboles
        public static string ApplySymbols(string pattern, Dictionary<string, string> symbols) {
            var result = pattern;
            foreach (var entry in symbols) {
                result = result.Replace("{" + entry.Key + "}", entry.Value, StringComparison.InvariantCultureIgnoreCase);
            }
            return result;
        }
    }
}