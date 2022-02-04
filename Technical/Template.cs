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