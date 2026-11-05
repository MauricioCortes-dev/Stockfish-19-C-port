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
    /// <summary>Dual Hyperbola: the four original u64 lanes evaluated independently, without an AVX2 requirement.</summary>
    public readonly struct DualMagic
    {
        private static readonly byte[, ] rankAttacks = BuildRankAttacks();
        private static readonly DualMagic[] magics = Build();
        public Bitboard MaskFile { get; }
        public Bitboard MaskDiag { get; }
        public Bitboard MaskNone => 0;
        public Bitboard MaskAntidiag { get; }
        public Bitboard R { get; }
        public Bitboard RR { get; }
        public int Shift { get; }

        private readonly int file;
        private DualMagic(int s)
        {
            MaskFile = SlidingGeometry.LineMask(s, 0, 1);
            MaskDiag = SlidingGeometry.LineMask(s, 1, 1);
            MaskAntidiag = SlidingGeometry.LineMask(s, -1, 1);
            R = unchecked((1UL << s) * 2);
            RR = unchecked((1UL << (63 - s)) * 2);
            Shift = 8 * (s >> 3);
            file = s & 7;
        }

        public static DualMagic For(Square square)
        {
            if (!Square.IsOk(square))
                throw new ArgumentOutOfRangeException(nameof(square));
            return magics[square];
        }

        public static byte RankAttacks(int file, int occupancy6)
        {
            if ((uint)file >= 8 || (uint)occupancy6 >= 64)
                throw new ArgumentOutOfRangeException();
            return rankAttacks[file, occupancy6];
        }

        private ulong Lane(ulong occupied, ulong mask)
        {
            ulong o = occupied & mask;
            // bswap reverses each original 128-bit pair. Its two lane swaps cancel
            // around lane-wise subtraction because RR is broadcast to every lane.
            ulong fwd = unchecked(o - (ulong)R);
            ulong rev = SlidingGeometry.ReverseBytes(unchecked(SlidingGeometry.ReverseBytes(o) - (ulong)RR));
            return (fwd ^ rev) & mask;
        }

        public (Bitboard bishop, Bitboard rook) BothAttacksBB(Bitboard occupied)
        {
            ulong o = occupied;
            ulong rank = (ulong)rankAttacks[file, (int)((o >> (Shift + 1)) & 0x3f)] << Shift;
            return (Lane(o, MaskDiag) | Lane(o, MaskAntidiag), unchecked(Lane(o, MaskFile) + rank));
        }

        private static byte[, ] BuildRankAttacks()
        {
            var table = new byte[8, 64];
            for (int f = 0; f < 8; ++f)
                for (int o = 0; o < 64; ++o)
                    table[f, o] = (byte)SlidingGeometry.Attack(PieceType.ROOK, f, (ulong)o << 1);
            return table;
        }

        private static DualMagic[] Build()
        {
            var result = new DualMagic[64];
            for (int s = 0; s < 64; ++s)
                result[s] = new DualMagic(s);
            return result;
        }
    }
}
