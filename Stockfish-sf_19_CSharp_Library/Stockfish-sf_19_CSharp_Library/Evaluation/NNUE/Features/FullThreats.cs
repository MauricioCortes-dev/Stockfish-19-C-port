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
    public static class FullThreats
    {
        public const uint HashValue = 0x2e6b9d04U, Dimensions = 59808;
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

        private static readonly int[] allPieces =
        {
            1,
            2,
            3,
            4,
            5,
            6,
            9,
            10,
            11,
            12,
            13,
            14
        };
        private static readonly int[] numValidTargets =
        {
            0,
            4,
            10,
            8,
            8,
            10,
            0,
            0,
            0,
            4,
            10,
            8,
            8,
            10,
            0,
            0
        };
        private static readonly int[, ] map =
        {
            {
                -1,
                0,
                -1,
                1,
                -1,
                -1
            },
            {
                0,
                1,
                2,
                3,
                4,
                -1
            },
            {
                0,
                1,
                2,
                3,
                -1,
                -1
            },
            {
                0,
                1,
                2,
                3,
                -1,
                -1
            },
            {
                0,
                1,
                2,
                3,
                4,
                -1
            },
            {
                -1,
                -1,
                -1,
                -1,
                -1,
                -1
            }
        };
        private static readonly HelperOffsets[] helpers = new HelperOffsets[16];
        private static readonly uint[, ] offsets = new uint[16, 64];
        private static readonly byte[,, ] indexLut2 = new byte[16, 64, 64];
        private static readonly uint[,, ] indexLut1 = new uint[16, 16, 2];
        static FullThreats()
        {
            int cumulative = 0;
            foreach (int pc in allPieces)
            {
                int pt = pc & 7, size = 0;
                for (int from = 0; from < 64; ++from)
                {
                    offsets[pc, from] = (uint)size;
                    Bitboard attacks = pt == 1 ? Attacks.AttacksBB(PieceType.PAWN, from, (Color)(pc >> 3)) : Attacks.AttacksBB((PieceType)pt, from);
                    if (pt != 1 || (from >= 8 && from <= 55))
                        size += Popcount(attacks);
                    for (int to = 0; to < 64; ++to)
                        indexLut2[pc, from, to] = (byte)Popcount((((1UL << to) - 1) & (ulong)attacks));
                }

                helpers[pc] = new HelperOffsets
                {
                    cumulativePieceOffset = size,
                    cumulativeOffset = cumulative
                };
                cumulative += numValidTargets[pc] * size;
            }

            foreach (int attacker in allPieces)
                foreach (int attacked in allPieces)
                {
                    int a = (attacker & 7), b = (attacked & 7), m = map[a - 1, b - 1];
                    bool semiExcluded = a == b && ((attacker ^ attacked) == 8 || a != 1);
                    uint feature = unchecked((uint)(helpers[attacker].cumulativeOffset + ((attacked >> 3) * (numValidTargets[attacker] / 2) + m) * helpers[attacker].cumulativePieceOffset));
                    indexLut1[attacker, attacked, 0] = m < 0 ? Dimensions : feature;
                    indexLut1[attacker, attacked, 1] = m < 0 || semiExcluded ? Dimensions : feature;
                }
        }

        internal static void Require(Color c, Square from, Square to)
        {
            if ((int)c >= 2 || (int)from >= 64 || (int)to >= 64)
                throw new ArgumentOutOfRangeException();
        }

        internal static int Orientation(Square ksq) => (int)ksq % 8 < 4 ? 0 : 7;
        public static IndexType MakeIndex(Color perspective, Piece attacker, Square from, Square to, Piece attacked, Square ksq)
        {
            Require(perspective, from, to);
            Require(perspective, ksq, ksq);
            int orientation = Orientation(ksq) ^ (56 * perspective);
            int f = (int)from ^ orientation, t = (int)to ^ orientation;
            int a = (int)attacker ^ (8 * perspective), b = (int)attacked ^ (8 * perspective);
            if ((a & 7) < 1 || (a & 7) > 6 || a >= 15 || (b & 7) < 1 || (b & 7) > 6 || b >= 15)
                throw new ArgumentOutOfRangeException();
            return indexLut1[a, b, f < t ? 1 : 0] + offsets[a, f] + indexLut2[a, f, t];
        }

        public static void AppendActiveIndices(Color perspective, Position pos, ref IndexList active)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos));
            Square ksq = pos.SquareOf(PieceType.KING, perspective);
            Bitboard occupied = pos.Pieces(), pawnTargets = pos.Pieces(PieceType.KNIGHT, PieceType.ROOK);
            Bitboard minorTargets = pos.Pieces(PieceType.PAWN) | pos.Pieces(PieceType.KNIGHT) | pos.Pieces(PieceType.BISHOP) | pos.Pieces(PieceType.ROOK);
            Bitboard queenTargets = minorTargets | pos.Pieces(PieceType.QUEEN);
            for (int color = 0; color < 2; ++color)
                for (int direction = 0; direction < 2; ++direction)
                {
                    int dir = (direction == 0 ? 9 : 7) * (color == 0 ? 1 : -1);
                    Bitboard attacks = Shift(pos.Pieces((Color)color, PieceType.PAWN), (Direction)dir) & pawnTargets;
                    while (attacks != 0)
                    {
                        Square to = PopLsb(ref attacks);
                        active.PushBackIfLt(MakeIndex(perspective, Piece.MakePiece((Color)color, PieceType.PAWN), to - (Direction)dir, to, pos.PieceOn(to), ksq), Dimensions);
                    }
                }

            for (int color = 0; color < 2; ++color)
                for (int pt = 2; pt < 6; ++pt)
                {
                    Piece attacker = Piece.MakePiece((Color)color, (PieceType)pt);
                    Bitboard bb = pos.Pieces((Color)color, (PieceType)pt);
                    Bitboard targets = pt == 2 || pt == 5 ? queenTargets : minorTargets;
                    while (bb != 0)
                    {
                        Square from = PopLsb(ref bb);
                        Bitboard attacks = Attacks.AttacksBB((PieceType)pt, from, occupied) & targets;
                        while (attacks != 0)
                        {
                            Square to = PopLsb(ref attacks);
                            active.PushBackIfLt(MakeIndex(perspective, attacker, from, to, pos.PieceOn(to), ksq), Dimensions);
                        }
                    }
                }
        }

        public static void AppendChangedIndices(Color perspective, Square ksq, in DirtyThreats diff, ref IndexList removed, ref IndexList added)
        {
            for (int i = 0; i < diff.list.Size(); ++i)
            {
                DirtyThreat dirty = diff.list[i];
                uint index = MakeIndex(perspective, dirty.Pc(), dirty.PcSq(), dirty.ThreatenedSq(), dirty.ThreatenedPc(), ksq);
                if (dirty.Add())
                    added.PushBackIfLt(index, Dimensions);
                else
                    removed.PushBackIfLt(index, Dimensions);
            }
        }

        public static void AppendChangedIndicesBoth(Square whiteKsq, Square blackKsq, in DirtyThreats diff, ref IndexList whiteRemoved, ref IndexList whiteAdded, ref IndexList blackRemoved, ref IndexList blackAdded)
        {
            for (int i = 0; i < diff.list.Size(); ++i)
            {
                DirtyThreat dirty = diff.list[i];
                uint w = MakeIndex(Color.WHITE, dirty.Pc(), dirty.PcSq(), dirty.ThreatenedSq(), dirty.ThreatenedPc(), whiteKsq);
                uint b = MakeIndex(Color.BLACK, dirty.Pc(), dirty.PcSq(), dirty.ThreatenedSq(), dirty.ThreatenedPc(), blackKsq);
                if (dirty.Add())
                {
                    whiteAdded.PushBackIfLt(w, Dimensions);
                    blackAdded.PushBackIfLt(b, Dimensions);
                }
                else
                {
                    whiteRemoved.PushBackIfLt(w, Dimensions);
                    blackRemoved.PushBackIfLt(b, Dimensions);
                }
            }
        }
    }
}
