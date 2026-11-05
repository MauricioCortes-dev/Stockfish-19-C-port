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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    public static partial class MoveGenerator
    {
        public static int SplatMoves(Move[] moveList, int start, Square from, Bitboard toBB)
        {
            if (!MoveProcessing.UseBlocks || Bitboards.Popcount(toBB) > 32) return SplatMovesScalar(moveList, start, from, toBB);
            return SplatMovesBlocks(moveList, start, from, toBB);
        }
        public static int SplatPawnMoves(Move[] moveList, int start, Bitboard toBB, Direction offset)
        {
            if (!MoveProcessing.UseBlocks || Bitboards.Popcount(toBB) > 8) return SplatPawnMovesScalar(moveList, start, toBB, offset);
            return SplatPawnMovesBlocks(moveList, start, toBB, offset);
        }

        /// <summary>SF19-000200: compress AllSquares and pack 32 logical move lanes.</summary>
        public static int SplatMovesBlocks(Move[] moveList, int start, Square from, Bitboard toBB)
        {
            int count = Bitboards.Popcount(toBB);
            RequireBuffer(moveList, start, count);
            if (count > 32) throw new ArgumentException("The block emitter supports at most 32 destination lanes.");
            Span<ushort> lanes = stackalloc ushort[32]; lanes.Clear();
            BlockLanes.CompressSquares(toBB, lanes);
            ushort origin = unchecked((ushort)((int)from << Move.FromSqShift));
            int j = 0, width = Vector<ushort>.Count;
            if (Vector.IsHardwareAccelerated)
                for (; j <= 32 - width; j += width)
                    BlockLanes.Store(BlockLanes.Load<ushort>(lanes.Slice(j)) | new Vector<ushort>(origin), lanes.Slice(j));
            for (; j < 32; ++j) lanes[j] |= origin;
            // Native stores include spare lanes. Managed spans only publish live moves.
            for (j = 0; j < count; ++j) moveList[start + j] = new Move(lanes[j]);
            return start + count;
        }

        /// <summary>SF19-000199: eight lanes with signed saturated subtraction before packing.</summary>
        public static int SplatPawnMovesBlocks(Move[] moveList, int start, Bitboard toBB, Direction offset)
        {
            int count = Bitboards.Popcount(toBB);
            RequireBuffer(moveList, start, count);
            if (count > 8) throw new ArgumentException("The pawn block supports at most eight destinations.");
            Span<ushort> squares = stackalloc ushort[8]; squares.Clear();
            BlockLanes.CompressSquares(toBB, squares);
            // Preserve the previous API's validation-before-write contract for invalid sources.
            for (int i = 0; i < count; ++i) _ = (Square)(int)squares[i] - offset;
            short displacement = unchecked((short)(int)offset);
            for (int i = 0; i < count; ++i)
            {
                int from = Math.Clamp((int)squares[i] - displacement, short.MinValue, short.MaxValue);
                ushort packed = unchecked((ushort)((from << Move.FromSqShift) | (squares[i] << Move.ToSqShift)));
                moveList[start + i] = new Move(packed);
            }
            return start + count;
        }
    }
}
