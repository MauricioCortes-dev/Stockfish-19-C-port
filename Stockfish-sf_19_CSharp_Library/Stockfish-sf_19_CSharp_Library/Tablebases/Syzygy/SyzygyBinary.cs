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
using System.Buffers.Binary;
namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Unsigned number/swap_endian instantiations, independent of host byte order and alignment.</summary>
    internal static class SyzygyBinary
    {
        internal static void SwapEndian(ref byte value) { }
        internal static void SwapEndian(ref ushort value) => value = BinaryPrimitives.ReverseEndianness(value);
        internal static void SwapEndian(ref uint value) => value = BinaryPrimitives.ReverseEndianness(value);
        internal static void SwapEndian(ref ulong value) => value = BinaryPrimitives.ReverseEndianness(value);
        internal static byte Number8(TBFile file, long offset, bool littleEndian) => (byte)Number(file, offset, 1, littleEndian);
        internal static ushort Number16(TBFile file, long offset, bool littleEndian) => (ushort)Number(file, offset, 2, littleEndian);
        internal static uint Number32(TBFile file, long offset, bool littleEndian) => (uint)Number(file, offset, 4, littleEndian);
        internal static ulong Number64(TBFile file, long offset, bool littleEndian) => Number(file, offset, 8, littleEndian);
        private static ulong Number(TBFile file, long offset, int size, bool littleEndian)
        {
            file.Require(offset, (ulong)size);
            ulong result = 0;
            for (int i = 0; i < size; ++i)
                result = (result << 8) | file.U8(offset + (littleEndian ? size - i - 1 : i));
            return result;
        }
    }
}
