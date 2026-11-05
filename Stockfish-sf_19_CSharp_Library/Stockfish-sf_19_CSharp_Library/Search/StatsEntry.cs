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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Type-level replacement for the positive integral template parameter D.</summary>
    public interface IStatsLimit { int Limit { get; } }

    /// <summary>SF19-000103. Inline statistics entry with the original clamped, integer update.</summary>
    public struct StatsEntry<T, TStorage, TLimit>
        where T : struct
        where TStorage : struct, IStatsEntryStorage<T>
        where TLimit : struct, IStatsLimit
    {
        private TStorage entry;
        public static readonly int Bound = ValidateLimit();
        private static int ValidateLimit()
        {
            int d = default(TLimit).Limit;
            if (d <= 0 || d > default(TStorage).Maximum || d > int.MaxValue / d)
                throw new ArgumentOutOfRangeException(nameof(TLimit), "D must satisfy the original StatsEntry static assertions.");
            return d;
        }
        public StatsEntry(T value) { entry = default; _ = Bound; entry.Store(value); }
        public StatsEntry(ref StatsEntry<T, TStorage, TLimit> source) : this(source.Load()) { }
        public T Load() { _ = Bound; return entry.Load(); }
        public void Assign(T value) { _ = Bound; entry.Store(value); }
        public void CopyFrom(ref StatsEntry<T, TStorage, TLimit> source) => Assign(source.Load());

        /// <summary>Adapts mutating operator&lt;&lt; without a C# struct read/copy/write of shared storage.</summary>
        public void Update(int bonus)
        {
            int d = Bound;
            int clamped = Math.Max(-d, Math.Min(d, bonus));
            long current = entry.ToWide(entry.Load());
            // i8/i16 are promoted to int. Preserve signed C++ intermediate widths;
            // reject signed overflow (undefined in C++), rather than silently wrap.
            long product = entry.ArithmeticBits == 32
                ? checked((int)current * Math.Abs(clamped))
                : checked(current * Math.Abs(clamped));
            long updated = entry.ArithmeticBits == 32
                ? checked(checked((int)current + clamped) - (int)(product / d))
                : checked(checked(current + clamped) - product / d);
            entry.Store(entry.Narrow(updated));
            Debug.Assert(Math.Abs(entry.ToWide(entry.Load())) <= d);
        }
    }
}
