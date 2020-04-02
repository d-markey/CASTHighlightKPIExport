using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightKPIExport {
    // description d'une option CLI
    public class Option {
        public Option(string name, string shortName, string description, bool flag = false) {
            Name = name;
            ShortName = shortName;
            Flag = flag;
            Description = description;
        }

        public bool Flag { get; protected set; }
        public string Name { get; protected set; }
        public string ShortName { get; protected set; }
        public string Description { get; protected set; }

        protected internal List<string> _values = new List<string>();
        public IEnumerable<string> Values {
            get {
                return _values;
            }
            protected set {
                _values = value.ToList();
            }
        }

        public string Value { get => Values.LastOrDefault(); }

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

    // gestion des arguments passés sur la CLI
    public class Args {

        // options CLI de l'outil
        readonly Option[] _options = new [] {
            new Option("url",           "s",     $"base URL of CAST Highlight server; default is {HighlightKPIExport.HighlightClient.DefaultBaseUrl}"),
            new Option("user",          "u",    $"user id; typically an email address"),
            new Option("password",      "p",    $"password; if this is the name of an existing file, the password is read from this file"),
            new Option("credentials",   "c",    $"credential file name; the file must contain used id and password separated by a colon, e.g. \"me@acme.com:my_Pa$$W0rd\""),
            new Option("domainid",      "d",    $"id of an Highlight domain; mandatory; multiple occurrences are allowed"),
            new Option("appid",         "a",    $"id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted"),
            new Option("template",      "t",    $"file name of the template file; by default, \"{HighlightKPIExport.Template.ScoreCard}\" for a single application, \"{HighlightKPIExport.Template.Csv}\" for multiple applications"),
            new Option("output",        "o",    $"file name of the result file; by default, results are displayed on the standard output"),
            new Option("verbose",       "v",    $"turn verbosity on", true),
            new Option("help",          "h",    $"display help information", true),
        };

        public Option Url => _options[0];
        public Option User => _options[1];
        public Option Password => _options[2];
        public Option Credentials => _options[3];
        public Option DomainIds => _options[4];
        public Option AppIds => _options[5];
        public Option Template => _options[6];
        public Option Output => _options[7];
        public Option Verbose => _options[8];
        public Option Help => _options[9];
 
        public readonly Option Arg = new Option("", "", "");

        // interprétation des arguments passés en ligne de commande
        public void Parse(string[] args) {
            var pos = 0;
            while (pos < args.Length) {
                var opt = args[pos];
                var lc_opt = opt.ToLower();
                Option option = null;
                if (lc_opt.StartsWith("--")) {
                    lc_opt = lc_opt.Substring(2);
                    option = _options.FirstOrDefault(_ => _.Name == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else if (lc_opt.StartsWith("-")) {
                    lc_opt = lc_opt.Substring(1);
                    option = _options.FirstOrDefault(_ => _.ShortName == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else if (lc_opt.StartsWith("/")) {
                    lc_opt = lc_opt.Substring(1);
                    option = _options.FirstOrDefault(_ => _.Name == lc_opt || _.ShortName == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else {
                    option = Arg;
                }
                if (option == null) {
                    Console.WriteLine($"Unknown option {opt}, ignored");
                } else if (option.Flag) {
                    option._values.Add("true");
                } else if (pos < args.Length) {
                    option._values.Add(args[pos]);
                } else {
                    Console.WriteLine($"Missing value for option {opt}, ignored");
                }
                pos++;
            }
        }

        // description de l'utilisation en CLI
        public string GetUsage() {
            var sb = new StringBuilder();
            sb.AppendLine("HighlightKPIExport - Export Application KPIs from CAST Highlight");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine();
            sb.AppendLine("   dotnet HighlightKPIExport.dll [options]");
            sb.AppendLine();
            sb.AppendLine("Available options:");
            foreach (var opt in _options.Where(o => !string.IsNullOrWhiteSpace(o.Description))) {
                sb.Append("   ");
                opt.AppendUsage(sb).AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}