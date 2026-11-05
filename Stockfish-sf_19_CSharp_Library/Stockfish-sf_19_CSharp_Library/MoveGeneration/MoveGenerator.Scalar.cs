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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>Referencia escalar de los emisores, reutilizada por los generadores de jugadas.</summary>
    public static partial class MoveGenerator
    {
        /// <summary>SF19-000202: devuelve el índice posterior a la última jugada escrita.</summary>
        public static int SplatMovesScalar(Move[] moveList, int start, Square from, Bitboard toBB)
        {
            RequireBuffer(moveList, start, Bitboards.Popcount(toBB));
            while (toBB)
                moveList[start++] = new Move(from, Bitboards.PopLsb(ref toBB));
            return start;
        }

        /// <summary>SF19-000201: offset sustituye el parámetro de plantilla Direction.</summary>
        public static int SplatPawnMovesScalar(Move[] moveList, int start, Bitboard toBB, Direction offset)
        {
            RequireBuffer(moveList, start, Bitboards.Popcount(toBB));
            // Validar todos los orígenes antes de escribir: Square conserva su dominio 0..64.
            Bitboard remaining = toBB;
            while (remaining)
            {
                Square to = Bitboards.PopLsb(ref remaining);
                _ = to - offset;
            }
            while (toBB)
            {
                Square to = Bitboards.PopLsb(ref toBB);
                moveList[start++] = new Move(to - offset, to);
            }
            return start;
        }

        /// <summary>SF19-000203: conserva selección y orden Q/R/B/N de make_promotions.</summary>
        public static int MakePromotions(Move[] moveList, int start, Square to, GenType type, Direction direction, bool enemy)
        {
            bool all = type == GenType.EVASIONS || type == GenType.NON_EVASIONS;
            Square from = to - direction;
            bool queen = type == GenType.CAPTURES || all;
            bool underpromotions = (type == GenType.CAPTURES && enemy) || (type == GenType.QUIETS && !enemy) || all;
            RequireBuffer(moveList, start, (queen ? 1 : 0) + (underpromotions ? 3 : 0));
            if (queen) moveList[start++] = Move.Make(MoveType.PROMOTION, from, to, PieceType.QUEEN);
            if (underpromotions)
            {
                moveList[start++] = Move.Make(MoveType.PROMOTION, from, to, PieceType.ROOK);
                moveList[start++] = Move.Make(MoveType.PROMOTION, from, to, PieceType.BISHOP);
                moveList[start++] = Move.Make(MoveType.PROMOTION, from, to, PieceType.KNIGHT);
            }
            return start;
        }

        private static void RequireBuffer(Move[] moveList, int start, int count)
        {
            if (moveList == null) throw new ArgumentNullException(nameof(moveList));
            if (start < 0 || start > moveList.Length) throw new ArgumentOutOfRangeException(nameof(start));
            if (count > moveList.Length - start) throw new ArgumentException("El buffer no tiene espacio para todas las jugadas.", nameof(moveList));
        }
    }
}
