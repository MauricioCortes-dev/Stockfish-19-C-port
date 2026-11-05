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
using System.Numerics;
using System.Runtime.InteropServices;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Canonical lanes for the original compress/pack operations. No retained pointers.</summary>
    internal static class BlockLanes
    {
        internal static Vector<T> Load<T>(ReadOnlySpan<T> values) where T : unmanaged => MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(values));
        internal static void Store<T>(Vector<T> value, Span<T> values) where T : unmanaged => MemoryMarshal.Write(MemoryMarshal.AsBytes(values), ref value);

        internal static int CompressSquares(Bitboard mask, Span<ushort> squares)
        {
            int count = 0;
            // Compress the canonical AllSquares lanes with an ascending selection mask.
            for (int group = 0; group < 8; ++group)
            {
                uint selected = (uint)((ulong)mask >> (group * 8)) & 255;
                for (int lane = 0; lane < 8; ++lane)
                    if ((selected & (1u << lane)) != 0) squares[count++] = (ushort)(group * 8 + lane);
            }
            return count;
        }
    }
}
