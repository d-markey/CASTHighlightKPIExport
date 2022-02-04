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

namespace HighlightKPIExport.Technical {
    // basic console logger
    public class Logger : ILogger {
        public Logger(bool enabled) {
            _enabled = enabled;
        }

        private bool _enabled = true;

        public void Log(string message) {
            if (_enabled) {
                Console.WriteLine(message);
            }
        }

        public void Error(string message) {
            Console.Error.WriteLine(message);
        }
    }
}