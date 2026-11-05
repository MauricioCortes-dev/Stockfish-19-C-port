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
    /// <summary>65536 buckets per thread, two colors and four atomic short entries per bucket.</summary>
    public sealed class UnifiedCorrectionHistory
    {
        public struct Policy : IDynStatsPolicy<CorrectionHistoryEntry>
        {
            public int SizeMultiplier => HistoryConstants.CORRHIST_BASE_SIZE;

            public void Initialize(ref CorrectionHistoryEntry entry)
            {
            }

            public void Fill(ref CorrectionHistoryEntry entry, int value) => entry.Fill(unchecked((short)value));
        }

        private readonly DynStats<CorrectionHistoryEntry, Policy> values;
        public UnifiedCorrectionHistory(UIntPtr threads)
        {
            values = new DynStats<CorrectionHistoryEntry, Policy>(threads);
        }

        public UIntPtr GetSize() => values.GetSize();
        public ref CorrectionHistoryEntry this[UIntPtr index] => ref values[index];
        public void ClearRange(int value, UIntPtr thread, UIntPtr total) => values.ClearRange(value, thread, total);
        public ReadOnlyEntry ReadOnlyAt(UIntPtr index)
        {
            if (index.ToUInt64() >= (ulong)values.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new ReadOnlyEntry(this, index);
        }

        public readonly struct ReadOnlyEntry
        {
            private readonly UnifiedCorrectionHistory owner;
            private readonly UIntPtr index;
            internal ReadOnlyEntry(UnifiedCorrectionHistory owner, UIntPtr index)
            {
                this.owner = owner;
                this.index = index;
            }

            public short Pawn(int color) => CorrectionHistoryEntry.At(ref owner[index], color).Pawn.Load();
            public short Minor(int color) => CorrectionHistoryEntry.At(ref owner[index], color).Minor.Load();
            public short NonPawnWhite(int color) => CorrectionHistoryEntry.At(ref owner[index], color).NonPawnWhite.Load();
            public short NonPawnBlack(int color) => CorrectionHistoryEntry.At(ref owner[index], color).NonPawnBlack.Load();
        }
    }
}
