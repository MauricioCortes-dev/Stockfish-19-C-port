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
    /// <summary>SF19-000108/000933: ordinary 16x64 correction history, copied independently by value.</summary>
    public struct PieceToCorrectionHistory : IReadOnlyList<PieceToCorrectionHistory.Row>
    {
        /// <summary>Managed identity for a live table; copying a frame never copies its value.</summary>
        public sealed class Owner : PieceToCorrectionHistoryReference
        {
            public PieceToCorrectionHistory Value;
            public override ref PieceToCorrectionHistory Get() => ref Value;
        }
        public struct Limit1024 : IStatsLimit { public int Limit => HistoryConstants.CORRECTION_HISTORY_LIMIT; }
        public struct Row : IReadOnlyList<StatsCell<short, AtomicInt16, Limit1024>>,
            ILocalStatsNode<short, AtomicInt16, Limit1024, Row>
        {
            private Stats<short, AtomicInt16, Limit1024, StatsCell<short, AtomicInt16, Limit1024>, HistoryStorage64<StatsCell<short, AtomicInt16, Limit1024>>> values;
            public int Count => values.Count;
            public int Size() => Count;
            public int MaxSize() => Count;
            public bool Empty() => false;
            public StatsCell<short, AtomicInt16, Limit1024> this[int index] { get => values[index]; set => values[index] = value; }
            public StatsCell<short, AtomicInt16, Limit1024> Front { get => values.Front; set => values.Front = value; }
            public StatsCell<short, AtomicInt16, Limit1024> Back { get => values.Back; set => values.Back = value; }
            public void Fill(in short value) => HistoryFlatAccess.Fill<Row, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, 64, in value);
            public void CopyFrom(ref Row source) => HistoryFlatAccess.Copy<Row, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, ref source, 64);
            public void Swap(ref Row other) => HistoryFlatAccess.Swap<Row, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, ref other, 64);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref StatsCell<short, AtomicInt16, Limit1024> At(ref Row source, int index)
            {
                if ((uint)index >= 64) throw new ArgumentOutOfRangeException(nameof(index));
                return ref HistoryFlatAccess.Cells<Row, StatsCell<short, AtomicInt16, Limit1024>>(ref source, 64)[index];
            }
            public MultiArray<StatsCell<short, AtomicInt16, Limit1024>, HistoryStorage64<StatsCell<short, AtomicInt16, Limit1024>>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public MultiArray<StatsCell<short, AtomicInt16, Limit1024>, HistoryStorage64<StatsCell<short, AtomicInt16, Limit1024>>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
            IEnumerator<StatsCell<short, AtomicInt16, Limit1024>> IEnumerable<StatsCell<short, AtomicInt16, Limit1024>>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        private Stats<short, AtomicInt16, Limit1024, Row, HistoryStorage16<Row>> values;
        public int Count => values.Count;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => false;
        public Row this[int piece] { get => values[piece]; set => values[piece] = value; }
        public short this[int piece, int square]
        {
            get => At(ref this, piece, square).Load();
            set => At(ref this, piece, square).Assign(value);
        }
        public Row Front { get => values.Front; set => values.Front = value; }
        public Row Back { get => values.Back; set => values.Back = value; }
        public void Fill(in short value) => HistoryFlatAccess.Fill<PieceToCorrectionHistory, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, 16 * 64, in value);
        public PieceToCorrectionHistory(ref PieceToCorrectionHistory source) { values = default; CopyFrom(ref source); }
        public void CopyFrom(ref PieceToCorrectionHistory source) => HistoryFlatAccess.Copy<PieceToCorrectionHistory, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, ref source, 16 * 64);
        public void Swap(ref PieceToCorrectionHistory other) => HistoryFlatAccess.Swap<PieceToCorrectionHistory, StatsCell<short, AtomicInt16, Limit1024>, short>(ref this, ref other, 16 * 64);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref StatsCell<short, AtomicInt16, Limit1024> At(ref PieceToCorrectionHistory source, int piece, int square)
        {
            if ((uint)piece >= 16 || (uint)square >= 64) throw new ArgumentOutOfRangeException("index");
            return ref HistoryFlatAccess.Cells<PieceToCorrectionHistory, StatsCell<short, AtomicInt16, Limit1024>>(ref source, 16 * 64)[piece * 64 + square];
        }
        public MultiArray<Row, HistoryStorage16<Row>>.Enumerator GetEnumerator() => values.GetEnumerator();
        public MultiArray<Row, HistoryStorage16<Row>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
        IEnumerator<Row> IEnumerable<Row>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
