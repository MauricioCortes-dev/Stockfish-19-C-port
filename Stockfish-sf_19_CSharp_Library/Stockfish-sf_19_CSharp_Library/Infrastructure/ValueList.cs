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
using System.Collections.Generic;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Storage contract for the compile-time capacity of ValueList. Implementations must own their values inline.</summary>
    public interface IValueListStorage<T>
    {
        int Capacity { get; }
        T this[int index] { get; set; }
    }

    /// <summary>SF19-000185: fixed-capacity list with independent value copies, including default(T).</summary>
    /// <remarks>TStorage replaces the C++ integer template argument. MakeSpace returns the first writable index.
    /// Elements must themselves have C++-compatible value semantics; no mutable array backs the list.</remarks>
    public struct ValueList<T, TStorage> where TStorage : struct, IValueListStorage<T>
    {
        // Deliberate default inline storage: constrained indexer setters mutate this field.
#pragma warning disable CS0649
        private TStorage values;
#pragma warning restore CS0649
        private int length;
        public int Capacity => values.Capacity;
        public int Size() => length;
        public int SSize() => length;
        // The native const subscript addresses storage, including a slot written by a failed push_back_if_lt.
        // A setter replaces writes through the pointer returned by make_space.
        public T this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }
        public void PushBack(T value)
        {
            EnsureSpace(1);
            values[length++] = value;
        }
        public void PushBackIfLt(T value, T max) => PushBackIfLt(value, max, Comparer<T>.Default);
        public void PushBackIfLt(T value, T max, IComparer<T> comparer)
        {
            EnsureSpace(1); // Also required when comparison would be false.
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            values[length] = value; // The original writes before comparison, even when it does not advance size.
            if (comparer.Compare(value, max) < 0) ++length;
        }
        /// <summary>Preserves native const-reference aliasing when max refers to a slot of this same list.</summary>
        public void PushBackIfLtAt(T value, int maxIndex)
        {
            EnsureSpace(1);
            _ = values[maxIndex]; // Validate before changing storage.
            values[length] = value;
            if (Comparer<T>.Default.Compare(value, values[maxIndex]) < 0) ++length;
        }
        public int MakeSpace(int count)
        {
            EnsureSpace(count);
            int start = length;
            length += count;
            return start;
        }
        private void EnsureSpace(int count)
        {
            if (count < 0 || count > Capacity - length)
                throw new ArgumentOutOfRangeException(nameof(count), "ValueList capacity exceeded.");
        }
        public Enumerator GetEnumerator() => new Enumerator(this);
        public struct Enumerator
        {
            private readonly ValueList<T, TStorage> list;
            private int index;
            internal Enumerator(ValueList<T, TStorage> list) { this.list = list; index = -1; }
            public bool MoveNext()
            {
                if (index + 1 < list.length) { ++index; return true; }
                index = list.length;
                return false;
            }
            public T Current
            {
                get
                {
                    if (index < 0 || index >= list.length) throw new InvalidOperationException();
                    return list[index];
                }
            }
        }
    }
}
