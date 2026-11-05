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
    /// <summary>Immutable process-wide selection, and explicit backend queries for diagnostics.</summary>
    public static class SlidingAttacks
    {
        // Read once before the first attack query. No mutable global switch can change
        // algorithms halfway through a search. Optional host/acceptance configuration.
        public static SlidingAttackBackend Backend { get; } = ReadBackend();

        private static SlidingAttackBackend ReadBackend()
        {
            string? value = Environment.GetEnvironmentVariable("STOCKFISH_SLIDING_ATTACKS");
            if (string.IsNullOrEmpty(value))
                return IntPtr.Size == 8 ? SlidingAttackBackend.Magic64 : SlidingAttackBackend.Magic32;
            if (Enum.TryParse(value, true, out SlidingAttackBackend backend) && Enum.IsDefined(typeof(SlidingAttackBackend), backend))
                return backend;
            throw new ArgumentException("Unknown STOCKFISH_SLIDING_ATTACKS backend: " + value);
        }

        public static Bitboard AttacksBB(PieceType type, Square square, Bitboard occupied) => AttacksBB(type, square, occupied, Backend);
        public static Bitboard AttacksBB(PieceType type, Square square, Bitboard occupied, SlidingAttackBackend backend)
        {
            SlidingGeometry.Require(square, type);
            switch (backend)
            {
                case SlidingAttackBackend.Direct:
                    return Attacks.SlidingAttack(type, square, occupied);
                case SlidingAttackBackend.Magic64:
                    return MagicTables.ForWidth(true).Get(square, type).AttacksBB(occupied);
                case SlidingAttackBackend.Magic32:
                    return MagicTables.ForWidth(false).Get(square, type).AttacksBB(occupied);
                case SlidingAttackBackend.Hyperbola:
                    return Hyperbolas[square, (int)type - (int)PieceType.BISHOP].AttacksBB(square, occupied);
                case SlidingAttackBackend.DualHyperbola:
                    var pair = DualMagic.For(square).BothAttacksBB(occupied);
                    return type == PieceType.BISHOP ? pair.bishop : pair.rook;
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend));
            }
        }

        public static (Bitboard bishop, Bitboard rook) BothAttacksBB(Square square, Bitboard occupied)
        {
            if (Backend == SlidingAttackBackend.DualHyperbola)
                return DualMagic.For(square).BothAttacksBB(occupied);
            return (AttacksBB(PieceType.BISHOP, square, occupied), AttacksBB(PieceType.ROOK, square, occupied));
        }

        private static readonly HyperbolaMagic[, ] Hyperbolas = BuildHyperbolas();
        private static HyperbolaMagic[, ] BuildHyperbolas()
        {
            var result = new HyperbolaMagic[64, 2];
            for (int s = 0; s < 64; ++s)
            {
                result[s, 0] = HyperbolaMagic.For((Square)s, PieceType.BISHOP);
                result[s, 1] = HyperbolaMagic.For((Square)s, PieceType.ROOK);
            }

            return result;
        }
    }
}
