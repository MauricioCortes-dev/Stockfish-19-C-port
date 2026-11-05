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
using System.Numerics;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>SF19-000217: sixteen separate move/value lanes and mask-expand insertion.</summary>
    public ref struct MoveSorter
    {
        public const int MaxElements = 16;
        private Span<uint> sortedMoves;
        private Span<int> sortedValues;
        public int Count { get; private set; }

        public MoveSorter(ExtMove first, Span<uint> moves, Span<int> values)
        {
            if (moves.Length < MaxElements || values.Length < MaxElements) throw new ArgumentException("Sixteen lanes required.");
            sortedMoves = moves.Slice(0, MaxElements); sortedValues = values.Slice(0, MaxElements);
            SplatExtMove(first, sortedMoves, sortedValues);
            sortedValues.Slice(1).Fill(int.MinValue); Count = 1;
        }
        /// <summary>Explicit value-copy into independent caller-owned lanes.</summary>
        public MoveSorter(MoveSorter other, Span<uint> moves, Span<int> values)
        {
            if (moves.Length < MaxElements || values.Length < MaxElements) throw new ArgumentException("Sixteen lanes required.");
            sortedMoves = moves.Slice(0, MaxElements); sortedValues = values.Slice(0, MaxElements);
            other.sortedMoves.CopyTo(sortedMoves); other.sortedValues.CopyTo(sortedValues); Count = other.Count;
        }
        /// <summary>SF19-000216: raw moves are zero-extended to the original u32 register lanes.</summary>
        public static void SplatExtMove(ExtMove move, Span<uint> moves, Span<int> values)
        {
            if (moves.Length < MaxElements || values.Length < MaxElements) throw new ArgumentException("Sixteen lanes required.");
            moves.Slice(0, MaxElements).Fill(move.Raw()); values.Slice(0, MaxElements).Fill(move.value);
        }
        public void Insert(ExtMove move)
        {
            if (Count >= MaxElements) throw new InvalidOperationException("MoveSorter is full.");
            if (move.value == int.MinValue) throw new ArgumentException("INT_MIN is reserved for empty lanes.");
            uint less = 0; int i = 0, width = Vector<int>.Count;
            if (Vector.IsHardwareAccelerated)
                for (; i <= MaxElements - width; i += width)
                {
                    var compare = Vector.LessThan(BlockLanes.Load<int>(sortedValues.Slice(i)), new Vector<int>(move.value));
                    for (int lane = 0; lane < width; ++lane) if (compare[lane] != 0) less |= 1u << (i + lane);
                }
            for (; i < MaxElements; ++i) if (sortedValues[i] < move.value) less |= 1u << i;
            uint expand = unchecked(less - 1) & 0xffff;
            // Retain the old register image while mask_expand overwrites destination lanes.
            int source = 0;
            Span<uint> oldMoves = stackalloc uint[MaxElements]; sortedMoves.CopyTo(oldMoves);
            Span<int> oldValues = stackalloc int[MaxElements]; sortedValues.CopyTo(oldValues);
            for (i = 0; i < MaxElements; ++i)
                if ((expand & (1u << i)) != 0)
                {
                    sortedMoves[i] = oldMoves[source]; sortedValues[i] = oldValues[source++];
                }
                else { sortedMoves[i] = move.Raw(); sortedValues[i] = move.value; }
            ++Count;
        }
        public void WriteSorted(Span<ExtMove> output, int count)
        {
            if (count < 0 || count > Count || output.Length < count) throw new ArgumentOutOfRangeException(nameof(count));
            for (int i = 0; i < count; ++i) output[i] = new ExtMove(new Move((ushort)sortedMoves[i]), sortedValues[i]);
        }
    }
}
