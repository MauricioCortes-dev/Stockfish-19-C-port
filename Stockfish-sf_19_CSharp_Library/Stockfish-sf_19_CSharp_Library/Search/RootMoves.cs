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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000932: owned, dynamically sized root candidates with live element access.</summary>
    /// <remarks>C# reference assignment aliases this owner. Use the copy constructor or CopyFrom
    /// for native vector value copies, especially between workers. Each copy owns its candidate
    /// array; RootMove preserves value copies of both PVs. Exclusive use is required per owner.
    /// Growth invalidates element references. Sorting/rotation keep storage but change the
    /// candidates occupying its slots. Do not use removed elements or structurally mutate
    /// the collection during enumeration. Managed references cannot enforce these lifetimes.</remarks>
    public sealed class RootMoves : IReadOnlyList<RootMove>
    {
        private RootMove[] values = Array.Empty<RootMove>();
        private int length;

        public RootMoves() { }
        public RootMoves(RootMoves source) { CopyFrom(source); }
        public int Count => length;
        public int Capacity => values.Length;
        public int Size() => length;
        public bool Empty() => length == 0;

        public ref RootMove this[int index]
        {
            get
            {
                CheckIndex(index);
                return ref values[index];
            }
        }
        RootMove IReadOnlyList<RootMove>.this[int index] => this[index];
        public ref RootMove Front => ref this[0];
        public ref RootMove Back => ref this[length - 1];

        public ref RootMove EmplaceBack(Move move)
        {
            int required = checked(length + 1);
            if (required > values.Length)
                Reserve((int)Math.Max(required, Math.Min(int.MaxValue, 2L * values.Length)));
            // Construct before publishing the new length, including if PV allocation fails.
            values[length] = new RootMove(move);
            length = required;
            return ref values[length - 1];
        }

        public void Reserve(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity <= values.Length) return;
            var copy = new RootMove[capacity];
            Array.Copy(values, copy, length);
            values = copy;
        }

        /// <summary>Native vector assignment: independent candidates/PVs; self-copy is a no-op.</summary>
        public void CopyFrom(RootMoves source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (ReferenceEquals(this, source)) return;
            Reserve(source.length);
            Array.Copy(source.values, values, source.length);
            if (length > source.length) Array.Clear(values, source.length, length - source.length);
            length = source.length;
        }

        public void Clear()
        {
            Array.Clear(values, 0, length);
            length = 0;
        }

        public int IndexOf(Move move) => IndexOf(move, 0, length);
        public int IndexOf(Move move, int first, int last)
        {
            CheckRange(first, last);
            for (int i = first; i < last; ++i) if (values[i] == move) return i;
            return -1;
        }
        public int CountMatches(Move move, int first, int last)
        {
            CheckRange(first, last);
            int count = 0;
            for (int i = first; i < last; ++i) if (values[i] == move) ++count;
            return count;
        }

        public void StableSort() => StableSort(0, length);
        public void StableSort(int first, int last)
            => StableSort(first, last, (left, right) => left.CompareTo(right));

        /// <summary>Stable insertion sort over [first,last); a negative comparison precedes.</summary>
        /// <remarks>Comparison must provide a strict weak ordering and must not mutate this
        /// collection. Insertion sort keeps the array and allocates no scratch candidate bank.</remarks>
        public void StableSort(int first, int last, Comparison<RootMove> comparison)
        {
            CheckRange(first, last);
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (last - first < 2) return;
            for (int i = first + 1; i < last; ++i)
            {
                RootMove candidate = values[i];
                int j = i;
                // Compare before shifting, so a throwing comparison does not lose candidates.
                while (j > first && comparison(candidate, values[j - 1]) < 0) --j;
                if (j == i) continue;
                Array.Copy(values, j, values, j + 1, i - j);
                values[j] = candidate;
            }
        }

        /// <summary>RootMoves specialization of Utility::move_to_front: rotate the first match.</summary>
        public void MoveToFront(Predicate<RootMove> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            for (int i = 0; i < length; ++i)
            {
                if (!predicate(values[i])) continue;
                RootMove candidate = values[i];
                Array.Copy(values, 0, values, 1, i);
                values[0] = candidate;
                return;
            }
        }

        public void SwapElements(int left, int right)
        {
            CheckIndex(left); CheckIndex(right);
            RootMove copy = values[left]; values[left] = values[right]; values[right] = copy;
        }

        /// <summary>Independent values, suitable for a snapshot or a readonly consumer.</summary>
        public RootMove[] ToArray()
        {
            var copy = new RootMove[length];
            Array.Copy(values, copy, length);
            return copy;
        }

        private void CheckIndex(int index)
        {
            if ((uint)index >= (uint)length) throw new ArgumentOutOfRangeException(nameof(index));
        }
        private void CheckRange(int first, int last)
        {
            if (first < 0 || first > length) throw new ArgumentOutOfRangeException(nameof(first));
            if (last < first || last > length) throw new ArgumentOutOfRangeException(nameof(last));
        }

        public Enumerator GetEnumerator() => new Enumerator(values, length);
        IEnumerator<RootMove> IEnumerable<RootMove>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Ref enumeration visits actual slots; interface enumeration returns value copies.</summary>
        public struct Enumerator : IEnumerator<RootMove>
        {
            private readonly RootMove[] values;
            private readonly int length;
            private int index;
            internal Enumerator(RootMove[] values, int length) { this.values = values; this.length = length; index = -1; }
            public bool MoveNext()
            {
                if (index + 1 < length) { ++index; return true; }
                index = length;
                return false;
            }
            public ref RootMove Current
            {
                get
                {
                    if (index < 0 || index >= length) throw new InvalidOperationException();
                    return ref values[index];
                }
            }
            RootMove IEnumerator<RootMove>.Current => Current;
            object IEnumerator.Current => Current;
            public void Reset() => index = -1;
            public void Dispose() { }
        }
    }
}
