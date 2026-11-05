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
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using static Stockfish_sf_19_CSharp_Library.Board.Bitboards;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features
{
    public static class HalfKAv2_hm
    {
        public const uint HashValue = 0x7f234cb8U, Dimensions = 22528;
        public const int MaxActiveDimensions = 32;
        public struct IndexList
        {
            private ValueList<ushort, ValueStorage32<ushort>> values;
            public int Size() => values.Size();
            public int SSize() => values.SSize();
            public int MakeSpace(int count) => values.MakeSpace(count);
            public void PushBackIfLtAt(uint value, int maxIndex) => values.PushBackIfLtAt(unchecked((ushort)value), maxIndex);
            public ValueList<ushort, ValueStorage32<ushort>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public int Capacity => values.Capacity;

            public ushort this[int i] { get => values[i]; set => values[i] = value; }

            public void PushBack(uint value) => values.PushBack(unchecked((ushort)value));
            public void PushBackIfLt(uint value, uint limit) => values.PushBackIfLt(unchecked((ushort)value), unchecked((ushort)limit));
        }

        public static IndexType MakeIndex(Color perspective, Square s, Piece pc, Square ksq)
        {
            FullThreats.Require(perspective, s, ksq);
            int pt = PieceType.TypeOf(pc);
            if (pt < 1 || pt > 6)
                throw new ArgumentOutOfRangeException(nameof(pc));
            int flip = 56 * perspective;
            int orient = ((int)ksq % 8 < 4) ? 7 : 0;
            int bucket = (7 - (((int)ksq ^ flip) / 8)) * 4 + Math.Min((int)ksq % 8, 7 - (int)ksq % 8);
            int piece = pt == 6 ? 10 * 64 : (2 * (pt - 1) + ((int)Color.ColorOf(pc) ^ (int)perspective)) * 64;
            return (uint)(((int)s ^ orient ^ flip) + piece + bucket * 704);
        }

        public static void AppendChangedIndices(Color perspective, Square ksq, in DirtyPiece diff, ref IndexList removed, ref IndexList added)
        {
            removed.PushBack(MakeIndex(perspective, diff.from, diff.pc, ksq));
            if (diff.to != Square.SQ_NONE)
                added.PushBack(MakeIndex(perspective, diff.to, diff.pc, ksq));
            if (diff.remove_sq != Square.SQ_NONE)
                removed.PushBack(MakeIndex(perspective, diff.remove_sq, diff.remove_pc, ksq));
            if (diff.add_sq != Square.SQ_NONE)
                added.PushBack(MakeIndex(perspective, diff.add_sq, diff.add_pc, ksq));
        }

        /// <summary>Logical compress-store order of the AVX512ICL helper; only live indices are written.</summary>
        public static void WriteIndices(IReadOnlyList<Piece> oldPieces, IReadOnlyList<Piece> newPieces,
            Bitboard removedBB, Bitboard addedBB, Color perspective, Square ksq, ref IndexList removed, ref IndexList added)
        {
            if (oldPieces.Count != 64 || newPieces.Count != 64) throw new ArgumentException("Expected complete piece arrays.");
            while (removedBB != 0)
            {
                Square sq = PopLsb(ref removedBB); removed.PushBack(MakeIndex(perspective, sq, oldPieces[sq], ksq));
            }
            while (addedBB != 0)
            {
                Square sq = PopLsb(ref addedBB); added.PushBack(MakeIndex(perspective, sq, newPieces[sq], ksq));
            }
        }

        public static bool RequiresRefresh(in DirtyPiece diff, Color perspective) => diff.pc == Piece.MakePiece(perspective, PieceType.KING);
    }
}
