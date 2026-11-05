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
    public static partial class Attacks
    {
        // Private storage is completely built before CLR publishes this type. No caller can mutate tables.
        private static readonly Bitboard[,] pseudoAttacks;
        private static readonly Bitboard[,] pawnPushOrAttacks;
        private static readonly Bitboard[,] lineBB = new Bitboard[64, 64];
        private static readonly Bitboard[,] betweenBB = new Bitboard[64, 64];
        private static readonly Bitboard[,] rayPassBB = new Bitboard[64, 64];
        static Attacks()
        {
            pseudoAttacks = BuildPseudoAttacks();
            pawnPushOrAttacks = BuildPawnPushOrAttacks();
            for (int a = 0; a < 64; ++a)
                for (int b = 0; b < 64; ++b)
                {
                    Square s1 = a, s2 = b;
                    foreach (PieceType pt in new[] { PieceType.BISHOP, PieceType.ROOK })
                    {
                        if ((pseudoAttacks[pt, a] & s2) != 0)
                        {
                            // Bitboard & Bitboard returns ulong: restore the board domain before Square operators.
                            Bitboard common = AttacksBB(pt, s1, (Bitboard)0) & AttacksBB(pt, s2, (Bitboard)0);
                            lineBB[a, b] = common | s1 | s2;
                            betweenBB[a, b] = AttacksBB(pt, s1, Bitboards.SquareBB(s2)) & AttacksBB(pt, s2, Bitboards.SquareBB(s1));
                            rayPassBB[a, b] = AttacksBB(pt, s1, (Bitboard)0) & (AttacksBB(pt, s2, Bitboards.SquareBB(s1)) | s2);
                        }
                        betweenBB[a, b] |= s2;
                    }
                }
        }
        private static Bitboard[,] BuildPseudoAttacks()
        {
            var table = new Bitboard[8, 64];
            for (int s = 0; s < 64; ++s)
            {
                table[0, s] = Bitboards.PawnAttacksBB(Color.WHITE, Bitboards.SquareBB((Square)s));
                table[1, s] = Bitboards.PawnAttacksBB(Color.BLACK, Bitboards.SquareBB((Square)s));
                for (int pt = 2; pt <= 6; ++pt) table[pt, s] = PseudoAttacks((PieceType)pt, (Square)s);
            }
            return table;
        }
        private static Bitboard[,] BuildPawnPushOrAttacks()
        {
            var table = new Bitboard[2, 64];
            for (int c = 0; c < 2; ++c)
                for (int s = 0; s < 64; ++s)
                    table[c, s] = Bitboards.PawnSinglePushBB((Color)c, Bitboards.SquareBB((Square)s)) | pseudoAttacks[c, s];
            return table;
        }
        private static void RequireSquare(Square square)
        {
            if (!Square.IsOk(square)) throw new ArgumentOutOfRangeException(nameof(square));
        }
        private static void RequireTableType(PieceType pt)
        {
            if ((uint)(int)pt >= 8U) throw new ArgumentOutOfRangeException(nameof(pt));
        }
        public static Bitboard LineBB(Square from, Square to) { RequireSquare(from); RequireSquare(to); return lineBB[from, to]; }
        public static Bitboard BetweenBB(Square from, Square to) { RequireSquare(from); RequireSquare(to); return betweenBB[from, to]; }
        public static Bitboard RayPassBB(Square from, Square to) { RequireSquare(from); RequireSquare(to); return rayPassBB[from, to]; }
        public static Bitboard PawnPushOrAttacksBB(Color color, Square square)
        {
            RequireSquare(square);
            if ((int)color >= 2) throw new ArgumentOutOfRangeException(nameof(color));
            return pawnPushOrAttacks[color, square];
        }
        // Runtime PieceType replaces the C++ template parameter. Zero-occupancy and occupied forms remain separate.
        public static Bitboard AttacksBB(PieceType pt, Square square) => AttacksBB(pt, square, Color.COLOR_NB);
        public static Bitboard AttacksBB(PieceType pt, Square square, Color color)
        {
            RequireSquare(square); RequireTableType(pt);
            if (pt == PieceType.PAWN)
            {
                if ((int)color >= 2) throw new ArgumentOutOfRangeException(nameof(color));
                return pseudoAttacks[color, square];
            }
            return pseudoAttacks[pt, square];
        }
        public static Bitboard AttacksBB(PieceType pt, Square square, Bitboard occupied)
        {
            RequireSquare(square); RequireTableType(pt);
            if (pt == PieceType.PAWN) throw new ArgumentException("Occupied PieceType overload excludes PAWN.", nameof(pt));
            if (pt == PieceType.BISHOP || pt == PieceType.ROOK) return SlidingAttacks.AttacksBB(pt, square, occupied);
            if (pt == PieceType.QUEEN) return SlidingAttacks.AttacksBB(PieceType.BISHOP, square, occupied) | SlidingAttacks.AttacksBB(PieceType.ROOK, square, occupied);
            return pseudoAttacks[pt, square];
        }
        public static (Bitboard bishop, Bitboard rook) BothAttacksBB(Square square, Bitboard occupied)
        {
            RequireSquare(square);
            return SlidingAttacks.BothAttacksBB(square, occupied);
        }
        public static Bitboard AttacksBB(Piece piece, Square square, Bitboard occupied)
        {
            // Native pawn branch indexes a table without asserting is_ok(s); reject undefined indexing here too.
            RequireSquare(square);
            PieceType pt = PieceType.TypeOf(piece);
            return pt == PieceType.PAWN ? AttacksBB(pt, square, Color.ColorOf(piece)) : AttacksBB(pt, square, occupied);
        }
    }
}
