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
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000426: valida un candidato sin exigir que deje al propio rey fuera de jaque.</summary>
        /// <remarks>Requiere m.IsOk(). En NORMAL los bits de promoción deben ser cero,
        /// como exigen los asserts del original. No acepta los centinelas None/Null.</remarks>
        public bool PseudoLegal(Move m) => PseudoLegalCore(m, null);
        internal bool PseudoLegal(Move m, Move[] scratch) => PseudoLegalCore(m, scratch);
        private bool PseudoLegalCore(Move m, Move[]? scratch)
        {
            Color us = sideToMove;
            Square from = m.FromSq();
            Square to = m.ToSq();
            Piece pc = MovedPiece(m);
            if (m.TypeOf() != MoveType.NORMAL)
            {
                GenType type = Checkers() != 0 ? GenType.EVASIONS : GenType.NON_EVASIONS;
                if (scratch == null) return new MoveList(this, type).Contains(m);
                int count = MoveGenerator.Generate(this, scratch, 0, type);
                for (int i = 0; i < count; ++i) if (scratch[i] == m) return true;
                return false;
            }

            Debug.Assert(m.PromotionType() - PieceType.KNIGHT == PieceType.NO_PIECE_TYPE);
            if (pc == Piece.NO_PIECE || Color.ColorOf(pc) != us) return false;
            if ((Pieces(us) & to) != 0) return false;
            if (PieceType.TypeOf(pc) == PieceType.PAWN)
            {
                // Los alias promueven | y & a ulong; recuperar Bitboard antes de aplicar & Square.
                Bitboard promotionRanks = Bitboards.Rank8BB | Bitboards.Rank1BB;
                if ((promotionRanks & to) != 0) return false;
                Bitboard pawnCaptures = Attacks.AttacksBB(PieceType.PAWN, from, us) & Pieces(~us);
                bool isCapture = (pawnCaptures & to) != 0;
                bool isSinglePush = from + Direction.PawnPush(us) == to && Empty(to);
                // Un candidato arbitrario puede situar el doble avance fuera del tablero.
                // Comparar el intermedio numérico evita construir un Square inválido antes del &&.
                int doublePush = (int)from + 2 * Direction.PawnPush(us);
                bool isDoublePush = doublePush == to
                    && Rank.RelativeRank(us, from) == Rank.RANK_2 && Empty(to)
                    && Empty(to - Direction.PawnPush(us));
                if (!(isCapture || isSinglePush || isDoublePush)) return false;
            }
            else if ((Attacks.AttacksBB(PieceType.TypeOf(pc), from, Pieces()) & to) == 0)
                return false;
            if (Checkers() != 0 && PieceType.TypeOf(pc) != PieceType.KING)
            {
                if (Bitboards.MoreThanOne(Checkers())) return false;
                if ((Attacks.BetweenBB(SquareOf(PieceType.KING, us), Bitboards.Lsb(Checkers())) & to) == 0)
                    return false;
            }
            return true;
        }
    }
}
