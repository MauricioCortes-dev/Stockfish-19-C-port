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
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Inline 5x65536 local cells, D=7183. Keep large values in an Owner and use live ref access.</summary>
    public struct LowPlyHistory : IReadOnlyList<QuietHistoryRow>
    {
        public sealed class Owner { public LowPlyHistory Value; }
        private Stats<short, AtomicInt16, QuietHistoryRow.Limit7183, QuietHistoryRow, MultiArrayStoragePair<QuietHistoryRow, HistoryStorage4<QuietHistoryRow>, MultiArrayStorage1<QuietHistoryRow>>> values;
        public int Count => values.Count;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => false;
        public QuietHistoryRow this[int row] { get => values[row]; set => values[row] = value; }
        public short this[int row, int move]
        { get => At(ref this, row, move).Load(); set => At(ref this, row, move).Assign(value); }
        public QuietHistoryRow Front { get => values.Front; set => values.Front = value; }
        public QuietHistoryRow Back { get => values.Back; set => values.Back = value; }
        public void Fill(in short value) => HistoryFlatAccess.Fill<LowPlyHistory, StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>, short>(ref this, 5 * 65536, in value);
        public LowPlyHistory(ref LowPlyHistory source) { values = default; CopyFrom(ref source); }
        public void CopyFrom(ref LowPlyHistory source) => HistoryFlatAccess.Copy<LowPlyHistory, StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>, short>(ref this, ref source, 5 * 65536);
        public void Swap(ref LowPlyHistory other) => HistoryFlatAccess.Swap<LowPlyHistory, StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183>, short>(ref this, ref other, 5 * 65536);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref QuietHistoryRow At(ref LowPlyHistory source, int row)
        {
            if ((uint)row >= 5) throw new ArgumentOutOfRangeException("index");
            return ref HistoryFlatAccess.Cells<LowPlyHistory, QuietHistoryRow>(ref source, 5)[row];
        }
        public static ref StatsCell<short, AtomicInt16, QuietHistoryRow.Limit7183> At(ref LowPlyHistory source, int row, int move)
            => ref QuietHistoryRow.At(ref At(ref source, row), move);
        private HistorySnapshotEnumerator<QuietHistoryRow> Snapshot(bool reverse)
        {
            var rows = new QuietHistoryRowsSnapshot(Count);
            for (int i = 0; i < Count; ++i) rows.CopyFrom(i, ref At(ref this, i));
            return new HistorySnapshotEnumerator<QuietHistoryRow>(rows, reverse);
        }
        public HistorySnapshotEnumerator<QuietHistoryRow> GetEnumerator() => Snapshot(false);
        public HistorySnapshotEnumerator<QuietHistoryRow> GetReverseEnumerator() => Snapshot(true);
        IEnumerator<QuietHistoryRow> IEnumerable<QuietHistoryRow>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
