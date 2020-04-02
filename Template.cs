using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HighlightKPIExport {
    // template
    public class Template {

        public const string Csv = "template_csv.txt";
        public const string ScoreCard = "template_scorecard.html";

        public Template(string format) {
            Format = format;
        }

        public string Format { get; private set; }

        // génération d'un enttête ("{Keyword}" ==> "Keyword")
        public string ApplyHeader(Dictionary<string, string> symbols) {
            var result = new StringBuilder(Format);
            foreach (var entry in symbols) {
                result.Replace("{" + entry.Key + "}", entry.Key);
            }
            return result.ToString();
        } 

        // génération des données ("{Keyword}" ==> "Data")
        public string Apply(Dictionary<string, string> symbols) {
            var result = new StringBuilder(Format);
            foreach (var entry in symbols) {
                result.Replace("{" + entry.Key + "}", entry.Value);
            }
            return result.ToString();
        } 
    }
}