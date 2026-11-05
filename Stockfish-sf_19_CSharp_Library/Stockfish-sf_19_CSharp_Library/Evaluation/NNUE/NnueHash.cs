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
    // The exact misc hash_bytes/hash_combine uses required by NNUE. Generic misc helpers retain their own inventory.
    internal static class NnueHash
    {
        internal static ulong Bytes(ReadOnlySpan<byte> data) => Infrastructure.StableHash.Bytes(data);

        internal static ulong Native(ulong value) => UIntPtr.Size == 4 ? (uint)value : value;
        internal static ulong Combine(ulong seed, ulong value) => Native(unchecked(seed ^ (Native(value) + 0x9e3779b9UL + Native(seed << 6) + (seed >> 2))));
    }
}
