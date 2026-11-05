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
using System.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Inject a monotonic millisecond clock before searching; never use wall time for limits.</summary>
    public sealed class MonotonicClock : IMonotonicClock
    {
        public static MonotonicClock Instance { get; } = new MonotonicClock();

        private MonotonicClock()
        {
        }

        // Quotient/remainder avoid multiplying the complete tick count by 1000.
        public TimePoint Now
        {
            get
            {
                long t = Stopwatch.GetTimestamp(), f = Stopwatch.Frequency;
                return checked(t / f * 1000 + t % f * 1000 / f);
            }
        }
    }
}
