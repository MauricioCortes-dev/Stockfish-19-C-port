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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Fixed outer dimension and inline elements, including nested MultiArray values.</summary>
    /// <remarks>SF19-000188. Assignment copies all storage. T and custom storage must preserve
    /// the native value-copy contract. Use MultiArray.At(ref array, index) for live references.</remarks>
    public struct MultiArray<T, TStorage> : IReadOnlyList<T>
        where TStorage : struct, IMultiArrayStorage<T, TStorage>
    {
        private TStorage values;
        public int Count => values.Length;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => Count == 0;

        public T this[int index]
        {
            get => Element(ref this, index);
            set => Element(ref this, index) = value;
        }
        public T Front { get => this[0]; set => this[0] = value; }
        public T Back { get => this[Count - 1]; set => this[Count - 1] = value; }

        internal static ref T Element(ref MultiArray<T, TStorage> array, int index)
        {
            if ((uint)index >= (uint)array.Count) throw new ArgumentOutOfRangeException(nameof(index));
            return ref array.values.Access(ref array.values, index);
        }

        /// <summary>Exact-type scalar/element assignment. Inferred conversions are rejected before writing.</summary>
        public void Fill<TValue>(in TValue value)
        {
            if (typeof(TValue) != typeof(T))
                throw new ArgumentException("Fill requires the exact element type or an explicit assignment adapter.", nameof(value));
            T item = (T)(object)value!;
            for (int i = 0; i < Count; ++i) Element(ref this, i) = item;
        }

        /// <summary>Explicit operator= adaptation, also used to fill nested children recursively.</summary>
        public void Fill<TValue>(in TValue value, MultiArrayAssignment<T, TValue> assign)
        {
            if (assign == null) throw new ArgumentNullException(nameof(assign));
            for (int i = 0; i < Count; ++i) assign(ref Element(ref this, i), in value);
        }

        public void Swap(ref MultiArray<T, TStorage> other)
        {
            TStorage temporary = values;
            values = other.values;
            other.values = temporary;
        }

        /// <summary>Enumerates a value snapshot; reverse=true adapts the native reverse iterators.</summary>
        public Enumerator GetEnumerator() => new Enumerator(this, false);
        public Enumerator GetReverseEnumerator() => new Enumerator(this, true);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public struct Enumerator : IEnumerator<T>
        {
            private MultiArray<T, TStorage> snapshot;
            private readonly bool reverse;
            private int position;
            internal Enumerator(MultiArray<T, TStorage> array, bool reverse)
            { snapshot = array; this.reverse = reverse; position = -1; }
            public bool MoveNext()
            {
                if (position + 1 < snapshot.Count) { ++position; return true; }
                position = snapshot.Count;
                return false;
            }
            public T Current
            {
                get
                {
                    if (position < 0 || position >= snapshot.Count) throw new InvalidOperationException();
                    return snapshot[reverse ? snapshot.Count - 1 - position : position];
                }
            }
            object? IEnumerator.Current => Current;
            public void Reset() => position = -1;
            public void Dispose() { }
        }
    }

    /// <summary>Ref access takes an explicit caller-owned value, avoiding an escaping struct this.</summary>
    public static class MultiArray
    {
        public static ref T At<T, TStorage>(ref MultiArray<T, TStorage> array, int index)
            where TStorage : struct, IMultiArrayStorage<T, TStorage>
            => ref MultiArray<T, TStorage>.Element(ref array, index);
    }
}
