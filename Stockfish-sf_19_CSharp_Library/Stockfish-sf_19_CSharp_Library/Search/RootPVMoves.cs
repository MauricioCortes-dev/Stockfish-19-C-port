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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000533: dynamically sized root PV with independent value copies.</summary>
    /// <remarks>Published storage is never modified: writes copy it, preserving struct assignment
    /// semantics. MAX_PLY is an initial reservation, not a length limit. This readable adaptation
    /// allocates on writes; performance and Unity runtimes require separate measurements.</remarks>
    public struct RootPVMoves : IReadOnlyList<Move>
    {
        private Move[] values;
        private int length;
        public int Capacity => values == null ? ChessConstants.MAX_PLY : values.Length;
        public int Count => length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Size() => length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Empty() => length == 0;

        public Move this[int index]
        {
            get
            {
                CheckIndex(index);
                return values[index];
            }
            set
            {
                CheckIndex(index);
                Move[] copy = CopyStorage(Capacity);
                copy[index] = value;
                values = copy;
            }
        }

        public void PushBack(Move move)
        {
            if (length == int.MaxValue) throw new InvalidOperationException("Root PV is too long.");
            Move[] copy = CopyStorage(Math.Max(Capacity, length + 1));
            copy[length] = move;
            values = copy;
            ++length;
        }

        /// <summary>Publish one root move plus its child line without repeated copy-on-write appends.</summary>
        internal void AssignContinuation(ref PVMoves child)
        {
            if (length == 0) throw new InvalidOperationException("A root move is required.");
            if (child.Count == 0) { length = 1; return; }
            int count = checked(child.Count + 1);
            var copy = new Move[Math.Max(Capacity, count)];
            copy[0] = values[0];
            for (int i = 0; i < child.Count; ++i) copy[i + 1] = child[i];
            // Published arrays remain immutable, including for existing value copies/enumerators.
            values = copy;
            length = count;
        }

        /// <summary>Like vector::resize, may shrink or grow. New moves are value-initialized.</summary>
        public void Resize(int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException(nameof(newSize));
            if (newSize > length) values = CopyStorage(Math.Max(Capacity, newSize));
            length = newSize;
        }

        public void Reserve(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > Capacity) values = CopyStorage(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => length = 0;

        public Move[] ToArray()
        {
            var copy = new Move[length];
            if (length != 0) Array.Copy(values, copy, length);
            return copy;
        }

        private Move[] CopyStorage(int capacity)
        {
            var copy = new Move[capacity];
            if (length != 0) Array.Copy(values, copy, length);
            return copy;
        }

        private void CheckIndex(int index)
        {
            if ((uint)index >= (uint)length) throw new ArgumentOutOfRangeException(nameof(index));
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<Move> IEnumerable<Move>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Enumeration owns a value snapshot, including across later mutations.</summary>
        public struct Enumerator : IEnumerator<Move>
        {
            private readonly RootPVMoves list;
            private int index;
            internal Enumerator(RootPVMoves list) { this.list = list; index = -1; }
            public bool MoveNext()
            {
                if (index + 1 < list.length) { ++index; return true; }
                index = list.length;
                return false;
            }
            public Move Current
            {
                get
                {
                    if (index < 0 || index >= list.length) throw new InvalidOperationException();
                    return list[index];
                }
            }
            object IEnumerator.Current => Current;
            public void Reset() => index = -1;
            public void Dispose() { }
        }
    }
}
