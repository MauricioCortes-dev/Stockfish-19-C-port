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
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    public static class NnuePermutation
    {
        public static int[] Invert(ReadOnlySpan<int> order)
        {
            int[] inverse = new int[order.Length];
            bool[] seen = new bool[order.Length];
            for (int i = 0; i < order.Length; ++i)
            {
                int j = order[i];
                if ((uint)j >= order.Length || seen[j])
                    throw new ArgumentException("Order must be a permutation.", nameof(order));
                seen[j] = true;
                inverse[j] = i;
            }

            return inverse;
        }

        public static void Permute<T>(Span<T> data, int blockSize, ReadOnlySpan<int> order)
            where T : unmanaged
        {
            Invert(order);
            if (blockSize <= 0 || order.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize));
            Span<byte> bytes = MemoryMarshal.AsBytes(data);
            int chunk = checked(blockSize * order.Length);
            if (bytes.Length % chunk != 0)
                throw new ArgumentException("Blocks must divide the complete data.");
            byte[] buffer = new byte[chunk];
            for (int i = 0; i < bytes.Length; i += chunk)
            {
                for (int j = 0; j < order.Length; ++j)
                    bytes.Slice(i + order[j] * blockSize, blockSize).CopyTo(buffer.AsSpan(j * blockSize));
                buffer.AsSpan().CopyTo(bytes.Slice(i, chunk));
            }
        }
    }
}
