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
namespace Stockfish_sf_19_CSharp_Library.Search
{
    internal interface IHistorySnapshot<T> { int Count { get; } T Get(int index); }
    internal sealed class ArrayHistorySnapshot<T> : IHistorySnapshot<T>
    {
        private readonly T[] values;
        internal ArrayHistorySnapshot(T[] values) { this.values = values; }
        public int Count => values.Length;
        public T Get(int index) => values[index];
    }
    // CLR arrays reject value-type elements larger than 65535 bytes. One heap owner per
    // 128 KiB row preserves inline value semantics without allocating objects per cell.
    internal sealed class QuietHistoryRowsSnapshot : IHistorySnapshot<QuietHistoryRow>
    {
        private sealed class RowOwner { internal QuietHistoryRow Value; }
        private readonly RowOwner[] rows;
        internal QuietHistoryRowsSnapshot(int count)
        { rows = new RowOwner[count]; for (int i = 0; i < count; ++i) rows[i] = new RowOwner(); }
        public int Count => rows.Length;
        internal void CopyFrom(int index, ref QuietHistoryRow value) => rows[index].Value.CopyFrom(ref value);
        public QuietHistoryRow Get(int index) => rows[index].Value;
    }
    // Representation adapter: snapshots live on the heap, never inside a giant enumerator value.
    public struct HistorySnapshotEnumerator<T> : IEnumerator<T>
    {
        private readonly IHistorySnapshot<T>? snapshot;
        private readonly bool reverse;
        private int cursor;
        internal HistorySnapshotEnumerator(IHistorySnapshot<T> snapshot, bool reverse)
        { this.snapshot = snapshot; this.reverse = reverse; cursor = -1; }
        public T Current
        {
            get
            {
                if (snapshot == null || cursor < 0 || cursor >= snapshot.Count)
                    throw new InvalidOperationException();
                return snapshot.Get(reverse ? snapshot.Count - 1 - cursor : cursor);
            }
        }
        object IEnumerator.Current => Current!;
        public bool MoveNext()
        {
            if (snapshot == null) return false;
            if (cursor < snapshot.Count) ++cursor;
            return cursor < snapshot.Count;
        }
        public void Reset() { cursor = -1; }
        public void Dispose() { }
    }
}
