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
using System.IO;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Bounded binary metadata reader and canonical Huffman/recursive-pair decoder.</summary>
    internal static class SyzygyDecoder
    {
        internal const int SymCount = 4096;
        internal static long Set(TBTable e, PairsData[,] items, TBFile file, SyzygyIndex index)
        {
            long data = 4;
            byte header = file.U8(data++);
            if (e.HasPawns != ((header & 2) != 0) || (e.Key != e.Key2) != ((header & 1) != 0)) throw file.Corrupt("material flags");
            int sides = e.Type == TBType.WDL && e.Key != e.Key2 ? 2 : 1, files = e.HasPawns ? 4 : 1;
            bool pp = e.HasPawns && e.PawnCount[1] != 0;
            for (int f = 0; f < files; ++f)
            {
                file.Require(data, (ulong)(1 + (pp ? 1 : 0) + e.PieceCount));
                int first = file.U8(data++), second = pp ? file.U8(data++) : 255;
                for (int s = 0; s < sides; ++s)
                {
                    var d = items[s, f] = new PairsData();
                    for (int k = 0; k < e.PieceCount; ++k)
                    {
                        int pc = s == 0 ? file.U8(data + k) & 15 : file.U8(data + k) >> 4;
                        if ((pc & 7) < 1 || (pc & 7) > 6) throw file.Corrupt("piece code");
                        d.Pieces[k] = pc;
                    }
                    ValidateMaterial(e, d, file);
                    SetGroups(e, d, new[] { s == 0 ? first & 15 : first >> 4, s == 0 ? second & 15 : second >> 4 }, f, file, index);
                }
                data += e.PieceCount;
            }
            data += data & 1;
            for (int f = 0; f < files; ++f)
                for (int s = 0; s < sides; ++s) data = SetSizes(items[s, f], data, file);
            long map = data;
            if (e.Type == TBType.DTZ) data = SetDtzMap(items, files, data, file);
            for (int f = 0; f < files; ++f)
                for (int s = 0; s < sides; ++s)
                {
                    var d = items[s, f]; d.SparseIndex = data;
                    file.Require(data, d.SparseIndexSize, 6); data += checked((long)(d.SparseIndexSize * 6));
                }
            for (int f = 0; f < files; ++f)
                for (int s = 0; s < sides; ++s)
                {
                    var d = items[s, f]; d.BlockLength = data;
                    file.Require(data, d.BlockLengthSize, 2); data += (long)d.BlockLengthSize * 2;
                }
            for (int f = 0; f < files; ++f)
                for (int s = 0; s < sides; ++s)
                {
                    var d = items[s, f]; data = checked(data + 63) & ~63L; d.Data = data;
                    file.Require(data, d.BlocksNum, d.SizeofBlock); data += checked((long)(d.BlocksNum * d.SizeofBlock)); d.DataEnd = data;
                }
            return map;
        }

        internal static long SetDtzMap(PairsData[,] items, int files, long data, TBFile file)
        {
            long map = data;
            for (int f = 0; f < files; ++f)
            {
                PairsData d = items[0, f];
                if ((d.Flags & TBFlag.Mapped) == 0) continue;
                bool wide = (d.Flags & TBFlag.Wide) != 0;
                if (wide) data += data & 1;
                for (int i = 0; i < 4; ++i)
                {
                    int count = wide ? file.U16(data) : file.U8(data);
                    d.MapIdx[i] = unchecked((ushort)((wide ? (data - map) / 2 : data - map) + 1));
                    d.MapCount[i] = count;
                    ulong step = (ulong)(count + 1) * (wide ? 2UL : 1UL);
                    file.Require(data, step); data += (long)step;
                }
            }
            return data + (data & 1);
        }

        private static void ValidateMaterial(TBTable e, PairsData d, TBFile file)
        {
            var actual = new int[16]; var expected = new int[16];
            for (int i = 0; i < e.PieceCount; ++i) ++actual[d.Pieces[i]];
            int color = 0;
            foreach (char c in e.Code)
                if (c == 'v') color = 8;
                else ++expected[" PNBRQK".IndexOf(c) + color];
            for (int i = 0; i < 16; ++i) if (actual[i] != expected[i]) throw file.Corrupt("piece material");
            if (e.HasPawns)
            {
                int lead = d.Pieces[0];
                if (PieceType.TypeOf(d.Pieces[0]) != PieceType.PAWN || actual[lead] != e.PawnCount[0]) throw file.Corrupt("leading pawns");
                for (int i = 0; i < e.PawnCount[0]; ++i) if (d.Pieces[i] != lead) throw file.Corrupt("pawn group");
                if (e.PawnCount[1] != 0)
                    for (int i = e.PawnCount[0]; i < e.PawnCount[0] + e.PawnCount[1]; ++i)
                        if (d.Pieces[i] != (lead ^ 8)) throw file.Corrupt("remaining pawns");
            }
        }

        internal static void SetGroups(TBTable e, PairsData d, int[] order, int f, TBFile file, SyzygyIndex index)
        {
            int n = 0, firstLen = e.HasPawns ? 0 : e.HasUniquePieces ? 3 : 2;
            d.GroupLen[0] = 1;
            for (int i = 1; i < e.PieceCount; ++i)
                if (--firstLen > 0 || d.Pieces[i] == d.Pieces[i - 1]) ++d.GroupLen[n];
                else d.GroupLen[++n] = 1;
            d.GroupLen[++n] = 0;
            bool pp = e.HasPawns && e.PawnCount[1] != 0;
            if (order[0] >= n || (pp && (order[1] >= n || order[0] == order[1])) || (!pp && order[1] != 15)) throw file.Corrupt("group order");
            if (!e.HasPawns && !e.HasUniquePieces && (PieceType.TypeOf(d.Pieces[0]) != PieceType.KING || PieceType.TypeOf(d.Pieces[1]) != PieceType.KING)) throw file.Corrupt("king group");
            int next = pp ? 2 : 1, free = 64 - d.GroupLen[0] - (pp ? d.GroupLen[1] : 0);
            ulong idx = 1;
            for (int k = 0; next < n || k == order[0] || k == order[1]; ++k)
                if (k == order[0])
                {
                    d.GroupIdx[0] = idx;
                    idx = checked(idx * (ulong)(e.HasPawns ? index.LeadPawnsSize[d.GroupLen[0], f] : e.HasUniquePieces ? 31332 : 462));
                }
                else if (k == order[1])
                {
                    d.GroupIdx[1] = idx; idx = checked(idx * (ulong)index.Binomial[d.GroupLen[1], 48 - d.GroupLen[0]]);
                }
                else
                {
                    if (next >= n) throw file.Corrupt("group order gap");
                    d.GroupIdx[next] = idx; idx = checked(idx * (ulong)index.Binomial[d.GroupLen[next], free]);
                    free -= d.GroupLen[next++];
                }
            d.GroupIdx[n] = idx;
        }

        internal static long SetSizes(PairsData d, long data, TBFile file)
        {
            d.Flags = file.U8(data++);
            if ((d.Flags & TBFlag.SingleValue) != 0) { d.MinSymLen = file.U8(data++); return data; }
            ulong tbSize = d.GroupIdx[Array.IndexOf(d.GroupLen, 0)];
            file.Require(data, 9);
            int blockBits = file.U8(data++), spanBits = file.U8(data++);
            if (blockBits >= 64 || spanBits >= 64) throw file.Corrupt("block/span shift");
            d.SizeofBlock = 1UL << blockBits; d.Span = 1UL << spanBits;
            d.SparseIndexSize = checked(tbSize + d.Span - 1) / d.Span;
            byte padding = file.U8(data++); d.BlocksNum = file.U32(data); data += 4;
            d.BlockLengthSize = checked(d.BlocksNum + padding);
            d.MaxSymLen = file.U8(data++); d.MinSymLen = file.U8(data++);
            if (d.MinSymLen == 0 || d.MinSymLen > d.MaxSymLen || d.MaxSymLen >= 64 || d.BlocksNum == 0) throw file.Corrupt("Huffman lengths/blocks");
            d.LowestSym = data; d.Base64 = new ulong[d.MaxSymLen - d.MinSymLen + 1];
            file.Require(data, (ulong)d.Base64.Length, 2);
            for (int i = d.Base64.Length - 2; i >= 0; --i)
            {
                d.Base64[i] = unchecked(d.Base64[i + 1] + file.U16(data + i * 2) - file.U16(data + (i + 1) * 2)) / 2;
                if (unchecked(d.Base64[i] * 2) < d.Base64[i + 1]) throw file.Corrupt("canonical Huffman base");
            }
            for (int i = 0; i < d.Base64.Length; ++i) d.Base64[i] <<= 64 - i - d.MinSymLen;
            data += d.Base64.Length * 2;
            int size = file.U16(data); data += 2;
            if (size > SymCount) throw file.Corrupt("symbol count");
            file.Require(data, (ulong)size, 3);
            d.Btree = new LR[SymCount]; d.Symlen = new byte[SymCount];
            for (int i = 0; i < SymCount; ++i) d.Btree[i] = i < size ? LR.Read(file, data + i * 3) : new LR(0, 0xF0, 0xFF);
            SetSymlen(d, size, file);
            return data + size * 3 + (size & 1);
        }

        // Iterative DFS preserves u8 length arithmetic and grey-cycle detection without a 4096-frame call stack.
        internal static void SetSymlen(PairsData d, int size, TBFile file)
        {
            var color = new SymColour[SymCount]; var stack = new int[SymCount];
            for (int root = 0; root < size; ++root)
            {
                if (color[root] != SymColour.White) continue;
                int top = 0; stack[0] = root;
                while (top >= 0)
                {
                    int sym = stack[top]; color[sym] = SymColour.Grey;
                    int right = d.Btree[sym].Get(LR.Side.Right), left = d.Btree[sym].Get(LR.Side.Left);
                    if (right == 0xFFF) { color[sym] = SymColour.Black; --top; continue; }
                    if (color[left] == SymColour.Grey || color[right] == SymColour.Grey) throw file.Corrupt("cyclic pairing tree");
                    if (color[left] == SymColour.White) { stack[++top] = left; continue; }
                    if (color[right] == SymColour.White) { stack[++top] = right; continue; }
                    d.Symlen[sym] = unchecked((byte)(d.Symlen[left] + d.Symlen[right] + 1));
                    color[sym] = SymColour.Black; --top;
                }
            }
        }

        internal static int DecompressPairs(PairsData d, ulong idx, TBFile file)
        {
            if ((d.Flags & TBFlag.SingleValue) != 0) return d.MinSymLen;
            ulong k = idx / d.Span;
            if (k >= d.SparseIndexSize) throw file.Corrupt("position index");
            SparseEntry sparse = SparseEntry.Read(file, checked(d.SparseIndex + (long)k * 6));
            uint block = sparse.Block;
            int diff = unchecked((int)(idx % d.Span - d.Span / 2));
            long offset = (long)sparse.Offset + diff;
            if (block >= d.BlockLengthSize) block = d.BlockLengthSize - 1;
            int Length(uint b) => file.U16(d.BlockLength + (long)b * 2);
            while (offset < 0 && block > 0) offset += Length(--block) + 1;
            while (offset > Length(block) && block + 1 < d.BlockLengthSize) offset -= Length(block++) + 1;
            if (offset < 0 || offset > Length(block) || block >= d.BlocksNum) throw file.Corrupt("sparse block");
            long ptr = checked(d.Data + (long)(block * d.SizeofBlock));
            ulong buf64 = TBFile.Fits(ptr, 8, 1, d.DataEnd) ? file.U64BE(ptr) : 0; ptr += 8;
            int bufSize = 64, sym = 0, iterations = 0;
            while (true)
            {
                if (++iterations > 65536) throw file.Corrupt("Huffman stream");
                int len = 0;
                while (len < d.Base64.Length && buf64 < d.Base64[len]) ++len;
                if (len == d.Base64.Length) throw file.Corrupt("Huffman code");
                sym = (int)((unchecked((ushort)((buf64 - d.Base64[len]) >> (64 - len - d.MinSymLen)))
                    + file.U16(d.LowestSym + len * 2)) & (SymCount - 1));
                if (offset < d.Symlen[sym] + 1) break;
                offset -= d.Symlen[sym] + 1; len += d.MinSymLen;
                buf64 <<= len; bufSize -= len;
                if (bufSize <= 32)
                {
                    bufSize += 32;
                    if (TBFile.Fits(ptr, 4, 1, d.DataEnd)) buf64 |= (ulong)file.U32BE(ptr) << (64 - bufSize);
                    ptr += 4;
                }
            }
            while (d.Symlen[sym] != 0)
            {
                int left = d.Btree[sym].Get(LR.Side.Left);
                if (offset < d.Symlen[left] + 1) sym = left;
                else { offset -= d.Symlen[left] + 1; sym = d.Btree[sym].Get(LR.Side.Right); }
            }
            return d.Btree[sym].Get(LR.Side.Left);
        }
    }
}
