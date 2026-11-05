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

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>Move ordering primitives with scalar and block paths, independent of histories and MovePicker.</summary>
    public static class MoveOrdering
    {
        /// <summary>SF19-000218: partial insertion sort, preserving the original scalar ordering.</summary>
        /// <remarks>The first move seeds the sorted prefix even below limit. The rest of the
        /// prefix receives qualifying moves in descending order; the tail is not fully sorted.</remarks>
        public static void PartialInsertionSort(Span<ExtMove> moves, int limit)
        {
            if (MoveProcessing.UseBlocks) PartialInsertionSortBlocks(moves, limit);
            else PartialInsertionSortScalar(moves, limit);
        }

        public static void PartialInsertionSortScalar(Span<ExtMove> moves, int limit)
        {
            ScalarTail(moves, limit, 0, 1);
        }

        /// <summary>SF19-000898: sort the first eligible group, then continue the scalar tail.</summary>
        public static void PartialInsertionSortBlocks(Span<ExtMove> moves, int limit)
        {
            if (moves.IsEmpty) return;
            // The previous public scalar contract accepts INT_MIN. Preserve it outside
            // the native optimized branch's insert precondition instead of silently losing moves.
            for (int i = 1; i < moves.Length; ++i)
                if (moves[i].value == int.MinValue && moves[i].value >= limit)
                { PartialInsertionSortScalar(moves, limit); return; }
            Span<uint> lanes = stackalloc uint[16]; Span<int> scores = stackalloc int[16];
            var sorter = new MoveSorter(moves[0], lanes, scores);
            int sortedEnd = 0, p = 1;
            for (; p < moves.Length; ++p)
                if (moves[p].value >= limit)
                {
                    if (sortedEnd + 1 >= MoveSorter.MaxElements) break;
                    sorter.Insert(moves[p]); moves[p] = moves[++sortedEnd];
                }
            sorter.WriteSorted(moves, sortedEnd + 1);
            ScalarTail(moves, limit, sortedEnd, p);
        }

        private static void ScalarTail(Span<ExtMove> moves, int limit, int sortedEnd, int start)
        {
            for (int p = start; p < moves.Length; ++p)
                if (moves[p].value >= limit)
                {
                    ExtMove tmp = moves[p];
                    moves[p] = moves[++sortedEnd];
                    int q = sortedEnd;
                    for (; q != 0 && moves[q - 1] < tmp; --q) moves[q] = moves[q - 1];
                    moves[q] = tmp;
                }
        }
    }
}
