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
using Stockfish_sf_19_CSharp_Library.Configuration;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;
using static Stockfish_sf_19_CSharp_Library.Types.Value;
using static Stockfish_sf_19_CSharp_Library.Types.ChessConstants;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    public sealed partial class TbProbe
    {
        internal const int MaxDtz = 1 << 18;
        public Config RankRootMoves(OptionsMap options, Position pos, RootMoves rootMoves, bool rankDtz = false, Func<bool>? timeAbort = null)
        {
            if (options == null || pos == null || rootMoves == null) throw new ArgumentNullException();
            lifetime.EnterReadLock();
            try { RequireAlive(); return RankRootMovesCore(options, pos, rootMoves, rankDtz, timeAbort ?? NeverAbort); }
            finally { lifetime.ExitReadLock(); }
        }
        private static bool NeverAbort() => false;
        private Config RankRootMovesCore(OptionsMap options, Position pos, RootMoves rootMoves, bool rankDtz, Func<bool> timeAbort)
        {
            Config config = default;
            if (!Enabled || rootMoves.Empty()) return config;
            config.useRule50 = options["Syzygy50MoveRule"].AsInt() != 0;
            config.probeDepth = options["SyzygyProbeDepth"].AsInt();
            config.cardinality = options["SyzygyProbeLimit"].AsInt();
            bool dtzAvailable = true;
            if (config.cardinality > tables.MaxCardinality) { config.cardinality = tables.MaxCardinality; config.probeDepth = 0; }
            if (config.cardinality >= pos.Count(PieceType.ALL_PIECES) && !pos.CanCastle(CastlingRights.ANY_CASTLING))
            {
                rankDtz = rankDtz || pos.DtzIsDtm();
                config.rootInTB = RootProbeCore(pos, rootMoves, config.useRule50, rankDtz, timeAbort);
                if (!config.rootInTB && !timeAbort()) { dtzAvailable = false; config.rootInTB = RootProbeWdlCore(pos, rootMoves, config.useRule50); }
            }
            if (config.rootInTB)
            {
                rootMoves.StableSort(0, rootMoves.Count, (a, b) => b.tbRank.CompareTo(a.tbRank));
                if (dtzAvailable || rootMoves[0].tbScore <= VALUE_DRAW) config.cardinality = 0;
            }
            else for (int i = 0; i < rootMoves.Count; ++i) rootMoves[i].tbRank = 0;
            return config;
        }

        public bool RootProbe(Position pos, RootMoves moves, bool rule50, bool rankDtz, Func<bool>? timeAbort = null)
        {
            if (pos == null || moves == null) throw new ArgumentNullException();
            lifetime.EnterReadLock();
            try { RequireAlive(); return Enabled && RootProbeCore(pos, moves, rule50, rankDtz, timeAbort ?? NeverAbort); }
            finally { lifetime.ExitReadLock(); }
        }
        public bool RootProbeWdl(Position pos, RootMoves moves, bool rule50)
        {
            if (pos == null || moves == null) throw new ArgumentNullException();
            lifetime.EnterReadLock();
            try { RequireAlive(); return Enabled && RootProbeWdlCore(pos, moves, rule50); }
            finally { lifetime.ExitReadLock(); }
        }

        private bool RootProbeCore(Position pos, RootMoves moves, bool rule50, bool rankDtz, Func<bool> timeAbort)
        {
            var bank = queryScratch.Value!;
            var slot = bank.Acquire();
            try { return RootProbeCoreImpl(pos, moves, rule50, rankDtz, timeAbort, slot); }
            finally { bank.Release(); }
        }
        private bool RootProbeCoreImpl(Position pos, RootMoves moves, bool rule50, bool rankDtz, Func<bool> timeAbort, ProbeScratch slot)
        {
            if (!Enabled) return false;
            ProbeState result = ProbeState.OK; var state = slot.State;
            int cnt50 = pos.Rule50Count(); bool rep = pos.HasRepeated();
            int bound = rule50 ? MaxDtz / 2 - 100 : 1;
            for (int i = 0; i < moves.Count; ++i)
            {
                ref RootMove m = ref moves[i]; int dtz;
                pos.DoMove(m.pv[0], state);
                try
                {
                    if (pos.Rule50Count() == 0) dtz = (-Search(pos, ref result, false)).DtzBeforeZeroing();
                    else if ((rule50 && pos.IsDraw(1, slot.Responses)) || pos.IsRepetition(1)) dtz = 0;
                    else { dtz = -ProbeDtzCore(pos, ref result); dtz = dtz > 0 ? dtz + 1 : dtz < 0 ? dtz - 1 : dtz; }
                    if (pos.Checkers() != 0 && dtz == 2 && MoveGeneration.MoveGenerator.Generate(pos, slot.Responses, 0, MoveGeneration.GenType.LEGAL) == 0) dtz = 1;
                }
                finally { pos.UndoMove(m.pv[0]); }
                if (timeAbort() || result == ProbeState.FAIL) return false;
                int rank = dtz > 0 ? (dtz + cnt50 <= 99 && !rep ? MaxDtz - (rankDtz ? dtz : 0) : MaxDtz / 2 - (dtz + cnt50))
                    : dtz < 0 ? (-dtz * 2 + cnt50 < 100 ? -MaxDtz - (rankDtz ? dtz : 0) : -MaxDtz / 2 + (-dtz + cnt50)) : 0;
                m.tbRank = rank;
                m.tbScore = rank >= bound ? VALUE_MATE - MAX_PLY - 1
                    : rank > 0 ? (Math.Max(3, rank - (MaxDtz / 2 - 200)) * Values.PawnValue) / 200
                    : rank == 0 ? (int)VALUE_DRAW
                    : rank > -bound ? (Math.Min(-3, rank + (MaxDtz / 2 - 200)) * Values.PawnValue) / 200
                    : -VALUE_MATE + MAX_PLY + 1;
            }
            return true;
        }
        private bool RootProbeWdlCore(Position pos, RootMoves moves, bool rule50)
        {
            var bank = queryScratch.Value!;
            var slot = bank.Acquire();
            try { return RootProbeWdlCoreImpl(pos, moves, rule50, slot); }
            finally { bank.Release(); }
        }
        private bool RootProbeWdlCoreImpl(Position pos, RootMoves moves, bool rule50, ProbeScratch slot)
        {
            ReadOnlySpan<int> ranks = stackalloc int[] { -MaxDtz, -MaxDtz + 101, 0, MaxDtz - 101, MaxDtz };
            ReadOnlySpan<Value> values = stackalloc Value[] { -VALUE_MATE + MAX_PLY + 1, VALUE_DRAW - 2, VALUE_DRAW, VALUE_DRAW + 2, VALUE_MATE - MAX_PLY - 1 };
            ProbeState result = ProbeState.OK; var state = slot.State;
            for (int i = 0; i < moves.Count; ++i)
            {
                ref RootMove m = ref moves[i]; WDLScore wdl;
                pos.DoMove(m.pv[0], state);
                try { wdl = pos.IsDraw(1, slot.Responses) ? WDLScore.WDLDraw : -Search(pos, ref result, false); }
                finally { pos.UndoMove(m.pv[0]); }
                if (result == ProbeState.FAIL) return false;
                m.tbRank = ranks[wdl + 2];
                if (!rule50) wdl = wdl > WDLScore.WDLDraw ? WDLScore.WDLWin : wdl < WDLScore.WDLDraw ? WDLScore.WDLLoss : WDLScore.WDLDraw;
                m.tbScore = values[wdl + 2];
            }
            return true;
        }
    }
}
