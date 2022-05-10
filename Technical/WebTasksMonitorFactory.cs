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


namespace HighlightKPIExport.Technical {
    // factory class
    public static class WebTaskMonitorFactory {
        public static readonly byte DefaultConcurrency = 3;
        public static byte MaxConcurrency = DefaultConcurrency;

        public static WebTaskMonitor<T> Build<T>(ILogger logger) {
            return new WebTaskMonitor<T>(logger, MaxConcurrency);
        }
    }
}