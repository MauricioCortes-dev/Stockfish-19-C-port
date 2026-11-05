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
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Original seeded search. One immutable table per width, built lazily and safely published.</summary>
    public sealed class MagicTables
    {
        private static readonly Lazy<MagicTables> tables64 = new Lazy<MagicTables>(() => new MagicTables(true));
        private static readonly Lazy<MagicTables> tables32 = new Lazy<MagicTables>(() => new MagicTables(false));
        private readonly Magic[, ] magics = new Magic[64, 2];
        public static MagicTables ForWidth(bool is64Bit) => is64Bit ? tables64.Value : tables32.Value;
        private MagicTables(bool is64Bit)
        {
            Build(PieceType.ROOK, new ulong[0x19000], is64Bit);
            Build(PieceType.BISHOP, new ulong[0x1480], is64Bit);
        }

        public Magic Get(Square square, PieceType type)
        {
            SlidingGeometry.Require(square, type);
            return magics[square, (int)type - (int)PieceType.BISHOP];
        }

        private void Build(PieceType pt, ulong[] table, bool is64Bit)
        {
            int[, ] seeds =
            {
                {
                    8977,
                    44560,
                    54343,
                    38998,
                    5731,
                    95205,
                    104912,
                    17020
                },
                {
                    728,
                    10316,
                    55013,
                    32803,
                    12281,
                    15100,
                    16645,
                    255
                }
            };
            var occupancy = new ulong[4096];
            var reference = new ulong[4096];
            var epoch = new int[4096];
            int count = 0, offset = 0;
            for (int s = 0; s < 64; ++s)
            {
                ulong rank = 0xffUL << (8 * (s >> 3)), file = 0x0101010101010101UL << (s & 7);
                ulong edges = (0xff000000000000ffUL & ~rank) | (0x8181818181818181UL & ~file);
                ulong mask = SlidingGeometry.Attack(pt, s, 0) & ~edges;
                int shift = (is64Bit ? 64 : 32) - SlidingGeometry.PopCount(mask), size = 0;
                ulong b = 0;
                do
                {
                    occupancy[size] = b;
                    reference[size++] = SlidingGeometry.Attack(pt, s, b);
                    b = unchecked(b - mask) & mask;
                }
                while (b != 0);
                var rng = new PRNG((ulong)seeds[is64Bit ? 1 : 0, s >> 3]);
                Magic candidate;
                while (true)
                {
                    ulong multiplier;
                    do
                    {
                        multiplier = rng.SparseRand();
                    }
                    while (SlidingGeometry.PopCount(unchecked(multiplier * mask) >> 56) < 6);
                    candidate = new Magic(table, mask, multiplier, shift, offset, is64Bit);
                    ++count;
                    int i;
                    for (i = 0; i < size; ++i)
                    {
                        int index = (int)candidate.Index(occupancy[i]);
                        if (epoch[index] < count)
                        {
                            epoch[index] = count;
                            table[offset + index] = reference[i];
                        }
                        else if (table[offset + index] != reference[i])
                            break;
                    }

                    if (i == size)
                        break;
                }

                magics[s, (int)pt - (int)PieceType.BISHOP] = candidate;
                offset += size;
            }

            if (offset != table.Length)
                throw new InvalidOperationException("Unexpected original magic table size.");
        }
    }
}
