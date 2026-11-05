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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000534: mutable fixed-capacity PV, copied entirely by value.</summary>
    public struct PVMoves : IReadOnlyList<Move>
    {
        // Inline values avoid mutable-array aliasing on ordinary struct assignment.
#pragma warning disable CS0649
        private ValueStorage256<Move> values;
#pragma warning restore CS0649
        private int length;
        public const int Capacity = ChessConstants.MAX_PLY + 1;
        public int Count => length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Size() => length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Empty() => length == 0;

        /// <summary>Addresses storage, including inactive slots, like native moves[index].</summary>
        public Move this[int index]
        {
            get { CheckIndex(index); return values[index]; }
            set { CheckIndex(index); values[index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => length = 0;

        public void PushBack(Move move)
        {
            if (length == Capacity) throw new InvalidOperationException("PV capacity exceeded.");
            values[length++] = move;
        }

        /// <summary>The native resize only shortens the PV and preserves inactive storage.</summary>
        public void Resize(int newSize)
        {
            if ((uint)newSize > (uint)length) throw new ArgumentOutOfRangeException(nameof(newSize));
            length = newSize;
        }

        /// <summary>Prepend a move to a value snapshot of the child PV, or create a one-move PV.</summary>
        public void Update(Move move, PVMoves? childPv)
        {
            int childLength = childPv.HasValue ? childPv.Value.length : 0;
            if (childLength > ChessConstants.MAX_PLY)
                throw new ArgumentOutOfRangeException(nameof(childPv));
            if (childPv.HasValue)
            {
                PVMoves child = childPv.Value;
                for (int i = 0; i < childLength; ++i) values[i + 1] = child.values[i];
            }
            values[0] = move;
            length = childLength + 1;
        }

        /// <summary>Adapt native operator=; retain inactive slots when updating an existing PV.</summary>
        public void Assign(RootPVMoves rhs)
        {
            length = Math.Min(rhs.Count, ChessConstants.MAX_PLY);
            for (int i = 0; i < length; ++i) values[i] = rhs[i];
        }

        /// <summary>Allows PVMoves pv = root; C# cannot overload assignment to an existing value.</summary>
        public static implicit operator PVMoves(RootPVMoves rhs)
        {
            PVMoves result = default;
            result.Assign(rhs);
            return result;
        }

        public Move[] ToArray()
        {
            var result = new Move[length];
            for (int i = 0; i < length; ++i) result[i] = values[i];
            return result;
        }

        private static void CheckIndex(int index)
        {
            if ((uint)index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<Move> IEnumerable<Move>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public struct Enumerator : IEnumerator<Move>
        {
            private readonly PVMoves list;
            private int index;
            internal Enumerator(PVMoves list) { this.list = list; index = -1; }
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
                    return list.values[index];
                }
            }
            object IEnumerator.Current => Current;
            public void Reset() => index = -1;
            public void Dispose() { }
        }
    }
}
