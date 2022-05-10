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
using System.Threading;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    public abstract class ScheduledTask<T> {

        private static int SEQUENCE = 0;

        public ScheduledTask(Func<Task<T>> taskBuilder) {
            Id = Interlocked.Increment(ref SEQUENCE);
            _taskBuilder = taskBuilder;
        }

        private Func<Task<T>> _taskBuilder;

        public Task<T> Task { get; private set; }
        public int Id { get; private set; }

        public bool IsStarted => Task != null;

        public bool IsCompleted => Task?.IsCompleted ?? false;

        public void Start() {
            if (Task == null) {
                Task = _taskBuilder();
                _taskBuilder = null;
            }
        }

        public virtual T GetResult() {
            return Task.Result;
        }

        public abstract string Reference { get; }
    }
}