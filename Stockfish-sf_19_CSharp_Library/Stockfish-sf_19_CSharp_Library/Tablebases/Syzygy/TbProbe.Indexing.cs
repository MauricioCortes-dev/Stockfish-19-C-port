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
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    public sealed partial class TbProbe
    {
        private int DoProbeTable(Position pos, TBTable entry, TBTable.Loaded loaded, WDLScore wdl, ref ProbeState result)
        {
            Span<Square> squares = stackalloc Square[MaxSupportedPieces];
            Span<Piece> pieces = stackalloc Piece[MaxSupportedPieces];
            squares.Clear(); pieces.Clear();
            ulong idx;
            int next = 0, size = 0, leadPawnsCnt = 0, tbFile = 0;
            Bitboard leadPawns = 0, b;
            bool symmetricBlack = entry.Key == entry.Key2 && pos.SideToMove() == Color.BLACK;
            bool blackStronger = pos.MaterialKey() != entry.Key;
            bool flip = symmetricBlack || blackStronger;
            int flipColor = flip ? 8 : 0, flipSquares = flip ? 56 : 0;
            int stm = (flip ? 1 : 0) ^ pos.SideToMove();
            if (entry.HasPawns)
            {
                Piece pc = loaded.Items[0, 0].Pieces[0] ^ flipColor;
                leadPawns = b = pos.Pieces(Color.ColorOf(pc), PieceType.PAWN);
                do { squares[size++] = SyzygyGeometry.Xor(Bitboards.PopLsb(ref b), flipSquares); } while (b != 0);
                leadPawnsCnt = size;
                int max = 0;
                for (int i = 1; i < size; ++i) if (index.PawnsComp(squares[max], squares[i])) max = i;
                Swap(squares, 0, max);
                tbFile = File.EdgeDistance(File.FileOf(squares[0]));
            }
            if (entry.Type == TBType.DTZ && !CheckDtzStm(entry, loaded.Items[0, tbFile], stm))
            { result = ProbeState.CHANGE_STM; return 0; }
            b = pos.Pieces() ^ leadPawns;
            do
            {
                Square s = Bitboards.PopLsb(ref b);
                squares[size] = SyzygyGeometry.Xor(s, flipSquares); pieces[size++] = pos.PieceOn(s) ^ flipColor;
            } while (b != 0);
            PairsData d = loaded.Items[entry.Type == TBType.DTZ || entry.Key == entry.Key2 ? 0 : stm, tbFile];
            for (int i = leadPawnsCnt; i < size - 1; ++i)
                for (int j = i + 1; j < size; ++j)
                    if (d.Pieces[i] == pieces[j]) { Swap(pieces, i, j); Swap(squares, i, j); break; }
            if (File.FileOf(squares[0]) > File.FILE_D)
                for (int i = 0; i < size; ++i) squares[i] = Square.FlipFile(squares[i]);
            if (entry.HasPawns)
            {
                idx = (ulong)index.LeadPawnIdx[leadPawnsCnt, squares[0]];
                SortSquares(squares, 1, leadPawnsCnt - 1, true);
                for (int i = 1; i < leadPawnsCnt; ++i) idx += (ulong)index.Binomial[i, index.MapPawns[squares[i]]];
            }
            else
            {
                if (Rank.RankOf(squares[0]) > Rank.RANK_4)
                    for (int i = 0; i < size; ++i) squares[i] = Square.FlipRank(squares[i]);
                for (int i = 0; i < d.GroupLen[0]; ++i)
                {
                    if (SyzygyGeometry.OffA1H8(squares[i]) == 0) continue;
                    if (SyzygyGeometry.OffA1H8(squares[i]) > 0)
                        for (int j = i; j < size; ++j) squares[j] = ((squares[j] >> 3) | (squares[j] << 3)) & 63;
                    break;
                }
                if (entry.HasUniquePieces)
                {
                    int adjust1 = squares[1] > squares[0] ? 1 : 0;
                    int adjust2 = (squares[2] > squares[0] ? 1 : 0) + (squares[2] > squares[1] ? 1 : 0);
                    if (SyzygyGeometry.OffA1H8(squares[0]) != 0)
                        idx = (ulong)((index.MapA1D1D4[squares[0]] * 63 + squares[1] - adjust1) * 62 + squares[2] - adjust2);
                    else if (SyzygyGeometry.OffA1H8(squares[1]) != 0)
                        idx = (ulong)((6 * 63 + Rank.RankOf(squares[0]) * 28 + index.MapB1H1H7[squares[1]]) * 62 + squares[2] - adjust2);
                    else if (SyzygyGeometry.OffA1H8(squares[2]) != 0)
                        idx = (ulong)(6 * 63 * 62 + 4 * 28 * 62 + Rank.RankOf(squares[0]) * 7 * 28
                            + (Rank.RankOf(squares[1]) - adjust1) * 28 + index.MapB1H1H7[squares[2]]);
                    else idx = (ulong)(6 * 63 * 62 + 4 * 28 * 62 + 4 * 7 * 28 + Rank.RankOf(squares[0]) * 7 * 6
                        + (Rank.RankOf(squares[1]) - adjust1) * 6 + Rank.RankOf(squares[2]) - adjust2);
                }
                else idx = (ulong)index.MapKK[index.MapA1D1D4[squares[0]], squares[1]];
            }
            idx *= d.GroupIdx[0]; int groupSq = d.GroupLen[0];
            bool remainingPawns = entry.HasPawns && entry.PawnCount[1] != 0;
            while (d.GroupLen[++next] != 0)
            {
                SortSquares(squares, groupSq, d.GroupLen[next], false); ulong n = 0;
                for (int i = 0; i < d.GroupLen[next]; ++i)
                {
                    int adjust = 0;
                    for (int j = 0; j < groupSq; ++j) if (squares[groupSq + i] > squares[j]) ++adjust;
                    n += (ulong)index.Binomial[i + 1, squares[groupSq + i] - adjust - (remainingPawns ? 8 : 0)];
                }
                remainingPawns = false; idx += n * d.GroupIdx[next]; groupSq += d.GroupLen[next];
            }
            int value = SyzygyDecoder.DecompressPairs(d, idx, loaded.File!);
            if (entry.Type == TBType.DTZ) return MapDtzScore(d, loaded, value, wdl);
            if (value > 4) throw loaded.File!.Corrupt("WDL value");
            return value - 2; // map_score<WDL>
        }
        private static void Swap<T>(Span<T> array, int i, int j) { T x = array[i]; array[i] = array[j]; array[j] = x; }
        private void SortSquares(Span<Square> squares, int start, int count, bool pawns)
        {
            // Stable insertion sort on at most five squares.
            for (int i = start + 1; i < start + count; ++i)
            {
                Square value = squares[i]; int j = i;
                while (j > start && (pawns ? index.PawnsComp(value, squares[j - 1]) : value < squares[j - 1]))
                { squares[j] = squares[j - 1]; --j; }
                squares[j] = value;
            }
        }
    }
}
