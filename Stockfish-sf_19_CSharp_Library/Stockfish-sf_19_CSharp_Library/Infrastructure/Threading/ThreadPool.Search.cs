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
using System.Threading.Tasks;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Threading
{
    public sealed partial class ThreadPool
    {
        private Task<SearchResult>?[] secondarySearches = Array.Empty<Task<SearchResult>?>();
        public bool IsWorkerThread
        {
            get
            {
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                foreach (Thread thread in System.Threading.Volatile.Read(ref threads))
                    if (thread.ManagedThreadId == id)
                        return true;
                return false;
            }
        }

        /// <summary>Prepare all workers, then launch only the main worker. The host owns resource exclusion.</summary>
        /// <remarks>The returned task carries search/callback failures. WaitForAllJobs additionally observes idle
        /// before replacing resources. A fresh control preserves even a stop requested before the launch.</remarks>
        public Task<SearchResult> StartThinking(Position position, LimitsType limits, SearchControl? signal = null)
        {
            if (limits == null) throw new ArgumentNullException(nameof(limits));
            if (limits.Depth < 0 || limits.Mate < 0 || limits.MovesToGo < 0 || limits.MoveTime < 0
                || limits.Time[0] < 0 || limits.Time[1] < 0
                || limits.Increment[0] < 0 || limits.Increment[1] < 0)
                throw new ArgumentOutOfRangeException(nameof(limits));
            EnsureHost();
            lock (gate)
            {
                PrepareRoot(position, limits);
                secondarySearches = Array.Empty<Task<SearchResult>?>();
                signal = signal ?? new SearchControl();
                signal.IncreaseDepth = true;
                foreach (Thread thread in threads)
                    thread.Worker.BindSearchControl(signal);
                return threads[0].StartSearching();
            }
        }

        // Called by the main worker; never acquire the host monitor while the host might be joining it.
        public void StartSearching()
        {
            EnsureNotSecondary();
            secondarySearches = new Task<SearchResult>?[Math.Max(0, threads.Length - 1)];
            for (int i = 1; i < threads.Length; ++i)
                secondarySearches[i - 1] = threads[i].StartSearching();
        }

        /// <summary>Join secondary workers only; safe from the main worker. Drain all before surfacing failures.</summary>
        public void WaitForSearchFinished()
        {
            EnsureNotSecondary();
            List<Exception>? errors = null;
            for (int i = 1; i < threads.Length; ++i)
            {
                try
                {
                    threads[i].WaitForSearchFinished();
                    if (secondarySearches.Length >= i)
                        secondarySearches[i - 1]?.GetAwaiter().GetResult();
                }
                catch (Exception error)
                {
                    (errors ??= new List<Exception>()).Add(error);
                }
            }
            if (errors != null)
                throw new AggregateException(errors);
        }

        private void EnsureNotSecondary()
        {
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            for (int i = 1; i < threads.Length; ++i)
                if (threads[i].ManagedThreadId == id)
                    throw new InvalidOperationException("A secondary worker cannot coordinate the pool.");
        }

        public ulong TbHits()
        {
            ulong hits = 0;
            foreach (Thread thread in System.Threading.Volatile.Read(ref threads))
                hits = unchecked(hits + thread.Worker.TbHits);
            return hits;
        }

        internal double TakeBestMoveChanges()
        {
            double changes = 0;
            foreach (Thread thread in threads)
                changes += thread.Worker.TakeBestMoveChanges();
            return changes;
        }

        /// <summary>Native vote/decisive-score selection. Requires nonempty roots and all searches stopped.</summary>
        public Thread GetBestThread()
        {
            if (threads.Length == 0) throw new InvalidOperationException("Pool is empty.");
            Thread bestThread = threads[0];
            Value minScore = Value.VALUE_INFINITE;
            var votes = new Dictionary<Move, long>();
            foreach (Thread thread in threads)
            {
                if (thread.Worker.RootMoves.Empty()) throw new InvalidOperationException("Root moves are empty.");
                minScore = Math.Min(minScore, thread.Worker.RootMoves[0].score);
            }
            foreach (Thread thread in threads)
            {
                ref RootMove move = ref thread.Worker.RootMoves[0];
                votes.TryGetValue(move.pv[0], out long vote);
                votes[move.pv[0]] = unchecked(vote + (int)move.score - (int)minScore + 14);
            }
            foreach (Thread thread in threads)
            {
                ref RootMove best = ref bestThread.Worker.RootMoves[0];
                ref RootMove next = ref thread.Worker.RootMoves[0];
                bool bestDecisive = best.score != -Value.VALUE_INFINITE && best.score.IsDecisive() && !best.IsInexact();
                bool nextDecisive = next.score != -Value.VALUE_INFINITE && next.score.IsDecisive() && !next.IsInexact();
                if (bestDecisive)
                {
                    if (nextDecisive && Math.Abs((int)next.score) > Math.Abs((int)best.score))
                        bestThread = thread;
                }
                else if (nextDecisive
                         || (!next.score.IsLoss()
                             && (votes[next.pv[0]] > votes[best.pv[0]]
                                 || (votes[next.pv[0]] == votes[best.pv[0]] && next.pv.Count > best.pv.Count))))
                    bestThread = thread;
            }
            return bestThread;
        }
    }
}
