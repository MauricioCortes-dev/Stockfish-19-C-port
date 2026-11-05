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
    /// <summary>Shape adapter for PawnHistory: inline 16x64 shared history, D=8192. Concurrent copies must use the ref APIs.</summary>
    public struct PawnHistoryEntry : IReadOnlyList<PawnHistoryEntry.Row>
    {
        public struct Limit8192 : IStatsLimit { public int Limit => 8192; }
        public struct Row : IReadOnlyList<AtomicStatsCell<short, AtomicInt16, Limit8192>>,
            IAtomicStatsNode<short, AtomicInt16, Limit8192, Row>
        {
            private AtomicStats<short, AtomicInt16, Limit8192, AtomicStatsCell<short, AtomicInt16, Limit8192>, HistoryStorage64<AtomicStatsCell<short, AtomicInt16, Limit8192>>> values;
            public int Count => values.Count;
            public int Size() => Count;
            public int MaxSize() => Count;
            public bool Empty() => false;
            public AtomicStatsCell<short, AtomicInt16, Limit8192> this[int index] { get => values[index]; set => values[index] = value; }
            public AtomicStatsCell<short, AtomicInt16, Limit8192> Front { get => values.Front; set => values.Front = value; }
            public AtomicStatsCell<short, AtomicInt16, Limit8192> Back { get => values.Back; set => values.Back = value; }
            public void Fill(in short value) => HistoryFlatAccess.Fill<Row, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, 64, in value);
            public void CopyFrom(ref Row source) => HistoryFlatAccess.Copy<Row, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, ref source, 64);
            public void Swap(ref Row other) => HistoryFlatAccess.Swap<Row, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, ref other, 64);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref AtomicStatsCell<short, AtomicInt16, Limit8192> At(ref Row source, int index)
            {
                if ((uint)index >= 64) throw new ArgumentOutOfRangeException(nameof(index));
                return ref HistoryFlatAccess.Cells<Row, AtomicStatsCell<short, AtomicInt16, Limit8192>>(ref source, 64)[index];
            }
            public MultiArray<AtomicStatsCell<short, AtomicInt16, Limit8192>, HistoryStorage64<AtomicStatsCell<short, AtomicInt16, Limit8192>>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public MultiArray<AtomicStatsCell<short, AtomicInt16, Limit8192>, HistoryStorage64<AtomicStatsCell<short, AtomicInt16, Limit8192>>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
            IEnumerator<AtomicStatsCell<short, AtomicInt16, Limit8192>> IEnumerable<AtomicStatsCell<short, AtomicInt16, Limit8192>>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        private AtomicStats<short, AtomicInt16, Limit8192, Row, HistoryStorage16<Row>> values;
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
        public void Fill(in short value) => HistoryFlatAccess.Fill<PawnHistoryEntry, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, 16 * 64, in value);
        public PawnHistoryEntry(ref PawnHistoryEntry source) { values = default; CopyFrom(ref source); }
        public void CopyFrom(ref PawnHistoryEntry source) => HistoryFlatAccess.Copy<PawnHistoryEntry, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, ref source, 16 * 64);
        public void Swap(ref PawnHistoryEntry other) => HistoryFlatAccess.Swap<PawnHistoryEntry, AtomicStatsCell<short, AtomicInt16, Limit8192>, short>(ref this, ref other, 16 * 64);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref AtomicStatsCell<short, AtomicInt16, Limit8192> At(ref PawnHistoryEntry source, int piece, int square)
        {
            if ((uint)piece >= 16 || (uint)square >= 64) throw new ArgumentOutOfRangeException("index");
            return ref HistoryFlatAccess.Cells<PawnHistoryEntry, AtomicStatsCell<short, AtomicInt16, Limit8192>>(ref source, 16 * 64)[piece * 64 + square];
        }
        public MultiArray<Row, HistoryStorage16<Row>>.Enumerator GetEnumerator() => values.GetEnumerator();
        public MultiArray<Row, HistoryStorage16<Row>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
        IEnumerator<Row> IEnumerable<Row>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
