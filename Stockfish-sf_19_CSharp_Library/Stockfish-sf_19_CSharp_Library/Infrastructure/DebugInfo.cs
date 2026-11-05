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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Individually atomic i64 fields; snapshots/copies are not transactions.</summary>
    public class DebugInfo
    {
        private readonly long[] data;
        public DebugInfo(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            data = new long[size];
        }

        public int Count => data.Length;

        public long this[int index] { get => Interlocked.Read(ref data[index]); set => Interlocked.Exchange(ref data[index], value); }

        public void Add(int index, long value) => Interlocked.Add(ref data[index], value);
        public void Maximum(int index, long value)
        {
            long current = this[index];
            while (current < value)
            {
                long observed = Interlocked.CompareExchange(ref data[index], value, current);
                if (observed == current)
                    break;
                current = observed;
            }
        }

        public void Minimum(int index, long value)
        {
            long current = this[index];
            while (current > value)
            {
                long observed = Interlocked.CompareExchange(ref data[index], value, current);
                if (observed == current)
                    break;
                current = observed;
            }
        }

        public void CopyFrom(DebugInfo other)
        {
            if (other == null || other.Count != Count)
                throw new ArgumentException("Mismatched debug shape.", nameof(other));
            for (int i = 0; i < Count; ++i)
                this[i] = other[i];
        }

        public virtual void Clear()
        {
            for (int i = 0; i < Count; ++i)
                this[i] = 0;
        }
    }
}
