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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
namespace Stockfish_sf_19_CSharp_Library.Search
{
    // Shape adapter shared by ButterflyHistory and LowPlyHistory: 65536 inline short cells.
    public struct QuietHistoryRow : IReadOnlyList<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>>,
        ILocalStatsNode<short, AtomicInt16, QuietHistoryRow.Limit7183, QuietHistoryRow>
    {
        public struct Limit7183 : IStatsLimit { public int Limit => 7183; }
        private HistoryStorage64<HistoryStorage64<HistoryStorage16<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>>>> values;
        public int Count => HistoryConstants.UINT_16_HISTORY_SIZE;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => false;
        public StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183> this[int index]
        { get => At(ref this, index); set => At(ref this, index) = value; }
        public StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183> Front { get => this[0]; set => this[0] = value; }
        public StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183> Back { get => this[Count - 1]; set => this[Count - 1] = value; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183> At(ref QuietHistoryRow source, int index)
        {
            if ((uint)index >= HistoryConstants.UINT_16_HISTORY_SIZE) throw new ArgumentOutOfRangeException(nameof(index));
            return ref Cells(ref source)[index];
        }
        // The closed storage is 65536 consecutive inline cells without CLR references.
        // A row view avoids three nested storage delegates per cell during every
        // search's mandatory history preparation. Keep scalar atomic Load/Assign.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static System.Span<StatsCell<short, AtomicInt16, Limit7183>> Cells(ref QuietHistoryRow source)
        {
            var cells = System.Runtime.InteropServices.MemoryMarshal.Cast<QuietHistoryRow, StatsCell<short, AtomicInt16, Limit7183>>(
                System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref source, 1));
            if (cells.Length != HistoryConstants.UINT_16_HISTORY_SIZE)
                throw new System.InvalidOperationException("Unexpected quiet-history layout.");
            return cells;
        }
        public void Fill(in short value)
        {
            var cells = Cells(ref this);
            for (int i = 0; i < cells.Length; ++i) cells[i].Assign(value);
        }
        public void CopyFrom(ref QuietHistoryRow source)
        => HistoryFlatAccess.Copy<QuietHistoryRow, StatsCell<short, AtomicInt16, Limit7183>, short>(ref this, ref source, 65536);
        public void Swap(ref QuietHistoryRow other)
        => HistoryFlatAccess.Swap<QuietHistoryRow, StatsCell<short, AtomicInt16, Limit7183>, short>(ref this, ref other, 65536);
        private HistorySnapshotEnumerator<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>> Snapshot(bool reverse)
        {
            var items = new StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>[Count];
            for (int i = 0; i < Count; ++i) items[i] = At(ref this, i);
            return new HistorySnapshotEnumerator<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>>(new ArrayHistorySnapshot<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>>(items), reverse);
        }
        public HistorySnapshotEnumerator<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>> GetEnumerator() => Snapshot(false);
        public HistorySnapshotEnumerator<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>> GetReverseEnumerator() => Snapshot(true);
        IEnumerator<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>> IEnumerable<StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
