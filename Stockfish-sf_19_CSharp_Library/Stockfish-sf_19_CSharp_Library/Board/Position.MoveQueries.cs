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
        /// <summary>SF19-000425: comprueba la legalidad de una jugada que ya es seudolegal.</summary>
        public bool Legal(Move m)
        {
            Debug.Assert(m.IsOk());
            Color us = sideToMove;
            Square from = m.FromSq();
            Square to = m.ToSq();
            Debug.Assert(Color.ColorOf(MovedPiece(m)) == us);
            Debug.Assert(PieceOn(SquareOf(PieceType.KING, us)) == Piece.MakePiece(us, PieceType.KING));

            if (m.TypeOf() == MoveType.CASTLING)
            {
                to = Square.RelativeSquare(us, to > from ? Square.SQ_G1 : Square.SQ_C1);
                Direction step = to > from ? Direction.WEST : Direction.EAST;
                for (Square s = to; s != from; s += step)
                    if (AttackersToExist(s, Pieces(), ~us))
                        return false;
                return !chess960 || (BlockersForKing(us) & m.ToSq()) == 0;
            }
            if (PieceType.TypeOf(PieceOn(from)) == PieceType.KING)
                return !AttackersToExist(to, Pieces() ^ from, ~us);
            return (BlockersForKing(us) & from) == 0
                || (Attacks.LineBB(from, to) & Pieces(us, PieceType.KING)) != 0;
        }

        /// <summary>SF19-000427: determina si una jugada seudolegal da jaque sin ejecutarla.</summary>
        public bool GivesCheck(Move m)
        {
            Debug.Assert(m.IsOk());
            Debug.Assert(Color.ColorOf(MovedPiece(m)) == sideToMove);
            Square from = m.FromSq();
            Square to = m.ToSq();
            if ((CheckSquares(PieceType.TypeOf(PieceOn(from))) & to) != 0)
                return true;
            if ((BlockersForKing(~sideToMove) & from) != 0)
                return (Attacks.LineBB(from, to) & Pieces(~sideToMove, PieceType.KING)) == 0
                    || m.TypeOf() == MoveType.CASTLING;

            // MoveType es un valor de dominio; los if conservan las ramas del switch C++.
            if (m.TypeOf() == MoveType.NORMAL) return false;
            if (m.TypeOf() == MoveType.PROMOTION)
                return (Attacks.AttacksBB(m.PromotionType(), to, Pieces() ^ from)
                    & Pieces(~sideToMove, PieceType.KING)) != 0;
            if (m.TypeOf() == MoveType.EN_PASSANT)
            {
                Square capsq = Square.MakeSquare(Types.File.FileOf(to), Rank.RankOf(from));
                Bitboard b = (Pieces() ^ from ^ capsq) | to;
                var (bishopAttacks, rookAttacks) = Attacks.BothAttacksBB(SquareOf(PieceType.KING, ~sideToMove), b);
                return ((rookAttacks & Pieces(sideToMove, PieceType.QUEEN, PieceType.ROOK))
                    | (bishopAttacks & Pieces(sideToMove, PieceType.QUEEN, PieceType.BISHOP))) != 0;
            }
            Square rto = Square.RelativeSquare(sideToMove, to > from ? Square.SQ_F1 : Square.SQ_D1);
            return (CheckSquares(PieceType.ROOK) & rto) != 0;
        }
    }
}
