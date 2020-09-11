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