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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>A copyable handle to a live entry, never a snapshot. Writes are not transactions.</summary>
    /// <remarks>Clear preserves handles; Resize/Dispose retire them. Lifecycle changes require quiescence.</remarks>
    public readonly struct TTWriter
    {
        private readonly TranspositionTable.Storage? storage;
        private readonly ulong clusterIndex;
        private readonly int slot;

        internal TTWriter(TranspositionTable.Storage storage, ulong clusterIndex, int slot)
        {
            this.storage = storage;
            this.clusterIndex = clusterIndex;
            this.slot = slot;
        }

        private ref TTEntry Entry
        {
            get
            {
                if (storage == null) throw new InvalidOperationException("An unbound TTWriter has no entry.");
                return ref storage.Entry(clusterIndex, slot);
            }
        }

        public void Write(Key k, Value v, bool pv, Bound b, Depth d, Move m, Value ev, byte generation8)
            { Entry.Save(k, v, pv, b, d, m, ev, generation8); GC.KeepAlive(storage); }

        public void Penalize(int penalty) { Entry.Penalize(penalty); GC.KeepAlive(storage); }
    }
}
