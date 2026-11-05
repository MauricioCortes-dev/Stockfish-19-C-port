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
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        public Depth RootDepth { get; set; }
        public Value RootDelta { get; set; } = 1;
        public int PvIndex { get; set; }
        public int PvLast { get; set; }
        public PVMoves LastIterationPV { get; set; }

        private RelaxedAtomic<ulong, AtomicUInt64> bestMoveChanges;
        public ulong BestMoveChanges => bestMoveChanges.Load();

        private int nmpMinPly;
        public SearchControl Control { get; set; } = new SearchControl();

        /// <summary>Context for direct search calls, without iterative deepening or time management.</summary>
        /// <remarks>Preserves the supplied position's complete StateInfo chain. Call once per search;
        /// for TT/history reuse, prepare again with rebuildRootMoves as appropriate. Lifecycle requires exclusive access.</remarks>
        public int PrepareSearch(Position pos, Depth depth, Value rootDelta, int ply = 0, bool rebuildRootMoves = true)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos));
            if (depth < 1 || depth >= ChessConstants.MAX_PLY)
                throw new ArgumentOutOfRangeException(nameof(depth));
            if (rootDelta < 1 || rootDelta > 2 * (int)Value.VALUE_INFINITE)
                throw new ArgumentOutOfRangeException(nameof(rootDelta));
            int frame = PrepareQuiescence(ply);
            RootDepth = depth;
            RootDelta = rootDelta;
            nmpMinPly = 0;
            bestMoveChanges.Store(0);
            tbHits.Store(0);
            LastIterationPV = default;
            PvIndex = 0;
            if (rebuildRootMoves)
            {
                RootMoves.Clear();
                foreach (Move m in new MoveList(pos, GenType.LEGAL))
                    RootMoves.EmplaceBack(m);
                TbConfig = Tablebases?.RankRootMoves(Options, pos, RootMoves) ?? default;
            }

            PvLast = RootMoves.Count;
            short low = 102;
            LowPlyHistory.Value.Fill(in low);
            for (int c = 0; c < 2; ++c)
            {
                var cells = QuietHistoryRow.Cells(ref ButterflyHistory.At(ref MainHistory.Value, c));
                for (int i = 0; i < cells.Length; ++i)
                {
                    ref var cell = ref cells[i];
                    cell.Assign((short)(cell.Load() * 729 / 1024));
                }
            }

            return frame;
        }
    }
}
