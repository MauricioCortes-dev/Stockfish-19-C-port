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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    public static class NnueConstants
    {
        public const uint Version = 0x6A448AFA;
        public const int OutputScale = 16, WeightScaleBits = 6, FtMaxVal = 255, HiddenOneVal = 128;
        public const int CacheLineSize = 64, MaxSimdWidth = 32;
        public const int L1 = 1024, L2 = 32, L3 = 32, PSQTBuckets = 8, LayerStacks = 8;
        public const string Leb128MagicString = "COMPRESSED_LEB128";
        public static int CeilToMultiple(int n, int basis)
        {
            if (basis == 0)
                throw new DivideByZeroException();
            return checked((n + basis - 1) / basis * basis);
        }

        public static uint CeilToMultiple(uint n, uint basis)
        {
            if (basis == 0)
                throw new DivideByZeroException();
            return unchecked((n + basis - 1) / basis * basis);
        }

        public static long CeilToMultiple(long n, long basis)
        {
            if (basis == 0)
                throw new DivideByZeroException();
            return checked((n + basis - 1) / basis * basis);
        }

        public static ulong CeilToMultiple(ulong n, ulong basis)
        {
            if (basis == 0)
                throw new DivideByZeroException();
            return unchecked((n + basis - 1) / basis * basis);
        }

        public static sbyte CeilToMultiple(sbyte n, sbyte basis) => unchecked((sbyte)CeilToMultiple((int)n, (int)basis));
        public static byte CeilToMultiple(byte n, byte basis) => unchecked((byte)CeilToMultiple((int)n, (int)basis));
        public static short CeilToMultiple(short n, short basis) => unchecked((short)CeilToMultiple((int)n, (int)basis));
        public static ushort CeilToMultiple(ushort n, ushort basis) => unchecked((ushort)CeilToMultiple((int)n, (int)basis));
    }
}
