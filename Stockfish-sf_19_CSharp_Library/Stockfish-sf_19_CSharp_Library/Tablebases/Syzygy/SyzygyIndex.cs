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

using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Immutable geometry used by Syzygy's seven-piece canonical encoding.</summary>
    internal sealed class SyzygyIndex
    {
        internal readonly int[] MapPawns = new int[64], MapB1H1H7 = new int[64], MapA1D1D4 = new int[64];
        internal readonly int[,] MapKK = new int[10, 64], Binomial = new int[6, 64];
        internal readonly int[,] LeadPawnIdx = new int[6, 64], LeadPawnsSize = new int[6, 4];
        internal SyzygyIndex()
        {
            int code = 0;
            for (Square s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                if (SyzygyGeometry.OffA1H8(s) < 0) MapB1H1H7[s] = code++;
            var diagonal = new List<Square>(); code = 0;
            for (Square s = Square.SQ_A1; s <= Square.SQ_D4; ++s)
                if (SyzygyGeometry.OffA1H8(s) < 0 && File.FileOf(s) <= File.FILE_D) MapA1D1D4[s] = code++;
                else if (SyzygyGeometry.OffA1H8(s) == 0 && File.FileOf(s) <= File.FILE_D) diagonal.Add(s);
            foreach (Square s in diagonal) MapA1D1D4[s] = code++;
            var both = new List<(int, Square)>(); code = 0;
            for (int idx = 0; idx < 10; ++idx)
                for (Square s1 = Square.SQ_A1; s1 <= Square.SQ_D4; ++s1)
                    if (MapA1D1D4[s1] == idx && (idx != 0 || s1 == Square.SQ_B1))
                        for (Square s2 = Square.SQ_A1; s2 <= Square.SQ_H8; ++s2)
                            if (((Attacks.PseudoAttacks(PieceType.KING, s1) | s1) & s2) != 0) continue;
                            else if (SyzygyGeometry.OffA1H8(s1) == 0 && SyzygyGeometry.OffA1H8(s2) > 0) continue;
                            else if (SyzygyGeometry.OffA1H8(s1) == 0 && SyzygyGeometry.OffA1H8(s2) == 0) both.Add((idx, s2));
                            else MapKK[idx, s2] = code++;
            foreach (var pair in both) MapKK[pair.Item1, pair.Item2] = code++;
            Binomial[0, 0] = 1;
            for (int n = 1; n < 64; ++n)
                for (int k = 0; k < 6 && k <= n; ++k)
                    Binomial[k, n] = (k > 0 ? Binomial[k - 1, n - 1] : 0) + (k < n ? Binomial[k, n - 1] : 0);
            int available = 47;
            for (int count = 1; count <= 5; ++count)
                for (File f = File.FILE_A; f <= File.FILE_D; ++f)
                {
                    int idx = 0;
                    for (Rank r = Rank.RANK_2; r <= Rank.RANK_7; ++r)
                    {
                        Square sq = Square.MakeSquare(f, r);
                        if (count == 1) { MapPawns[sq] = available--; MapPawns[Square.FlipFile(sq)] = available--; }
                        LeadPawnIdx[count, sq] = idx; idx += Binomial[count - 1, MapPawns[sq]];
                    }
                    LeadPawnsSize[count, f] = idx;
                }
        }
        internal bool PawnsComp(Square i, Square j) => MapPawns[i] < MapPawns[j];
    }
}
