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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Configuration;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        public LimitsType Limits { get; private set; } = new LimitsType();
        public OptionsMap Options { get; private set; } = OptionsMap.CreateSearchDefaults();
        public SearchManager Manager { get; private set; } = new SearchManager();

        public TimePoint Elapsed() => Manager.Time.Elapsed(() => TotalNodes);
        /// <summary>Synchronous one-worker library entry. Caller owns scheduling and exclusive Worker/TT/options access.
        /// Pass a fresh control before launching; stop is never reset here. The input position/history is copied like ThreadPool::start_thinking.</summary>
        public SearchResult StartSearching(Position position, LimitsType limits, OptionsMap options, SearchManager manager, SearchControl signal)
        {
            if (position == null || limits == null || options == null || manager == null || signal == null)
                throw new ArgumentNullException();
            if (limits.Depth < 0 || limits.Mate < 0 || limits.MovesToGo < 0 || limits.MoveTime < 0 || limits.Time[0] < 0 || limits.Time[1] < 0 || limits.Increment[0] < 0 || limits.Increment[1] < 0)
                throw new ArgumentOutOfRangeException(nameof(limits));
            Limits = new LimitsType(limits);
            Options = options;
            Manager = manager;
            Control = signal;
            RootPosition.Set(position.Fen(), position.IsChess960(), RootState);
            RootState.CopyFrom(position.State()!);
            RootMoves.Clear();
            foreach (string text in Limits.SearchMoves)
            {
                Move m = UciParsing.ToMove(RootPosition, text);
                if (m != Move.None())
                    RootMoves.EmplaceBack(m);
            }

            if (RootMoves.Empty())
                foreach (Move m in new MoveList(RootPosition, GenType.LEGAL))
                    RootMoves.EmplaceBack(m);
            TbConfig = Tablebases?.RankRootMoves(Options, RootPosition, RootMoves) ?? default;
            PrepareQuiescence();
            RootDepth = 0;
            nmpMinPly = 0;
            bestMoveChanges.Store(0);
            LastIterationPV = default;
            tbHits.Store(0);
            signal.IncreaseDepth = true;
            BindSearchControl(signal);
            return StartPreparedSearch();
        }

        internal void BindSearchControl(SearchControl signal)
        {
            Control = signal;
            if (!IsMainThread) return;
            Manager.Begin(signal, Limits.PonderMode);
            signal.CheckTime = Manager.CheckTime;
            signal.OnIteration = (depth, move, number) => Manager.Updates.OnIteration?.Invoke(new InfoIteration { Depth = depth, CurrentMove = UciFormatting.Move(move, RootPosition.IsChess960()), CurrentMoveNumber = new UIntPtr((uint)number) });
        }

        internal SearchResult StartPreparedSearch()
        {
            AccumulatorStack.Reset();
            if (!IsMainThread)
            {
                IterativeDeepening();
                return new SearchResult("", "");
            }
            try { return SearchMain(); }
            finally { Manager.DetachControl(); }
        }

        private SearchResult SearchMain()
        {
            SearchManager manager = Manager;
            SearchControl signal = Control;
            bool pvSent;
            try
            {
                manager.Time.Init(Limits, RootPosition.SideToMove(), RootPosition.GamePly(), Options, ref manager.OriginalTimeAdjust);
                TT.NewSearch();
                manager.Updates.OnStart?.Invoke();
                if (RootMoves.Empty())
                {
                    manager.Updates.OnUpdateNoMoves?.Invoke(new InfoShort { Depth = 0, Score = new Score(RootPosition.Checkers() != 0 ? -(int)Value.VALUE_MATE : (int)Value.VALUE_DRAW, RootPosition) });
                    manager.Updates.OnBestmove?.Invoke("(none)", "");
                    return new SearchResult("(none)", "");
                }
                Pool?.StartSearching();
                pvSent = IterativeDeepening();
                // A condition variable replaces the native busy wait, preserving stop/ponderhit semantics.
                signal.WaitWhile(() => manager.Ponder || Limits.Infinite != 0);
            }
            finally
            {
                signal.RequestStop();
                Pool?.WaitForSearchFinished(); // Join secondaries even when a callback or search throws.
            }
            if (Limits.Npmsec != 0)
                manager.Time.AdvanceNodesTime(unchecked((long)(TotalNodes - unchecked((ulong)(long)Limits.Increment[RootPosition.SideToMove()]))));
            Worker bestThread = this;
            var skill = new Skill(Options["Skill Level"].AsInt(), Options["UCI_LimitStrength"].AsInt() != 0 ? Options["UCI_Elo"].AsInt() : 0);
            if (Pool != null && Limits.Depth == 0 && !skill.Enabled())
                bestThread = Pool.GetBestThread().Worker;
            manager.BestPreviousScore = bestThread.RootMoves[0].score;
            manager.BestPreviousAverageScore = bestThread.RootMoves[0].averageScore;
            if (bestThread.RootMoves[0].pv.Count == 1 && bestThread.RootMoves[0].ExtractPonderFromTT(TT, RootPosition))
                pvSent = false;
            if (!pvSent || !ReferenceEquals(bestThread, this))
                manager.OutputPv(bestThread, bestThread.RootDepth);
            string ponder = bestThread.RootMoves[0].pv.Count > 1 ? UciFormatting.Move(bestThread.RootMoves[0].pv[1], RootPosition.IsChess960()) : "";
            string best = UciFormatting.Move(bestThread.RootMoves[0].pv[0], RootPosition.IsChess960());
            manager.Updates.OnBestmove?.Invoke(best, ponder);
            return new SearchResult(best, ponder);
        }
    }
}
