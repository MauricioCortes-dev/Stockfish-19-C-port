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
using static Stockfish_sf_19_CSharp_Library.Types.Values;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000438: evaluación estática de intercambios contra un umbral, sin mutar Position.</summary>
        /// <remarks>Conserva la aproximación original para jugadas especiales. No prueba legalidad.</remarks>
        public bool SeeGe(Move m, int threshold = 0)
        {
            Debug.Assert(m.IsOk());
            if (m.TypeOf() != MoveType.NORMAL) return Value.VALUE_ZERO >= threshold;

            Square from = m.FromSq(), to = m.ToSq();
            Debug.Assert(PieceOn(from) != Piece.NO_PIECE);
            int swap = PieceValue[PieceOn(to)] - threshold;
            if (swap < 0) return false;
            swap = PieceValue[PieceOn(from)] - swap;
            if (swap <= 0) return true;

            Debug.Assert(Color.ColorOf(PieceOn(from)) == sideToMove);
            Bitboard occupied = Pieces() ^ from ^ to;
            Color stm = sideToMove;
            Bitboard attackers = AttackersTo(to, occupied);
            Bitboard stmAttackers, bb;
            int res = 1;
            while (true)
            {
                stm = ~stm;
                attackers &= occupied;
                if ((stmAttackers = attackers & Pieces(stm)) == 0) break;
                if ((Pinners(~stm) & occupied) != 0)
                {
                    stmAttackers &= (Bitboard)~BlockersForKing(stm);
                    if (stmAttackers == 0) break;
                }
                res ^= 1;
                if ((bb = stmAttackers & Pieces(PieceType.PAWN)) != 0)
                {
                    if ((swap = PawnValue - swap) < res) break;
                    occupied ^= Bitboards.LeastSignificantSquareBB(bb);
                    attackers |= Attacks.AttacksBB(PieceType.BISHOP, to, occupied) & Pieces(PieceType.BISHOP, PieceType.QUEEN);
                }
                else if ((bb = stmAttackers & Pieces(PieceType.KNIGHT)) != 0)
                {
                    if ((swap = KnightValue - swap) < res) break;
                    occupied ^= Bitboards.LeastSignificantSquareBB(bb);
                }
                else if ((bb = stmAttackers & Pieces(PieceType.BISHOP)) != 0)
                {
                    if ((swap = BishopValue - swap) < res) break;
                    occupied ^= Bitboards.LeastSignificantSquareBB(bb);
                    attackers |= Attacks.AttacksBB(PieceType.BISHOP, to, occupied) & Pieces(PieceType.BISHOP, PieceType.QUEEN);
                }
                else if ((bb = stmAttackers & Pieces(PieceType.ROOK)) != 0)
                {
                    if ((swap = RookValue - swap) < res) break;
                    occupied ^= Bitboards.LeastSignificantSquareBB(bb);
                    attackers |= Attacks.AttacksBB(PieceType.ROOK, to, occupied) & Pieces(PieceType.ROOK, PieceType.QUEEN);
                }
                else if ((bb = stmAttackers & Pieces(PieceType.QUEEN)) != 0)
                {
                    swap = QueenValue - swap;
                    Debug.Assert(swap >= res);
                    occupied ^= Bitboards.LeastSignificantSquareBB(bb);
                    var (bishopAttacks, rookAttacks) = Attacks.BothAttacksBB(to, occupied);
                    attackers |= (bishopAttacks & Pieces(PieceType.BISHOP, PieceType.QUEEN))
                               | (rookAttacks & Pieces(PieceType.ROOK, PieceType.QUEEN));
                }
                else return ((attackers & (Bitboard)~Pieces(stm)) != 0 ? res ^ 1 : res) != 0;
            }
            return res != 0;
        }
    }
}
