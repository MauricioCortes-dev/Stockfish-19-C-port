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
using System.Linq;
using System.Threading;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Diagnostics;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Main-worker persistent state. Only PonderHit is concurrent; callbacks execute on the searching thread.</summary>
    public sealed class SearchManager : ISearchManager
    {
        public TimeManagement Time { get; }

        public double OriginalTimeAdjust = -1, PreviousTimeReduction = 0.85;
        public int CallsCount;
        public Value[] IterValue { get; } = new Value[4];

        public Value BestPreviousScore = Value.VALUE_INFINITE, BestPreviousAverageScore = Value.VALUE_INFINITE;
        public bool StopOnPonderHit;
        public DebugStatistics Diagnostics { get; } = new DebugStatistics();
        public Action<DebugStatistics>? OnDiagnostics { get; set; }
        private long? lastInfoTime;
        private int ponder;
        public bool Ponder => Volatile.Read(ref ponder) != 0;

        private SearchControl? control;
        private readonly UpdateContextReference updates;
        public ref readonly UpdateContext Updates => ref updates.Value;
        public Func<uint>? RandomSource { get; }

        /// <summary>Own a value copy when no external callback owner is needed.</summary>
        public SearchManager(UpdateContext updates = default, IMonotonicClock? clock = null, Func<uint>? randomSource = null) : this(new UpdateContextReference(updates), clock, randomSource)
        {
        }

        /// <summary>Keep a live reference to an engine-owned callback context, as in the original.</summary>
        public SearchManager(UpdateContextReference updates, IMonotonicClock? clock = null, Func<uint>? randomSource = null)
        {
            this.updates = updates ?? throw new ArgumentNullException(nameof(updates));
            Time = new TimeManagement(clock);
            RandomSource = randomSource;
        }

        internal void Begin(SearchControl signal, bool pondering)
        {
            Volatile.Write(ref control, signal);
            StopOnPonderHit = false;
            Volatile.Write(ref ponder, pondering ? 1 : 0);
        }

        public void PonderHit() => SetPonderHit(false);

        public void SetPonderHit(bool pondering)
        {
            Volatile.Write(ref ponder, pondering ? 1 : 0);
            Volatile.Read(ref control)?.Wake();
        }

        internal void DetachControl() => Volatile.Write(ref control, null);

        public void Clear()
        {
            DetachControl();
            OriginalTimeAdjust = -1;
            PreviousTimeReduction = 0.85;
            CallsCount = 0;
            BestPreviousScore = BestPreviousAverageScore = Value.VALUE_INFINITE;
            Time.Clear();
        }

        internal bool HasWallClockLimit(Worker worker) => !Time.UsesNodesTime &&
            (worker.Limits.UseTimeManagement() || worker.Limits.MoveTime != 0);

        internal void CheckWallTime(Worker worker)
        {
            if (Ponder || !HasWallClockLimit(worker)) return;
            if (ClockLimitReached(worker, Time.ElapsedTime())) worker.Control.RequestStop();
        }
        private bool ClockLimitReached(Worker worker, TimePoint elapsed) =>
            (worker.Limits.UseTimeManagement() && (elapsed > Time.Maximum() || StopOnPonderHit)) ||
            (worker.Limits.MoveTime != 0 && elapsed >= worker.Limits.MoveTime);

        public void CheckTime(Worker worker)
        {
            // The native 512-call throttle can exceed a complete short clock on
            // managed runtimes. Wall-clock deadlines need a check at each visited
            // search/qsearch node; pure node limits retain the original cadence.
            if (--CallsCount > 0)
            {
                CheckWallTime(worker);
                return;
            }
            CallsCount = worker.Limits.Nodes != 0 ? Math.Min(512, unchecked((int)(worker.Limits.Nodes / 1024))) : 512;
            TimePoint elapsed = Time.Elapsed(() => worker.TotalNodes);
            if (!lastInfoTime.HasValue) lastInfoTime = Time.Clock.Now;
            TimePoint tick = worker.Limits.StartTime + elapsed;
            if (tick - lastInfoTime.Value >= 1000)
            {
                lastInfoTime = tick;
                OnDiagnostics?.Invoke(Diagnostics);
            }
            if (Ponder)
                return;
            if (ClockLimitReached(worker, elapsed) || (worker.Limits.Nodes != 0 && worker.TotalNodes >= worker.Limits.Nodes))
                worker.Control.RequestStop();
        }

        // C++ converts the report's counters to usize; preserve the native 32/64-bit projection.
        private static UIntPtr NativeSize(ulong value) => UIntPtr.Size == 8 ? new UIntPtr(value) : new UIntPtr(unchecked((uint)value));
        public void OutputPv(Worker worker, Depth depth)
        {
            var roots = worker.RootMoves;
            var pos = worker.RootPosition;
            ulong nodes = worker.TotalNodes, tbHits = unchecked(worker.TotalTbHits + (worker.TbConfig.rootInTB ? (ulong)roots.Count : 0));
            int multiPV = Math.Min(worker.Options["MultiPV"].AsInt(), roots.Count);
            for (int i = 0; i < multiPV; ++i)
            {
                ref RootMove rm = ref roots[i];
                bool previous = rm.score == -Value.VALUE_INFINITE;
                if (depth == 1 && previous && i > 0)
                    continue;
                Depth d = previous ? Math.Max(1, depth - 1) : (int)depth;
                Value v = previous ? rm.previousScore : rm.uciScore;
                if (v == -Value.VALUE_INFINITE)
                    v = Value.VALUE_ZERO;
                bool isTbScore = worker.TbConfig.rootInTB && !v.IsMateOrMated();
                if (isTbScore) v = rm.tbScore;
                if (v.IsDecisive() && !v.IsMateOrMated() && !previous && (!rm.IsInexact() || isTbScore))
                    worker.Tablebases?.ExtendPv(worker.Options, worker.Limits, pos, ref rm, ref v, multiPV, diagnostic: Updates.OnInfoString);
                string pv = string.Join(" ", (previous ? rm.previousPV : rm.pv).Select(m => UciFormatting.Move(m, pos.IsChess960())));
                TimePoint time = Math.Max(1, (long)Time.ElapsedTime());
                var info = new InfoFull
                {
                    Depth = d,
                    SelDepth = rm.selDepth,
                    MultiPV = new UIntPtr((uint)(i + 1)),
                    Score = new Score(v, pos),
                    Wdl = worker.Options["UCI_ShowWDL"].AsInt() != 0 ? UciFormatting.Wdl(v, pos) : "",
                    Bound = previous || isTbScore ? "" : rm.inexactLower ? "lowerbound" : rm.inexactUpper ? "upperbound" : "",
                    TimeMs = NativeSize((ulong)(long)time),
                    Nodes = NativeSize(nodes),
                    Nps = NativeSize(unchecked(nodes * 1000) / (ulong)(long)time),
                    TbHits = NativeSize(tbHits),
                    Pv = pv,
                    Hashfull = worker.TT.Hashfull()
                };
                Updates.OnUpdateFull?.Invoke(info);
            }
        }
    }
}
