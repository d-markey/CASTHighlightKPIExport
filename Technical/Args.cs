using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightKPIExport.Technical {
    // gestion des arguments passés sur la CLI
    public class Args {

        private static class Default {
            public const string DefaultBaseUrl = "https://rpa.casthighlight.com";
            public const string AuditFileName = "highlight_audit_{companyid}_{timestamp}.xlsx";
            public const string ScoreCardTemplateName = "template_scorecard.html";
            public const string CsvTemplateName = "template_csv.txt";
        } 

        public Args() {
            Url =            new Option("url",           "s",    $"base URL of CAST Highlight server; default is {Default.DefaultBaseUrl}", Default.DefaultBaseUrl);
            User =           new Option("user",          "u",    $"user id; typically an email address");
            Password =       new Option("password",      "p",    $"password; if this is the name of an existing file, the password is read from this file");
            Credentials =    new Option("credentials",   "c",    $"credential file name; the file must contain used id and password separated by a colon, e.g. \"me@acme.com:my_Pa$$W0rd\"");
            DomainIds =      new Option("domainid",      "d",    $"id of an Highlight domain; mandatory; multiple occurrences are allowed");
            AppIds =         new Option("appid",         "a",    $"id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted");
            Template =       new Option("template",      "t",    $"file name of the template file; by default, \"{Default.ScoreCardTemplateName}\" for a single application, \"{Default.CsvTemplateName}\" for multiple applications", () => AppIds.Values.Take(2).Count() == 1 ? Default.ScoreCardTemplateName : Default.CsvTemplateName);
            Output =         new Option("output",        "o",    $"file name of the result file; by default, results are displayed on the standard output");
            CompanyIds =     new Option("companyid",     "l",    $"id of an Highlight company; optional; multiple occurrences are allowed");
            AuditFile =      new Option("auditfile",     "af",   $"file name of the audit file; by default \"{Default.AuditFileName}\"", Default.AuditFileName);
            Verbose =        new Option("verbose",       "v",    $"turn verbosity on", true);
            Help =           new Option("help",          "h",    $"display help information", true);
            OtherArgs =      new Option("", "", "");

            _options = new [] {
                new Separator("Connection Information"),
                Url,
                User,
                Password,
                Credentials,
                new Separator("KPI Export"),
                DomainIds,
                AppIds,
                Template,
                Output,
                new Separator("Audit log Export"),
                CompanyIds,
                AuditFile,
                new Separator("Misc."),
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
        public readonly Option OtherArgs;

        // interprétation des arguments passés en ligne de commande
        public void Parse(string[] args) {
            var pos = 0;
            var options = _options.OfType<Option>().ToArray();
            while (pos < args.Length) {
                var opt = args[pos];
                var lc_opt = opt.ToLower();
                Option option = null;
                if (lc_opt.StartsWith("--")) {
                    lc_opt = lc_opt.Substring(2);
                    option = options.FirstOrDefault(_ => _.Name == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else if (lc_opt.StartsWith("-")) {
                    lc_opt = lc_opt.Substring(1);
                    option = options.FirstOrDefault(_ => _.ShortName == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else if (lc_opt.StartsWith("/")) {
                    lc_opt = lc_opt.Substring(1);
                    option = options.FirstOrDefault(_ => _.Name == lc_opt || _.ShortName == lc_opt);
                    if (option != null && !option.Flag)
                        pos++;
                } else {
                    option = OtherArgs;
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
            foreach (var opt in _options.Where(o => o != OtherArgs)) {
                sb.Append("   ");
                opt.AppendUsage(sb).AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}