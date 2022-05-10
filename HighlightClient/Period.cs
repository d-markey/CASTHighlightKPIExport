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

namespace HighlightKPIExport.Client {
    public class Period {
        public Period(int days = 0, int months = 0) {
            Days = days;
            Months = months;
        }

        public int Days { get; private set; }
        public int Months { get; private set; }

        public static readonly Period OneWeek = new Period(7);
        public static readonly Period TwoWeeks = new Period(2 * 7);
        public static readonly Period ThreeWeeks = new Period(3 * 7);
        public static readonly Period OneMonth = new Period(0, 1);
        public static readonly Period ThreeMonths = new Period(0, 3);

        public DateTime GetStartDateFrom(DateTime date) => date.AddMonths(-Months).AddDays(-Days);
    }
}