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
    /// <summary>Six-byte decoded sparse index entry; fields are read explicitly little endian.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct SparseEntry
    {
        internal readonly uint Block;
        internal readonly ushort Offset;
        internal SparseEntry(uint block, ushort offset) { Block = block; Offset = offset; }
        internal static SparseEntry Read(TBFile file, long offset) => new SparseEntry(file.U32(offset), file.U16(offset + 4));
    }
}
