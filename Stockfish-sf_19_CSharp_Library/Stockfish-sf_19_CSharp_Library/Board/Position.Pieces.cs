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
using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Types;
using SquareType = Stockfish_sf_19_CSharp_Library.Types.Square;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        public Piece PieceOn(SquareType square)
        {
            if (!SquareType.IsOk(square)) throw new InvalidOperationException("piece_on requiere una casilla jugable.");
            return board[square];
        }
        public IReadOnlyList<Piece> PieceArray() => boardView;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Empty(SquareType square) => PieceOn(square) == Piece.NO_PIECE;
        public Piece MovedPiece(Move move) => PieceOn(move.FromSq());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Pieces() => byTypeBB[PieceType.ALL_PIECES];
        public Bitboard Pieces(PieceType pt)
        {
            RequirePieceType(pt);
            return byTypeBB[pt];
        }
        public Bitboard Pieces(PieceType first, PieceType second, params PieceType[] rest)
        {
            if (rest == null) throw new ArgumentNullException(nameof(rest));
            Bitboard pieces = Pieces(first) | Pieces(second);
            foreach (PieceType pt in rest) pieces |= Pieces(pt);
            return pieces;
        }
        public Bitboard Pieces(Color color)
        {
            RequireColor(color);
            return byColorBB[color];
        }
        public Bitboard Pieces(Color color, PieceType pt) => Pieces(color) & Pieces(pt);
        public Bitboard Pieces(Color color, PieceType first, PieceType second, params PieceType[] rest) =>
            Pieces(color) & Pieces(first, second, rest);

        public int Count(PieceType pt, Color color)
        {
            RequireColor(color);
            // Count indexes Piece[], not PieceType[]. Pt==PIECE_TYPE_NB with WHITE
            // addresses B_ALL_PIECES (8) and is defined in the original template.
            Piece pc = Piece.MakePiece(color, pt);
            if ((int)pc >= pieceCount.Length) throw new ArgumentOutOfRangeException(nameof(pt));
            return pieceCount[pc];
        }
        public int Count(PieceType pt) => checked(Count(pt, Color.WHITE) + Count(pt, Color.BLACK));
        public SquareType SquareOf(PieceType pt, Color color)
        {
            if (Count(pt, color) != 1) throw new InvalidOperationException("square requiere exactamente una pieza de ese tipo y color.");
            return Bitboards.Lsb(Pieces(color, pt));
        }

        public bool DtzIsDtm() => Count(PieceType.PAWN) == 0
            && (Count(PieceType.ALL_PIECES) == 3 || (Count(PieceType.ALL_PIECES) == 4
                && Pieces(PieceType.QUEEN, PieceType.ROOK) == 0));

        public bool Capture(Move move)
        {
            if (!move.IsOk()) throw new InvalidOperationException("capture requiere una jugada distinta de none/null.");
            MoveType mt = move.TypeOf();
            if (mt == MoveType.NORMAL || mt == MoveType.PROMOTION) return !Empty(move.ToSq());
            return mt == MoveType.EN_PASSANT;
        }
        public bool CaptureStage(Move move)
        {
            if (!move.IsOk()) throw new InvalidOperationException("capture_stage requiere una jugada distinta de none/null.");
            MoveType mt = move.TypeOf();
            if (mt == MoveType.NORMAL) return !Empty(move.ToSq());
            if (mt == MoveType.PROMOTION) return !Empty(move.ToSq()) || move.PromotionType() == PieceType.QUEEN;
            return mt == MoveType.EN_PASSANT;
        }
    }
}
