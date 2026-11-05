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

using System.Runtime.InteropServices;
namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Two 12-bit Huffman children, or a leaf whose right child is 0xFFF.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct LR
    {
        internal enum Side { Left, Right }
        private readonly byte a, b, c;
        internal LR(byte a, byte b, byte c) { this.a = a; this.b = b; this.c = c; }
        internal ushort Get(Side side) => side == Side.Left ? (ushort)((b & 15) << 8 | a) : (ushort)(c << 4 | b >> 4);
        internal static LR Read(TBFile file, long offset) => new LR(file.U8(offset), file.U8(offset + 1), file.U8(offset + 2));
    }
}
