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

using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000466: atacantes con la ocupación actual.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard AttackersTo(Square square) => AttackersTo(square, Pieces());

        /// <summary>SF19-000423: occupied afecta a los rayos; no sustituye los conjuntos de piezas.</summary>
        public Bitboard AttackersTo(Square square, Bitboard occupied)
        {
            var (bishopAttacks, rookAttacks) = Attacks.BothAttacksBB(square, occupied);
            return (rookAttacks & Pieces(PieceType.ROOK, PieceType.QUEEN))
                 | (bishopAttacks & Pieces(PieceType.BISHOP, PieceType.QUEEN))
                 | (Attacks.AttacksBB(PieceType.PAWN, square, Color.BLACK) & Pieces(Color.WHITE, PieceType.PAWN))
                 | (Attacks.AttacksBB(PieceType.PAWN, square, Color.WHITE) & Pieces(Color.BLACK, PieceType.PAWN))
                 | (Attacks.AttacksBB(PieceType.KNIGHT, square) & Pieces(PieceType.KNIGHT))
                 | (Attacks.AttacksBB(PieceType.KING, square) & Pieces(PieceType.KING));
        }

        /// <summary>SF19-000424: conserva el cortocircuito y el orden de las cinco consultas originales.</summary>
        public bool AttackersToExist(Square square, Bitboard occupied, Color color)
        {
            RequireColor(color);
            return (Attacks.AttacksBB(PieceType.ROOK, square, occupied) & Pieces(color, PieceType.ROOK, PieceType.QUEEN)) != 0
                || (Attacks.AttacksBB(PieceType.BISHOP, square, occupied) & Pieces(color, PieceType.BISHOP, PieceType.QUEEN)) != 0
                || (Attacks.AttacksBB(PieceType.PAWN, square, ~color) & Pieces(color, PieceType.PAWN)) != 0
                || (Attacks.AttacksBB(PieceType.KNIGHT, square) & Pieces(color, PieceType.KNIGHT)) != 0
                || (Attacks.AttacksBB(PieceType.KING, square) & Pieces(color, PieceType.KING)) != 0;
        }

        /// <summary>SF19-000467: PieceType en ejecución sustituye al parámetro de plantilla Pt.</summary>
        public Bitboard AttacksBy(PieceType pt, Color color)
        {
            RequirePieceType(pt);
            if (pt == PieceType.PAWN)
                return color == Color.WHITE ? Bitboards.PawnAttacksBB(Color.WHITE, Pieces(Color.WHITE, PieceType.PAWN))
                                            : Bitboards.PawnAttacksBB(Color.BLACK, Pieces(Color.BLACK, PieceType.PAWN));

            Bitboard threats = 0;
            Bitboard attackers = Pieces(color, pt);
            while (attackers)
                threats |= Attacks.AttacksBB(pt, Bitboards.PopLsb(ref attackers), Pieces());
            return threats;
        }

        /// <summary>SF19-000422: bloqueadores del rey color y clavadores del color contrario.</summary>
        public void UpdateSliderBlockers(Color color)
        {
            Square kingSquare = SquareOf(PieceType.KING, color);
            StateInfo state = CurrentState;
            state.blockersForKing[color] = 0;
            state.pinners[~color] = 0;

            // Retirar todos los snipers permite reconocer también las clavadas por rayos X.
            Bitboard snipers = ((Attacks.AttacksBB(PieceType.ROOK, kingSquare) & Pieces(PieceType.QUEEN, PieceType.ROOK))
                              | (Attacks.AttacksBB(PieceType.BISHOP, kingSquare) & Pieces(PieceType.QUEEN, PieceType.BISHOP)))
                              & Pieces(~color);
            Bitboard occupancy = Pieces() ^ snipers;
            while (snipers)
            {
                Square sniperSquare = Bitboards.PopLsb(ref snipers);
                Bitboard blockers = Attacks.BetweenBB(kingSquare, sniperSquare) & occupancy;
                if (blockers != 0 && !Bitboards.MoreThanOne(blockers))
                {
                    state.blockersForKing[color] |= blockers;
                    if ((blockers & Pieces(color)) != 0)
                        state.pinners[~color] |= sniperSquare;
                }
            }
        }
    }
}
