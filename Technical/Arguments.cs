using System;
using System.Linq;
using System.Collections.Generic;

namespace HighlightKPIExport.Technical {
    public abstract class Arguments {
        protected abstract IEnumerable<IArgument> Options { get; }
        protected abstract IArgument FallbackOption { get; }

        protected abstract void HandleUnknownOption(string option);
        protected abstract void HandleUnsetOption(string option);

        public abstract string GetUsage();

        public virtual void Parse(string[] args) {
            var pos = 0;
            var options = Options.OfType<IArgument>().ToArray();
            while (pos < args.Length) {
                var opt = args[pos];
                var lc_opt = opt.ToLower();
                IArgument option = null;
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
                    option = FallbackOption;
                }
                if (option == null) {
                    HandleUnknownOption(opt);
                } else if (option.Flag) {
                    option.SetValue("true");
                } else if (pos < args.Length) {
                    option.SetValue(args[pos]);
                } else {
                    HandleUnsetOption(opt);
                }
                pos++;
            }
        }
    }
}