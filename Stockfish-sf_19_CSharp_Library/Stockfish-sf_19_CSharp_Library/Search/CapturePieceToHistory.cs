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
using Leaf = Stockfish_sf_19_CSharp_Library.Search.StatsCell<short,Stockfish_sf_19_CSharp_Library.Infrastructure.AtomicInt16,Stockfish_sf_19_CSharp_Library.Search.CapturePieceToHistory.Limit10692>;
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000902: fixed 16x64x8 local history with independent inline value storage.</summary>
    public struct CapturePieceToHistory : IReadOnlyList<CapturePieceToHistory.Plane>
    {
        /// <summary>Managed identity for a live table; constructing a picker never copies its value.</summary>
        public sealed class Owner { public CapturePieceToHistory Value; }
        public struct Limit10692 : IStatsLimit { public int Limit => 10692; }
        // Named shape adapters keep CLR metadata names bounded while retaining inline values.
        public struct Row : IReadOnlyList<Leaf>, ILocalStatsNode<short, AtomicInt16, Limit10692, Row>
        {
            private Stats<short, AtomicInt16, Limit10692, Leaf, HistoryStorage8<Leaf>> values;
            public int Count => values.Count;
            public int Size() => Count;
            public int MaxSize() => Count;
            public bool Empty() => false;
            public Leaf this[int index] { get => values[index]; set => values[index] = value; }
            public Leaf Front { get => values.Front; set => values.Front = value; }
            public Leaf Back { get => values.Back; set => values.Back = value; }
            public void Fill(in short value) => HistoryFlatAccess.Fill<Row, Leaf, short>(ref this, 8, in value);
            public void CopyFrom(ref Row source) => HistoryFlatAccess.Copy<Row, Leaf, short>(ref this, ref source, 8);
            public void Swap(ref Row other) => HistoryFlatAccess.Swap<Row, Leaf, short>(ref this, ref other, 8);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref Leaf At(ref Row source, int index)
            {
                if ((uint)index >= 8) throw new ArgumentOutOfRangeException(nameof(index));
                return ref HistoryFlatAccess.Cells<Row, Leaf>(ref source, 8)[index];
            }
            public MultiArray<Leaf, HistoryStorage8<Leaf>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public MultiArray<Leaf, HistoryStorage8<Leaf>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
            IEnumerator<Leaf> IEnumerable<Leaf>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        // Named shape adapters keep CLR metadata names bounded while retaining inline values.
        public struct Plane : IReadOnlyList<Row>, ILocalStatsNode<short, AtomicInt16, Limit10692, Plane>
        {
            private Stats<short, AtomicInt16, Limit10692, Row, HistoryStorage64<Row>> values;
            public int Count => values.Count;
            public int Size() => Count;
            public int MaxSize() => Count;
            public bool Empty() => false;
            public Row this[int index] { get => values[index]; set => values[index] = value; }
            public Row Front { get => values.Front; set => values.Front = value; }
            public Row Back { get => values.Back; set => values.Back = value; }
            public void Fill(in short value) => HistoryFlatAccess.Fill<Plane, Leaf, short>(ref this, 64 * 8, in value);
            public void CopyFrom(ref Plane source) => HistoryFlatAccess.Copy<Plane, Leaf, short>(ref this, ref source, 64 * 8);
            public void Swap(ref Plane other) => HistoryFlatAccess.Swap<Plane, Leaf, short>(ref this, ref other, 64 * 8);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref Row At(ref Plane source, int index)
            {
                if ((uint)index >= 64) throw new ArgumentOutOfRangeException(nameof(index));
                return ref HistoryFlatAccess.Cells<Plane, Row>(ref source, 64)[index];
            }
            public MultiArray<Row, HistoryStorage64<Row>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public MultiArray<Row, HistoryStorage64<Row>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
            IEnumerator<Row> IEnumerable<Row>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        private Stats<short, AtomicInt16, Limit10692, Plane, HistoryStorage16<Plane>> values;
        public int Count => values.Count;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => false;
        public Plane this[int piece] { get => values[piece]; set => values[piece] = value; }
        public short this[int piece, int square, int capturedType]
        {
            get => At(ref this, piece, square, capturedType).Load();
            set => At(ref this, piece, square, capturedType).Assign(value);
        }
        public Plane Front { get => values.Front; set => values.Front = value; }
        public Plane Back { get => values.Back; set => values.Back = value; }
        public void Fill(in short value) => HistoryFlatAccess.Fill<CapturePieceToHistory, Leaf, short>(ref this, 16 * 64 * 8, in value);
        public CapturePieceToHistory(ref CapturePieceToHistory source) { values = default; CopyFrom(ref source); }
        public void CopyFrom(ref CapturePieceToHistory source) => HistoryFlatAccess.Copy<CapturePieceToHistory, Leaf, short>(ref this, ref source, 16 * 64 * 8);
        public void Swap(ref CapturePieceToHistory other) => HistoryFlatAccess.Swap<CapturePieceToHistory, Leaf, short>(ref this, ref other, 16 * 64 * 8);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Leaf At(ref CapturePieceToHistory source, int piece, int square, int capturedType)
        {
            if ((uint)piece >= 16 || (uint)square >= 64 || (uint)capturedType >= 8) throw new ArgumentOutOfRangeException("index");
            return ref HistoryFlatAccess.Cells<CapturePieceToHistory, Leaf>(ref source, 16 * 64 * 8)[(piece * 64 + square) * 8 + capturedType];
        }
        public MultiArray<Plane, HistoryStorage16<Plane>>.Enumerator GetEnumerator() => values.GetEnumerator();
        public MultiArray<Plane, HistoryStorage16<Plane>>.Enumerator GetReverseEnumerator() => values.GetReverseEnumerator();
        IEnumerator<Plane> IEnumerable<Plane>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
