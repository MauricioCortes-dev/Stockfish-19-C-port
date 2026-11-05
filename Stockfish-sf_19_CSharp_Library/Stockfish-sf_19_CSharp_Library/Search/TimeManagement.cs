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
using Stockfish_sf_19_CSharp_Library.Configuration;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed class TimeManagement
    {
        public static readonly TimePoint NoBound = long.MaxValue / 2;
        private TimePoint startTime, optimumTime = NoBound, maximumTime = NoBound;
        private long availableNodes = -1;
        private bool useNodesTime;
        public IMonotonicClock Clock { get; }
        internal bool UsesNodesTime => useNodesTime;

        public TimeManagement(IMonotonicClock? clock = null)
        {
            Clock = clock ?? MonotonicClock.Instance;
        }

        public TimePoint Optimum() => optimumTime;
        public TimePoint Maximum() => maximumTime;
        public TimePoint Elapsed(Func<ulong> nodes) => useNodesTime ? unchecked((long)nodes()) : (long)ElapsedTime();
        public TimePoint ElapsedTime() => Clock.Now - startTime;
        public void Clear()
        {
            availableNodes = -1;
        }

        public void AdvanceNodesTime(long nodes)
        {
            if (!useNodesTime)
                throw new InvalidOperationException("Nodes time is disabled.");
            availableNodes = Math.Max(0, checked(availableNodes - nodes));
        }

        public void Init(LimitsType limits, Color us, int ply, OptionsMap options, ref double originalTimeAdjust)
        {
            TimePoint npmsec = options["nodestime"].AsInt();
            startTime = limits.StartTime;
            useNodesTime = npmsec != 0;
            if (useNodesTime)
                limits.MoveTime = checked((long)limits.MoveTime * npmsec);
            if (limits.Time[us] == 0)
            {
                optimumTime = maximumTime = NoBound;
                return;
            }

            TimePoint overhead = options["Move Overhead"].AsInt();
            double optScale, maxScale;
            if (useNodesTime)
            {
                if (availableNodes == -1)
                    availableNodes = checked(npmsec * (long)limits.Time[us]);
                limits.Time[us] = availableNodes;
                limits.Increment[us] = checked((long)limits.Increment[us] * npmsec);
                limits.Npmsec = npmsec;
                overhead = checked(overhead * npmsec);
            }

            long scale = useNodesTime ? (long)npmsec : 1;
            TimePoint scaledTime = Math.Max(1, (long)limits.Time[us] / scale);
            int mtg = limits.MovesToGo != 0 ? Math.Min(limits.MovesToGo, 50) : 50;
            if (scaledTime < 1000 && limits.MovesToGo == 0)
                mtg = (int)(scaledTime * 0.05);
            TimePoint left = Math.Max(1, checked((long)limits.Time[us] + (long)limits.Increment[us] * (mtg - 1) - overhead * (2 + mtg)));
            if (limits.MovesToGo == 0)
            {
                if (originalTimeAdjust < 0)
                    originalTimeAdjust = 0.3272 * Math.Log10(left) - 0.4141;
                double log = Math.Log10(scaledTime / 1000.0);
                double opt = Math.Min(0.0029869 + 0.00033554 * log, 0.004905), max = Math.Max(3.3744 + 3.0608 * log, 3.1441);
                optScale = Math.Min(0.012112 + Math.Pow(ply + 3.22713, 0.46866) * opt, 0.19404 * (long)limits.Time[us] / left) * originalTimeAdjust;
                maxScale = Math.Min(6.873, max + ply / 12.352);
            }
            else
            {
                optScale = Math.Min((0.88 + ply / 116.4) / mtg, 0.88 * (long)limits.Time[us] / left);
                maxScale = 1.3 + 0.11 * mtg;
            }

            optimumTime = checked((long)Math.Max(1.0, optScale * left));
            maximumTime = checked((long)Math.Max((long)optimumTime, Math.Min(0.8097 * (long)limits.Time[us] - overhead, maxScale * (long)optimumTime)));
            if (options["Ponder"].AsInt() != 0)
                optimumTime += (long)optimumTime / 4;
        }
    }
}
