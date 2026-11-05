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
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Nominal DynStats of 16x64 atomic short buckets, D=8192, 8192 buckets per thread.</summary>
    public sealed class PawnHistory
    {
        public struct Policy : IDynStatsPolicy<PawnHistoryEntry>
        {
            public int SizeMultiplier => HistoryConstants.PAWN_HISTORY_BASE_SIZE;
            public void Initialize(ref PawnHistoryEntry item) { } // CLR zero == native value initialization.
            public void Fill(ref PawnHistoryEntry item, int value) { short narrowed = unchecked((short)value); item.Fill(in narrowed); }
        }
        private readonly DynStats<PawnHistoryEntry, Policy> values;
        public PawnHistory(UIntPtr threads) { values = new DynStats<PawnHistoryEntry, Policy>(threads); }
        public PawnHistory(int threads) { values = new DynStats<PawnHistoryEntry, Policy>(threads); }
        public int Count => values.Count;
        public UIntPtr GetSize() => values.GetSize();
        public ref PawnHistoryEntry this[int index] => ref values[index];
        public ref PawnHistoryEntry this[UIntPtr index] => ref values[index];
        public void ClearRange(int value, UIntPtr threadIdx, UIntPtr numaTotal) => values.ClearRange(value, threadIdx, numaTotal);
        public ReadOnlyEntry ReadOnlyAt(UIntPtr index)
        { _ = values[index].Count; return new ReadOnlyEntry(this, index); }
        /// <summary>Live const view: loads from the owner, never a defensive copy of shared bits.</summary>
        public readonly struct ReadOnlyEntry
        {
            private readonly PawnHistory? owner;
            private readonly UIntPtr index;
            internal ReadOnlyEntry(PawnHistory owner, UIntPtr index) { this.owner = owner; this.index = index; }
            private PawnHistory Owner => owner ?? throw new InvalidOperationException("An unbound view has no native reference.");
            public int Count => Owner[index].Count;
            public short this[int piece, int square] => Owner[index][piece, square];
            public PawnHistoryEntry Snapshot() => new PawnHistoryEntry(ref Owner[index]);
        }
    }
}
