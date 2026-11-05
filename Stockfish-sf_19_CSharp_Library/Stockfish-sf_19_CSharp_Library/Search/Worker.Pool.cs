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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Numa;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        /// <summary>Borrowed table service; host owns lifetime and excludes resource changes during jobs.</summary>
        public TbProbe? Tablebases { get; set; }
        public Config TbConfig { get; set; }
        private ISearchManager? poolManager;
        internal Infrastructure.Threading.ThreadPool? Pool { get; private set; }
        public ulong TotalNodes => Pool?.NodesSearched() ?? Nodes;
        public ulong TotalTbHits => Pool?.TbHits() ?? TbHits;
        internal ulong TakeBestMoveChanges()
        {
            ulong changes = bestMoveChanges.Load();
            bestMoveChanges.Store(0); // Deliberately separate load/store, like RelaxedAtomic in C++.
            return changes;
        }
        public int ThreadIndex { get; private set; }
        public bool IsMainThread => ThreadIndex == 0;
        public ISearchManager ThreadManager => poolManager ?? Manager;

        public NumaReplicatedAccessToken NumaToken { get; private set; }
        internal Worker(SharedState state, ISearchManager manager, int index, int nodeIndex, int nodeCount, NumaReplicatedAccessToken token)
            : this(state.NetworkForNode(token), state.TT, state.HistoryForNode(token.NumaIndex), nodeIndex, nodeCount)
        {
            NumaToken = token;
            Pool = state.Threads;
            Tablebases = state.Tablebases;
            ThreadIndex = index;
            poolManager = manager;
            Options = state.Options;
            Manager = state.Threads.MainManager;
        }

        /// <summary>The constructor materializes the selected local network before creating refresh caches.</summary>
        public void EnsureNetworkReplicated() { _ = Network; }

        // Preparation only: no time-manager initialization, TT generation change, callbacks or search launch.
        internal void PrepareRoot(Position position, LimitsType limits, RootMoves moves, Config tbConfig = default)
        {
            Limits = new LimitsType(limits);
            RootPosition.Set(position.Fen(), position.IsChess960(), RootState);
            RootState.CopyFrom(position.State()!);
            RootMoves.CopyFrom(moves);
            TbConfig = tbConfig;
            PrepareQuiescence();
            RootDepth = 0;
            nmpMinPly = 0;
            bestMoveChanges.Store(0);
            LastIterationPV = default;
            tbHits.Store(0);
        }
    }
}
