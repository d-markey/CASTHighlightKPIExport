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


using System.Text;

namespace HighlightKPIExport.Technical {
    // separateur (fausse option CLI permettant d'améliorer la lisibilité de l'aide)
    public class Separator : IArgument {
        public Separator() : this(string.Empty) {
        }

        public Separator(string label){
            Description = label;
        }

        public bool Flag => false;
        public string Name => string.Empty;
        public string ShortName => string.Empty;
        public string Description { get; protected set; }

        public void SetValue(string value) {
        }

        public StringBuilder AppendUsage(StringBuilder sb) {
            if (!string.IsNullOrWhiteSpace(Description)) {
                sb.AppendLine();
                sb.Append("      *** ").Append(Description.ToUpper()).Append(" ***");
            }
            return sb;
        }
    }
}