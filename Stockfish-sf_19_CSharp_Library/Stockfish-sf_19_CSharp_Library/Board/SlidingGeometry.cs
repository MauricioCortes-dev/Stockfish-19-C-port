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
    // No dependency on Attacks' type initializer: table construction must not recurse into it.
    internal static class SlidingGeometry
    {
        internal static void Require(Square s, PieceType pt)
        {
            if (!Square.IsOk(s))
                throw new ArgumentOutOfRangeException(nameof(s));
            if (pt != PieceType.ROOK && pt != PieceType.BISHOP)
                throw new ArgumentOutOfRangeException(nameof(pt));
        }

        internal static ulong Ray(int s, int dx, int dy, ulong occupied = 0)
        {
            ulong result = 0;
            int x = s & 7, y = s >> 3;
            while (true)
            {
                x += dx;
                y += dy;
                if ((uint)x >= 8 || (uint)y >= 8)
                    return result;
                ulong bit = 1UL << (8 * y + x);
                result |= bit;
                if ((occupied & bit) != 0)
                    return result;
            }
        }

        internal static ulong LineMask(int s, int dx, int dy) => Ray(s, dx, dy) | Ray(s, -dx, -dy);
        internal static ulong Attack(PieceType pt, int s, ulong occupied) => pt == PieceType.ROOK ? Ray(s, 0, 1, occupied) | Ray(s, 0, -1, occupied) | Ray(s, 1, 0, occupied) | Ray(s, -1, 0, occupied) : Ray(s, 1, 1, occupied) | Ray(s, -1, -1, occupied) | Ray(s, -1, 1, occupied) | Ray(s, 1, -1, occupied);
        internal static int PopCount(ulong value)
        {
            return Bitboards.Popcount(value);
        }

        internal static ulong ReverseBytes(ulong b)
        {
            b = ((b & 0x00ff00ff00ff00ffUL) << 8) | ((b >> 8) & 0x00ff00ff00ff00ffUL);
            b = ((b & 0x0000ffff0000ffffUL) << 16) | ((b >> 16) & 0x0000ffff0000ffffUL);
            return (b << 32) | (b >> 32);
        }

        internal static ulong ReverseBits(ulong b)
        {
            b = ((b & 0x5555555555555555UL) << 1) | ((b >> 1) & 0x5555555555555555UL);
            b = ((b & 0x3333333333333333UL) << 2) | ((b >> 2) & 0x3333333333333333UL);
            b = ((b & 0x0f0f0f0f0f0f0f0fUL) << 4) | ((b >> 4) & 0x0f0f0f0f0f0f0f0fUL);
            return ReverseBytes(b);
        }
    }
}
