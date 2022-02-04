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

namespace HighlightKPIExport.Client.DTO {
    public class AuditLine {
        static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string Guid { get; set; }
        public long date { get; set; }
        public DateTime Date => EPOCH + new TimeSpan(0, 0, (int)(date / 1000));
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string Action { get; set; }
        public string IpSource { get; set; }
        public string Params { get; set; }
    }
}
