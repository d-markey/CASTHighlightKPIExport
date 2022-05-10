// HighlightKPIExport
// Copyright (C) 2020-2022 David MARKEY

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


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