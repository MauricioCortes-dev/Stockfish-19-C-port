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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using static Stockfish_sf_19_CSharp_Library.Board.Bitboards;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features
{
    public static class PP_3Wide
    {
        public const uint HashValue = 0x86f2b1ddU, Dimensions = 4560, IndexBase = 59808;
        public const int PawnIds = 96;
        public struct IndexList
        {
            private ValueList<ushort, ValueStorage256<ushort>> values;
            public int Size() => values.Size();
            public int SSize() => values.SSize();
            public int MakeSpace(int count) => values.MakeSpace(count);
            public void PushBackIfLtAt(uint value, int maxIndex) => values.PushBackIfLtAt(unchecked((ushort)value), maxIndex);
            public ValueList<ushort, ValueStorage256<ushort>>.Enumerator GetEnumerator() => values.GetEnumerator();
            public int Capacity => values.Capacity;

            public ushort this[int i] { get => values[i]; set => values[i] = value; }

            public void PushBack(uint value) => values.PushBack(unchecked((ushort)value));
            public void PushBackIfLt(uint value, uint limit) => values.PushBackIfLt(unchecked((ushort)value), unchecked((ushort)limit));
        }

        public static IndexType MakePawnId(Color color, Square square)
        {
            if ((int)color >= 2 || square < Square.SQ_A2 || square > Square.SQ_H7)
                throw new ArgumentOutOfRangeException();
            return (uint)(48 * color + square - Square.SQ_A2);
        }

        /// <summary>Unsigned16 lane arithmetic of pp_idx_epi16, for the complete pawn-id domain.</summary>
        public static ushort PairIndex16(ushort a, ushort b)
        {
            if (a >= PawnIds || b >= PawnIds) throw new ArgumentOutOfRangeException();
            uint hi = Math.Max(a, b), lo = Math.Min(a, b);
            uint index = unchecked(hi * (hi - 1) / 2 + lo + IndexBase);
            return checked((ushort)index);
        }

        public static IndexType MakeIndex(Color perspective, Color color, Square from, Square to, Color pairedColor, Square ksq)
        {
            FullThreats.Require(perspective, from, ksq);
            FullThreats.Require(perspective, to, ksq);
            int orientation = FullThreats.Orientation(ksq) ^ (56 * perspective);
            uint a = MakePawnId((Color)(color ^ perspective), (Square)((int)from ^ orientation));
            uint b = MakePawnId((Color)(pairedColor ^ perspective), (Square)((int)to ^ orientation));
            uint hi = Math.Max(a, b), lo = Math.Min(a, b);
            return unchecked(hi * (hi - 1) / 2 + lo + IndexBase);
        }

        public static void AppendActiveIndices(Color perspective, Position pos, ref IndexList active)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos));
            Square ksq = pos.SquareOf(PieceType.KING, perspective);
            Bitboard white = pos.Pieces(Color.WHITE, PieceType.PAWN), black = pos.Pieces(Color.BLACK, PieceType.PAWN), bb = white;
            while (bb != 0)
            {
                Square from = PopLsb(ref bb);
                Bitboard band = PawnPairBB(from), ww = band & bb, wb = band & black;
                while (ww != 0)
                    active.PushBack(MakeIndex(perspective, Color.WHITE, from, PopLsb(ref ww), Color.WHITE, ksq));
                while (wb != 0)
                    active.PushBack(MakeIndex(perspective, Color.WHITE, from, PopLsb(ref wb), Color.BLACK, ksq));
            }

            bb = black;
            while (bb != 0)
            {
                Square from = PopLsb(ref bb);
                Bitboard pairs = PawnPairBB(from) & bb;
                while (pairs != 0)
                    active.PushBack(MakeIndex(perspective, Color.BLACK, from, PopLsb(ref pairs), Color.BLACK, ksq));
            }
        }

        private static void Generate(Color c, Square ksq, Bitboard updatedW, Bitboard updatedB, Bitboard pawnsW, Bitboard pawnsB, ref IndexList output, bool compress = false)
        {
            Bitboard unchanged = (pawnsW | pawnsB) & ~(updatedW | updatedB), u = updatedW | updatedB;
            while (u != 0)
            {
                Square a = PopLsb(ref u);
                Bitboard mask = PawnPairBB(a) & (unchanged | u);
                Color color = (pawnsB & a) != 0 ? Color.BLACK : Color.WHITE;
                if (compress)
                {
                    int orientation = FullThreats.Orientation(ksq) ^ (56 * c);
                    ushort aId = (ushort)MakePawnId((Color)(color ^ c), (Square)((int)a ^ orientation));
                    // Original compress path visits all partners in square order, irrespective of color.
                    while (mask != 0)
                    {
                        Square to = PopLsb(ref mask);
                        Color partner = (pawnsB & to) != 0 ? Color.BLACK : Color.WHITE;
                        ushort bId = (ushort)MakePawnId((Color)(partner ^ c), (Square)((int)to ^ orientation));
                        output.PushBack(PairIndex16(aId, bId));
                    }
                    continue;
                }
                Bitboard black = pawnsB & mask, white = pawnsW & mask;
                while (black != 0)
                    output.PushBack(MakeIndex(c, color, a, PopLsb(ref black), Color.BLACK, ksq));
                while (white != 0)
                    output.PushBack(MakeIndex(c, color, a, PopLsb(ref white), Color.WHITE, ksq));
            }
        }

        public static void AppendChangedIndices(Color perspective, Square ksq, in DirtyPawnPairs diff, ref IndexList removed, ref IndexList added)
        {
            Bitboard wb = diff.before[0], bb = diff.before[1], wa = diff.after[0], ba = diff.after[1];
            if (wb == wa && bb == ba)
                return;
            Generate(perspective, ksq, wa & ~wb, ba & ~bb, wa, ba, ref added);
            Generate(perspective, ksq, wb & ~wa, bb & ~ba, wb, bb, ref removed);
        }

        public static void AppendChangedIndicesBlocks(Color perspective, Square ksq, in DirtyPawnPairs diff, ref IndexList removed, ref IndexList added)
        {
            Bitboard wb = diff.before[0], bb = diff.before[1], wa = diff.after[0], ba = diff.after[1];
            if (wb == wa && bb == ba) return;
            Generate(perspective, ksq, wa & ~wb, ba & ~bb, wa, ba, ref added, true);
            Generate(perspective, ksq, wb & ~wa, bb & ~ba, wb, bb, ref removed, true);
        }
        public static void AppendChangedIndicesBothBlocks(Square wk, Square bk, in DirtyPawnPairs diff, ref IndexList wr, ref IndexList wa, ref IndexList br, ref IndexList ba)
        {
            AppendChangedIndicesBlocks(Color.WHITE, wk, diff, ref wr, ref wa);
            AppendChangedIndicesBlocks(Color.BLACK, bk, diff, ref br, ref ba);
        }

        public static void AppendChangedIndicesBoth(Square whiteKsq, Square blackKsq, in DirtyPawnPairs diff, ref IndexList wr, ref IndexList wa, ref IndexList br, ref IndexList ba)
        {
            Bitboard wb = diff.before[0], bb = diff.before[1], w = diff.after[0], b = diff.after[1];
            if (wb == w && bb == b)
                return;
            GenerateBoth(whiteKsq, blackKsq, w & ~wb, b & ~bb, w, b, ref wa, ref ba);
            GenerateBoth(whiteKsq, blackKsq, wb & ~w, bb & ~b, wb, bb, ref wr, ref br);
        }

        private static void GenerateBoth(Square wk, Square bk, Bitboard uw, Bitboard ub, Bitboard pw, Bitboard pb, ref IndexList white, ref IndexList black)
        {
            Bitboard unchanged = (pw | pb) & ~(uw | ub), u = uw | ub;
            while (u != 0)
            {
                Square a = PopLsb(ref u);
                Bitboard mask = PawnPairBB(a) & (unchanged | u);
                Color color = (pb & a) != 0 ? Color.BLACK : Color.WHITE;
                Bitboard partners = pb & mask;
                while (partners != 0)
                {
                    Square to = PopLsb(ref partners);
                    white.PushBack(MakeIndex(Color.WHITE, color, a, to, Color.BLACK, wk));
                    black.PushBack(MakeIndex(Color.BLACK, color, a, to, Color.BLACK, bk));
                }

                partners = pw & mask;
                while (partners != 0)
                {
                    Square to = PopLsb(ref partners);
                    white.PushBack(MakeIndex(Color.WHITE, color, a, to, Color.WHITE, wk));
                    black.PushBack(MakeIndex(Color.BLACK, color, a, to, Color.WHITE, bk));
                }
            }
        }
    }
}
