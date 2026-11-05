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
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Ataques escalares originales de attacks.h, con geometría y wrappers escalares.</summary>
    public static partial class Attacks
    {
        private static readonly Direction[] RookDirections =
            { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };
        private static readonly Direction[] BishopDirections =
            { Direction.NORTH_EAST, Direction.SOUTH_EAST, Direction.SOUTH_WEST, Direction.NORTH_WEST };
        private static readonly int[] KnightSteps = { -17, -15, -10, -6, 6, 10, 15, 17 };
        private static readonly int[] KingSteps = { -9, -8, -7, -1, 1, 7, 8, 9 };

        /// <summary>SF19-000017: conserva el cast u8 temporal antes de comprobar el destino.</summary>
        public static Bitboard SafeDestination(Square square, int step)
        {
            // s + step debe ser int definido; no se toma el overflow signed C++ como oráculo.
            int candidate = checked((int)square + step);
            byte to = unchecked((byte)candidate);
            // No construir una Square para los 191 bytes externos a su dominio managed.
            return to < 64 && Math.Abs(File.FileOf(square) - (to & 7)) <= 2
                ? Bitboards.SquareBB((Square)to) : (Bitboard)0;
        }

        /// <summary>SF19-000018: primer bloqueador incluido; ROOK usa rectas, cualquier otro pt diagonales.</summary>
        public static Bitboard SlidingAttack(PieceType pt, Square square, Bitboard occupied)
        {
            Bitboard attacks = 0;
            foreach (Direction direction in pt == PieceType.ROOK ? RookDirections : BishopDirections)
            {
                Square from = square;
                Bitboard destination;
                while ((destination = SafeDestination(from, direction)) != 0)
                {
                    attacks |= destination;
                    if ((occupied & destination) != 0) break;
                    from += direction;
                }
            }
            return attacks;
        }

        /// <summary>SF19-000019: los ocho pasos de caballo originales.</summary>
        public static Bitboard KnightAttack(Square square)
        {
            Bitboard attacks = 0;
            foreach (int step in KnightSteps) attacks |= SafeDestination(square, step);
            return attacks;
        }

        /// <summary>SF19-000020: las ocho casillas adyacentes del rey, sin legalidad de posición.</summary>
        public static Bitboard KingAttack(Square square)
        {
            Bitboard attacks = 0;
            foreach (int step in KingSteps) attacks |= SafeDestination(square, step);
            return attacks;
        }

        /// <summary>SF19-000021: ataques en tablero vacío; peones y tipos externos incumplen el assert.</summary>
        public static Bitboard PseudoAttacks(PieceType pt, Square square)
        {
            if (pt == PieceType.ROOK || pt == PieceType.BISHOP) return SlidingAttack(pt, square, 0);
            if (pt == PieceType.QUEEN)
                return SlidingAttack(PieceType.ROOK, square, 0) | SlidingAttack(PieceType.BISHOP, square, 0);
            if (pt == PieceType.KNIGHT) return KnightAttack(square);
            if (pt == PieceType.KING) return KingAttack(square);
            throw new InvalidOperationException("pseudo_attacks requiere KNIGHT, BISHOP, ROOK, QUEEN o KING.");
        }
    }
}
