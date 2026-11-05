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
using System.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    public static partial class MoveGenerator
    {
        /// <summary>SF19-000207/000208: escribe en el buffer y devuelve el índice final exclusivo.</summary>
        /// <remarks>Requiere MAX_MOVES casillas libres y una Position preparada mediante Set.
        /// EVASIONS requiere jaque; CAPTURES, QUIETS y NON_EVASIONS requieren ausencia de jaque.
        /// LEGAL selecciona automáticamente la categoría y conserva el filtrado por intercambio original.</remarks>
        public static int Generate(Position pos, Move[] moveList, int start, GenType type)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            RequireBuffer(moveList, start, ChessConstants.MAX_MOVES);
            bool inCheck = pos.Checkers() != 0;
            if (type != GenType.LEGAL && (type == GenType.EVASIONS) != inCheck)
                throw new ArgumentException("La categoría de generación no corresponde al estado de jaque.", nameof(type));

            if (type != GenType.LEGAL)
                return GenerateAll(pos, moveList, start, pos.SideToMove(), type);

            Color us = pos.SideToMove();
            Bitboard pinned = pos.BlockersForKing(us) & pos.Pieces(us);
            Square ksq = pos.SquareOf(PieceType.KING, us);
            int cur = start;
            int end = GenerateAll(pos, moveList, start, us, inCheck ? GenType.EVASIONS : GenType.NON_EVASIONS);
            while (cur != end)
                if (((pinned & moveList[cur].FromSq()) != 0 || moveList[cur].FromSq() == ksq
                    || moveList[cur].TypeOf() == MoveType.EN_PASSANT) && !pos.Legal(moveList[cur]))
                    moveList[cur] = moveList[--end];
                else
                    ++cur;
            return end;
        }

        // SF19-000205: Us/Pt de las plantillas pasan a ser valores de dominio.
        private static int GenerateMoves(Position pos, Move[] moveList, int start, Bitboard target, Color us, PieceType pt)
        {
            Debug.Assert(pt != PieceType.KING && pt != PieceType.PAWN);
            Bitboard bb = pos.Pieces(us, pt);
            while (bb)
            {
                Square from = Bitboards.PopLsb(ref bb);
                Bitboard b = Attacks.AttacksBB(pt, from, pos.Pieces()) & target;
                start = SplatMoves(moveList, start, from, b);
            }
            return start;
        }

        // SF19-000204: conserva el orden de avances, promociones, capturas y captura al paso.
        private static int GeneratePawnMoves(Position pos, Move[] moveList, int start, Bitboard target, Color us, GenType type)
        {
            Color them = ~us;
            Bitboard tRank7BB = us == Color.WHITE ? Bitboards.Rank7BB : Bitboards.Rank2BB;
            Bitboard tRank3BB = us == Color.WHITE ? Bitboards.Rank3BB : Bitboards.Rank6BB;
            Direction up = Direction.PawnPush(us);
            Direction upRight = us == Color.WHITE ? Direction.NORTH_EAST : Direction.SOUTH_WEST;
            Direction upLeft = us == Color.WHITE ? Direction.NORTH_WEST : Direction.SOUTH_EAST;
            Bitboard emptySquares = ~pos.Pieces();
            Bitboard enemies = type == GenType.EVASIONS ? pos.Checkers() : pos.Pieces(them);
            Bitboard pawnsOn7 = pos.Pieces(us, PieceType.PAWN) & tRank7BB;
            Bitboard pawnsNotOn7 = pos.Pieces(us, PieceType.PAWN) & ~tRank7BB;

            if (type != GenType.CAPTURES)
            {
                Bitboard b1 = Bitboards.Shift(pawnsNotOn7, up) & emptySquares;
                Bitboard b2 = Bitboards.Shift(b1 & tRank3BB, up) & emptySquares;
                if (type == GenType.EVASIONS) { b1 &= target; b2 &= target; }
                start = SplatPawnMoves(moveList, start, b1, up);
                start = SplatPawnMoves(moveList, start, b2, up + up);
            }
            if (pawnsOn7)
            {
                Bitboard b1 = Bitboards.Shift(pawnsOn7, upRight) & enemies;
                Bitboard b2 = Bitboards.Shift(pawnsOn7, upLeft) & enemies;
                Bitboard b3 = Bitboards.Shift(pawnsOn7, up) & emptySquares;
                if (type == GenType.EVASIONS) b3 &= target;
                while (b1) start = MakePromotions(moveList, start, Bitboards.PopLsb(ref b1), type, upRight, true);
                while (b2) start = MakePromotions(moveList, start, Bitboards.PopLsb(ref b2), type, upLeft, true);
                while (b3) start = MakePromotions(moveList, start, Bitboards.PopLsb(ref b3), type, up, false);
            }
            if (type == GenType.CAPTURES || type == GenType.EVASIONS || type == GenType.NON_EVASIONS)
            {
                Bitboard b1 = Bitboards.Shift(pawnsNotOn7, upRight) & enemies;
                Bitboard b2 = Bitboards.Shift(pawnsNotOn7, upLeft) & enemies;
                start = SplatPawnMoves(moveList, start, b1, upRight);
                start = SplatPawnMoves(moveList, start, b2, upLeft);
                if (pos.EpSquare() != Square.SQ_NONE)
                {
                    Debug.Assert(Rank.RankOf(pos.EpSquare()) == Rank.RelativeRank(us, Rank.RANK_6));
                    if (type == GenType.EVASIONS && (target & (pos.EpSquare() + up)) != 0)
                        return start;
                    b1 = pawnsNotOn7 & Attacks.AttacksBB(PieceType.PAWN, pos.EpSquare(), them);
                    Debug.Assert(b1 != 0);
                    while (b1) moveList[start++] = Move.Make(MoveType.EN_PASSANT, Bitboards.PopLsb(ref b1), pos.EpSquare());
                }
            }
            return start;
        }

        // SF19-000206: en jaque doble solo se generan movimientos del rey.
        private static int GenerateAll(Position pos, Move[] moveList, int start, Color us, GenType type)
        {
            Debug.Assert(type != GenType.LEGAL);
            Square ksq = pos.SquareOf(PieceType.KING, us);
            Bitboard target = 0;
            if (type != GenType.EVASIONS || !Bitboards.MoreThanOne(pos.Checkers()))
            {
                target = type == GenType.EVASIONS ? Attacks.BetweenBB(ksq, Bitboards.Lsb(pos.Checkers()))
                       : type == GenType.NON_EVASIONS ? (Bitboard)~pos.Pieces(us)
                       : type == GenType.CAPTURES ? pos.Pieces(~us)
                       : (Bitboard)~pos.Pieces();
                start = GeneratePawnMoves(pos, moveList, start, target, us, type);
                start = GenerateMoves(pos, moveList, start, target, us, PieceType.KNIGHT);
                start = GenerateMoves(pos, moveList, start, target, us, PieceType.BISHOP);
                start = GenerateMoves(pos, moveList, start, target, us, PieceType.ROOK);
                start = GenerateMoves(pos, moveList, start, target, us, PieceType.QUEEN);
            }
            Bitboard b = Attacks.AttacksBB(PieceType.KING, ksq) & (type == GenType.EVASIONS ? (Bitboard)~pos.Pieces(us) : target);
            start = SplatMoves(moveList, start, ksq, b);
            if ((type == GenType.QUIETS || type == GenType.NON_EVASIONS) && pos.CanCastle(us & CastlingRights.ANY_CASTLING))
                for (int side = 0; side < 2; ++side)
                {
                    CastlingRights cr = us & (side == 0 ? CastlingRights.KING_SIDE : CastlingRights.QUEEN_SIDE);
                    if (!pos.CastlingImpeded(cr) && pos.CanCastle(cr))
                        moveList[start++] = Move.Make(MoveType.CASTLING, ksq, pos.CastlingRookSquare(cr));
                }
            return start;
        }
    }
}
