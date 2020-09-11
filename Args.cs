using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

using HighlightKPIExport.Technical;
using HighlightKPIExport.Audit;
using HighlightKPIExport.KPIs;

namespace HighlightKPIExport {
    // gestion des arguments passés sur la CLI
    public class Args : Arguments, ICredentialConfig, IAppInfoContext, IAuditContext {

        private static class Default {
            public static readonly Uri DefaultBaseUrl = new Uri("https://rpa.casthighlight.com");
            public static readonly string AuditFileName = "highlight_audit_{companyid}_{timestamp}.xlsx";
            public static readonly string ScoreCardTemplateName = "template_scorecard.html";
            public static readonly string CsvTemplateName = "template_csv.txt";
            public static readonly byte MaxConcurrency = WebTaskMonitorFactory.DefaultConcurrency;
        } 

        public Args() {
            Url =            new Option<Uri>        ("url",              "s",    $"base URL of CAST Highlight server; default is {Default.DefaultBaseUrl}", Default.DefaultBaseUrl);
            User =           new Option<MailAddress>("user",             "u",    $"user id (the email address the user registered with)", (MailAddress)null);
            Password =       new Option<string>     ("password",         "p",    $"password; if this is the name of an existing file, the password is read from this file");
            Credentials =    new Option<string>     ("credentials",      "c",    $"credential file name; the file must contain user id and password separated by a colon, e.g. \"me@acme.com:my_Pa$$W0rd\"");
            DomainIds =      new Option<string>     ("domainid",         "d",    $"id of an Highlight domain; mandatory; the company id is also a domain id; multiple occurrences are allowed");
            AppIds =         new Option<string>     ("appid",            "a",    $"id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted");
            Template =       new Option<string>     ("template",         "t",    $"file name of the template file; by default, \"{Default.ScoreCardTemplateName}\" for a single application, \"{Default.CsvTemplateName}\" for multiple applications", () => AppIds.Values.Take(2).Count() == 1 ? Default.ScoreCardTemplateName : Default.CsvTemplateName);
            Output =         new Option<string>     ("output",           "o",    $"file name of the result file; by default, results are displayed on the standard output");
            CompanyIds =     new Option<string>     ("companyid",        "l",    $"id of an Highlight company; optional; multiple occurrences are allowed");
            AuditFile =      new Option<string>     ("auditfile",        "af",   $"file name of the audit file; by default \"{Default.AuditFileName}\"", Default.AuditFileName);
            MaxConcurrency = new Option<byte>       ("maxconcurrency",   "mc",   $"maximum number of concurrent requests; by default {Default.MaxConcurrency}", Default.MaxConcurrency);
            Verbose =        new Option<bool>       ("verbose",          "v",    $"turn verbosity on", false);
            Symbols =        new Option<bool>       ("symbols",          "sl",   $"display list of available symbols", false);
            Help =           new Option<bool>       ("help",             "h",    $"display help information", false);
            fallback =      new Option<string>     ("*", "", "");

            _options = new IArgument[] {
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
                MaxConcurrency,
                Verbose,
                Symbols,
                Help
            };
        }

        // options CLI de l'outil
        private readonly IArgument[] _options;

        protected override IEnumerable<IArgument> Options => _options;

        public readonly Option<Uri> Url;
        public readonly Option<MailAddress> User;
        public readonly Option<string> Password;
        public readonly Option<string> Credentials;
        public readonly Option<string> DomainIds;
        public readonly Option<string> AppIds;
        public readonly Option<string> CompanyIds;
        public readonly Option<string> AuditFile;
        public readonly Option<string> Template;
        public readonly Option<string> Output;
        public readonly Option<byte> MaxConcurrency;
        public readonly Option<bool> Verbose;
        public readonly Option<bool> Symbols;
        public readonly Option<bool> Help;
        private readonly Option<string> fallback;

        protected override IArgument FallbackOption => fallback;

        protected override void HandleUnknownOption(string option) {
            throw new ArgumentException($"Unsupported option \"{option}\"");
        }

        protected override void HandleUnsetOption(string option) {
            throw new ArgumentException($"Missing value for option \"{option}\"");
        }

        // interprétation des arguments passés en ligne de commande
        public override void Parse(string[] args) {
            if (args.Length == 0) {
                Help.SetValue("true");
            } else {
                base.Parse(args);
                if (fallback.Values.Any()) {
                    throw new ArgumentException($"Unsupported argument(s): {string.Join(", ", fallback.Values)}");
                }
            }
        }

        // description de l'utilisation en CLI
        public override string GetUsage() {
            var sb = new StringBuilder();
            sb.AppendLine("HighlightKPIExport - Export Application KPIs and Audit Logs from CAST Highlight");
            sb.AppendLine();
            sb.AppendLine("Usage:");
            sb.AppendLine();
            sb.AppendLine("   dotnet HighlightKPIExport.dll [options]");
            sb.AppendLine();
            sb.AppendLine("Available options:");
            for (var i = 0; i < _options.Length; i++) {
                var opt = _options[i];
                sb.Append("   ");
                opt.AppendUsage(sb).AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }

        string ICredentialConfig.CredentialFileName => Credentials.Value;
        string ICredentialConfig.UserId => User.Value?.Address;
        string ICredentialConfig.Password => Password.Value;

        string IAppInfoContext.TemplateFileName => Template.Value;
        string IAppInfoContext.OutputFileName => Output.Value;
        IEnumerable<string> IAppInfoContext.DomainIds => DomainIds.Values;
        IEnumerable<string> IAppInfoContext.AppIds => AppIds.Values;

        string IAuditContext.AuditFile => AuditFile.Value;
        IEnumerable<string> IAuditContext.CompanyIds => CompanyIds.Values;
    }
}