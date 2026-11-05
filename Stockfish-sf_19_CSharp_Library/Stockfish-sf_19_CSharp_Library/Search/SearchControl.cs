/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS file)

  Stockfish is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  Stockfish is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Threading;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Concurrent stop signal and callbacks for fixed-depth or managed iterative search.</summary>
    /// <remarks>RequestStop/IsStopped and the internal depth signal are concurrent. Configure hooks before starting.
    /// Only the main worker invokes the SearchManager hooks; the pool shares stop/depth state.</remarks>
    public sealed class SearchControl
    {
        private int stopped, increaseDepth = 1;
        internal bool IncreaseDepth
        {
            get => Volatile.Read(ref increaseDepth) != 0;
            set => Volatile.Write(ref increaseDepth, value ? 1 : 0);
        }
        public bool IsStopped => Volatile.Read(ref stopped) != 0;

        private readonly object gate = new object();
        public void RequestStop() { Volatile.Write(ref stopped, 1); Wake(); }
        internal void Wake() { lock (gate) System.Threading.Monitor.PulseAll(gate); }
        internal void WaitWhile(Func<bool> predicate) { lock (gate) while (!IsStopped && predicate()) System.Threading.Monitor.Wait(gate); }
        public Action<Worker>? CheckTime { get; set; }
        public Action<int, Types.Move, int>? OnIteration { get; set; }

        internal void Check(Worker worker) => CheckTime?.Invoke(worker);
    }
}
