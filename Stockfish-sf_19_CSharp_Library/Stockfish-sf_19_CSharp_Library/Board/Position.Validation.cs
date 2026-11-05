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

using System.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000445: diagnóstico mediante asserts; no es un validador que devuelva false.</summary>
        public bool PosIsOk()
        {
            if ((sideToMove != Color.WHITE && sideToMove != Color.BLACK)
                || PieceOn(SquareOf(PieceType.KING, Color.WHITE)) != Piece.W_KING
                || PieceOn(SquareOf(PieceType.KING, Color.BLACK)) != Piece.B_KING
                || (EpSquare() != Square.SQ_NONE && Rank.RelativeRank(sideToMove, EpSquare()) != Rank.RANK_6))
                Debug.Assert(false, "pos_is_ok: Default");

            if (Count(PieceType.KING, Color.WHITE) != 1 || Count(PieceType.KING, Color.BLACK) != 1
                || AttackersToExist(SquareOf(PieceType.KING, ~sideToMove), Pieces(), sideToMove))
                Debug.Assert(false, "pos_is_ok: Kings");

            if ((Pieces(PieceType.PAWN) & (Bitboards.Rank1BB | Bitboards.Rank8BB)) != 0
                || Count(PieceType.PAWN, Color.WHITE) > 8 || Count(PieceType.PAWN, Color.BLACK) > 8)
                Debug.Assert(false, "pos_is_ok: Pawns");

            if (EpSquare() != Square.SQ_NONE)
            {
                Square ksq = SquareOf(PieceType.KING, sideToMove);
                Bitboard captured = (EpSquare() + Direction.PawnPush(~sideToMove)) & Pieces(~sideToMove, PieceType.PAWN);
                Bitboard pawns = Attacks.AttacksBB(PieceType.PAWN, EpSquare(), ~sideToMove) & Pieces(sideToMove, PieceType.PAWN);
                Bitboard potentialCheckers = Pieces(~sideToMove) ^ captured;
                if (captured == 0 || pawns == 0
                    || ((AttackersTo(ksq, (Bitboard)(Pieces() ^ captured) ^ EpSquare() ^ Bitboards.Lsb(pawns)) & potentialCheckers) != 0
                        && (AttackersTo(ksq, (Bitboard)(Pieces() ^ captured) ^ EpSquare() ^ Bitboards.Msb(pawns)) & potentialCheckers) != 0))
                    Debug.Assert(false, "pos_is_ok: En passant square");
            }

            if ((Pieces(Color.WHITE) & Pieces(Color.BLACK)) != 0
                || (Pieces(Color.WHITE) | Pieces(Color.BLACK)) != Pieces()
                || Bitboards.Popcount(Pieces(Color.WHITE)) > 16 || Bitboards.Popcount(Pieces(Color.BLACK)) > 16)
                Debug.Assert(false, "pos_is_ok: Bitboards");

            for (PieceType p1 = PieceType.PAWN; p1 <= PieceType.KING; ++p1)
                for (PieceType p2 = PieceType.PAWN; p2 <= PieceType.KING; ++p2)
                    if (p1 != p2 && (Pieces(p1) & Pieces(p2)) != 0)
                        Debug.Assert(false, "pos_is_ok: Bitboards");

            for (int pieceIndex = 0; pieceIndex < Zobrist.Pieces.Count; ++pieceIndex)
            {
                Piece pc = Zobrist.Pieces[pieceIndex];
                int count = 0;
                foreach (Piece onBoard in board)
                    if (onBoard == pc) ++count;
                if (pieceCount[pc] != Bitboards.Popcount(Pieces(Color.ColorOf(pc), PieceType.TypeOf(pc)))
                    || pieceCount[pc] != count)
                    Debug.Assert(false, "pos_is_ok: Pieces");
            }

            foreach (Color c in new[] { Color.WHITE, Color.BLACK })
                foreach (CastlingRights cr in new[] { c & CastlingRights.KING_SIDE, c & CastlingRights.QUEEN_SIDE })
                {
                    if (!CanCastle(cr)) continue;
                    if (PieceOn(CastlingRookSquare(cr)) != Piece.MakePiece(c, PieceType.ROOK)
                        || castlingRightsMask[castlingRookSquare[cr]] != cr
                        || (castlingRightsMask[SquareOf(PieceType.KING, c)] & cr) != cr)
                        Debug.Assert(false, "pos_is_ok: Castling");
                }

            Debug.Assert(MaterialKeyIsOk(), "pos_is_ok: materialKey");
            return true;
        }
    }
}
