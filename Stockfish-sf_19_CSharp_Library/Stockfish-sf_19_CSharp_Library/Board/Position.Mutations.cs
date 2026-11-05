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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        private static Color MutationColor(Piece pc)
        {
            Color color = Color.ColorOf(pc);
            RequireColor(color);
            return color;
        }
        private static void RequireMutationSquare(Square square)
        {
            if (!Square.IsOk(square)) throw new ArgumentOutOfRangeException(nameof(square));
        }

        /// <summary>SF19-000487: inserta en la representación; no actualiza StateInfo ni valida legalidad.</summary>
        public void PutPiece(Piece pc, Square square)
        {
            Color color = MutationColor(pc);
            RequireMutationSquare(square);
            board[square] = pc;
            byTypeBB[PieceType.ALL_PIECES] |= byTypeBB[PieceType.TypeOf(pc)] |= square;
            byColorBB[color] |= square;
            pieceCount[pc]++;
            pieceCount[Piece.MakePiece(color, PieceType.ALL_PIECES)]++;
        }
        public void PutPiece(Piece pc, Square square, ref DirtyThreats dts)
        {
            PutPiece(pc, square);
            UpdatePieceThreats(pc, true, square, ref dts, ulong.MaxValue);
        }

        /// <summary>SF19-000488: el registro de amenazas observa la pieza antes de retirarla.</summary>
        public void RemovePiece(Square square)
        {
            Piece pc = PieceOn(square);
            Color color = MutationColor(pc);
            byTypeBB[PieceType.ALL_PIECES] ^= square;
            byTypeBB[PieceType.TypeOf(pc)] ^= square;
            byColorBB[color] ^= square;
            board[square] = Piece.NO_PIECE;
            pieceCount[pc]--;
            pieceCount[Piece.MakePiece(color, PieceType.ALL_PIECES)]--;
        }
        public void RemovePiece(Square square, ref DirtyThreats dts)
        {
            Piece pc = PieceOn(square);
            _ = MutationColor(pc);
            UpdatePieceThreats(pc, false, square, ref dts, ulong.MaxValue);
            RemovePiece(square);
        }

        // SF19-000489: conserva la visibilidad privada del original.
        private void MovePiece(Square from, Square to)
        {
            Piece pc = PieceOn(from);
            Color color = MutationColor(pc);
            RequireMutationSquare(to);
            Bitboard fromTo = from | to;
            byTypeBB[PieceType.ALL_PIECES] ^= fromTo;
            byTypeBB[PieceType.TypeOf(pc)] ^= fromTo;
            byColorBB[color] ^= fromTo;
            board[from] = Piece.NO_PIECE;
            board[to] = pc;
        }
        private void MovePiece(Square from, Square to, ref DirtyThreats dts)
        {
            Piece pc = PieceOn(from);
            _ = MutationColor(pc);
            RequireMutationSquare(to);
            Bitboard fromTo = from | to;
            UpdatePieceThreats(pc, false, from, ref dts, fromTo);
            MovePiece(from, to);
            UpdatePieceThreats(pc, true, to, ref dts, fromTo);
        }

        /// <summary>SF19-000490: sustituye sin procesar rayos; se usa también para promociones.</summary>
        public void SwapPiece(Square square, Piece pc)
        {
            _ = MutationColor(pc);
            RemovePiece(square);
            PutPiece(pc, square);
        }
        public void SwapPiece(Square square, Piece pc, ref DirtyThreats dts)
        {
            _ = MutationColor(pc);
            Piece old = PieceOn(square);
            RemovePiece(square);
            UpdatePieceThreats(old, false, square, ref dts, ulong.MaxValue, computeRay: false);
            PutPiece(pc, square);
            UpdatePieceThreats(pc, true, square, ref dts, ulong.MaxValue, computeRay: false);
        }
    }
}
