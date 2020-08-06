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

        private static class Default {
            public const string DefaultBaseUrl = "https://rpa.casthighlight.com";
            public const string AuditFileName = "highlight_audit_{companyid}_{timestamp}.xlsx";
            public const string ScoreCardTemplateName = "template_scorecard.html";
            public const string CsvTemplateName = "template_csv.txt";
        } 

        public Args() {
            Url =            new Option("url",           "s",     $"base URL of CAST Highlight server; default is {Default.DefaultBaseUrl}", Default.DefaultBaseUrl);
            User =           new Option("user",          "u",    $"user id; typically an email address");
            Password =       new Option("password",      "p",    $"password; if this is the name of an existing file, the password is read from this file");
            Credentials =    new Option("credentials",   "c",    $"credential file name; the file must contain used id and password separated by a colon, e.g. \"me@acme.com:my_Pa$$W0rd\"");
            DomainIds =      new Option("domainid",      "d",    $"id of an Highlight domain; mandatory; multiple occurrences are allowed");
            AppIds =         new Option("appid",         "a",    $"id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted");
            CompanyIds =     new Option("companyid",     "l",    $"id of an Highlight company; optional; multiple occurrences are allowed");
            AuditFile =      new Option("auditfile",     "af",   $"file name of the audit file; by default \"{Default.AuditFileName}\"", Default.AuditFileName);
            Template =       new Option("template",      "t",    $"file name of the template file; by default, \"{Default.ScoreCardTemplateName}\" for a single application, \"{Default.CsvTemplateName}\" for multiple applications", () => AppIds.Values.Take(2).Count() == 1 ? Default.ScoreCardTemplateName : Default.CsvTemplateName);
            Output =         new Option("output",        "o",    $"file name of the result file; by default, results are displayed on the standard output");
            Verbose =        new Option("verbose",       "v",    $"turn verbosity on", true);
            Help =           new Option("help",          "h",    $"display help information", true);
            Arg =            new Option("", "", "");

            _options = new [] {
                Url,
                User,
                Password,
                Credentials,
                DomainIds,
                AppIds,
                CompanyIds,
                AuditFile,
                Template,
                Output,
                Verbose,
                Help
            };
        }

        // options CLI de l'outil
        private readonly Option[] _options;

        public readonly Option Url;
        public readonly Option User;
        public readonly Option Password;
        public readonly Option Credentials;
        public readonly Option DomainIds;
        public readonly Option AppIds;
        public readonly Option CompanyIds;
        public readonly Option AuditFile;
        public readonly Option Template;
        public readonly Option Output;
        public readonly Option Verbose;
        public readonly Option Help;
         public readonly Option Arg;

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