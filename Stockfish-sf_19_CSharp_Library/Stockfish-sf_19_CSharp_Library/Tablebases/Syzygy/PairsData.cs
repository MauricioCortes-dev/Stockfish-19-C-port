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

using Stockfish_sf_19_CSharp_Library.Types;
namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Per-side/per-pawn-file indexing and recursive-pairing metadata; frozen on publication.</summary>
    internal sealed class PairsData
    {
        internal byte Flags, MaxSymLen, MinSymLen;
        internal uint BlocksNum, BlockLengthSize;
        internal ulong SizeofBlock, Span, SparseIndexSize;
        internal long LowestSym, BlockLength, SparseIndex, Data, DataEnd;
        internal ulong[] Base64 = new ulong[0];
        internal byte[] Symlen = new byte[0];
        internal LR[] Btree = new LR[0];
        internal readonly Piece[] Pieces = new Piece[7];
        internal readonly ulong[] GroupIdx = new ulong[8];
        internal readonly int[] GroupLen = new int[8];
        internal readonly ushort[] MapIdx = new ushort[4];
        internal readonly int[] MapCount = new int[4];
    }
}
