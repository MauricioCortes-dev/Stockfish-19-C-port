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
using System.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Configuration;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    public sealed partial class TbProbe
    {
        /// <summary>Validate/extend a decisive PV using DTZ. Restores Position even on callback/file errors.</summary>
        /// <remarks>The line is not a proof of shortest mate. elapsedMilliseconds is an optional deterministic clock.</remarks>
        public void ExtendPv(OptionsMap options, LimitsType limits, Position pos, ref RootMove rootMove, ref Value value,
            int multiPv, Func<double>? elapsedMilliseconds = null, Action<string>? diagnostic = null)
        {
            if (options == null || limits == null || pos == null) throw new ArgumentNullException();
            if (multiPv < 1) throw new ArgumentOutOfRangeException(nameof(multiPv));
            if (!Enabled)
            {
                lifetime.EnterReadLock();
                try { RequireAlive(); return; }
                finally { lifetime.ExitReadLock(); }
            }
            long started = Stopwatch.GetTimestamp();
            Func<double> elapsed = elapsedMilliseconds ?? (() => (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency);
            int overhead = options["Move Overhead"].AsInt(); bool rule50 = options["Syzygy50MoveRule"].AsInt() != 0;
            bool TimeAbort() => limits.Npmsec == 0 && limits.UseTimeManagement() && 2 * multiPv * elapsed() >= overhead;
            if (TimeAbort()) return;
            lifetime.EnterReadLock();
            try
            {
                RequireAlive();
                if (!Enabled) return;
                if (rootMove.pv.Count == 0) throw new ArgumentException("A root move is required.", nameof(rootMove));
                var bank = queryScratch.Value!;
                int baseDepth = bank.Depth;
                var slot = bank.Acquire();
                var played = slot.Played;
                void Play(Move move)
                {
                    var child = bank.Acquire();
                    try
                    {
                        // Grow the record before changing Position, so allocation failure is safe.
                        played.Add(move);
                        try { pos.DoMove(move, child.State); }
                        catch { played.RemoveAt(played.Count - 1); throw; }
                    }
                    catch { bank.Release(); throw; }
                }
                try
                {
                    Play(rootMove.pv[0]); int ply = 1;
                    while (ply < rootMove.pv.Count)
                    {
                        Move pvMove = rootMove.pv[ply]; var legal = LegalRoots(pos, slot.Moves);
                        Config config = RankRootMovesCore(options, pos, legal, false, TimeAbort);
                        int found = -1;
                        for (int i = 0; i < legal.Count; ++i) if (legal[i].pv[0] == pvMove) { found = i; break; }
                        // The original assumes a legal PV; truncate safely for a managed caller with stale input.
                        if (found < 0 || legal[0].tbRank != legal[found].tbRank) break;
                        ++ply; Play(pvMove);
                        if (config.rootInTB && ((rule50 && pos.IsDraw(ply, slot.Responses)) || pos.IsRepetition(ply)))
                        { pos.UndoMove(pvMove); played.RemoveAt(played.Count - 1); bank.Release(); --ply; break; }
                        if (config.rootInTB && TimeAbort()) break;
                    }
                    rootMove.pv.Resize(ply);
                    while (!(rule50 && pos.IsDraw(0, slot.Responses)))
                    {
                        if (TimeAbort()) break;
                        // Native WDL-only fallback can cycle forever here with rule50 disabled:
                        // root_probe_wdl still assigns draw ranks after counter 100. Bound that
                        // exceptional extension by its actual repetition, without a length cap.
                        if (!rule50 && pos.Rule50Count() >= 100 && pos.IsRepetition(ply)) break;
                        var legal = LegalRoots(pos, slot.Moves);
                        for (int i = 0; i < legal.Count; ++i)
                        {
                            ref RootMove candidate = ref legal[i]; Move move = candidate.pv[0];
                            var temporary = bank.Acquire();
                            try
                            {
                                pos.DoMove(move, temporary.State);
                                try
                                {
                                    int count = MoveGenerator.Generate(pos, slot.Responses, 0, GenType.LEGAL);
                                    for (int r = 0; r < count; ++r) candidate.tbRank -= pos.Capture(slot.Responses[r]) ? 100 : 1;
                                }
                                finally { pos.UndoMove(move); }
                            }
                            finally { bank.Release(); }
                        }
                        if (legal.Empty()) break;
                        legal.StableSort(0, legal.Count, (a, b) => b.tbRank.CompareTo(a.tbRank));
                        Config config = RankRootMovesCore(options, pos, legal, true, TimeAbort);
                        if (!config.rootInTB || config.cardinality > 0) break;
                        Move next = legal[0].pv[0]; rootMove.pv.PushBack(next); ++ply; Play(next);
                    }
                    if (pos.IsDraw(0, slot.Responses)) value = Value.VALUE_DRAW;
                }
                finally
                {
                    try
                    {
                        for (int i = played.Count - 1; i >= 0; --i)
                        {
                            pos.UndoMove(played[i]);
                            bank.Release();
                        }
                    }
                    finally
                    {
                        while (bank.Depth > baseDepth) bank.Release();
                    }
                }
            }
            finally { lifetime.ExitReadLock(); }
            if (TimeAbort()) diagnostic?.Invoke("Syzygy based PV extension requires more time, increase Move Overhead as needed.");
        }
        private static RootMoves LegalRoots(Position pos, Move[] buffer)
        {
            var moves = new RootMoves();
            int count = MoveGenerator.Generate(pos, buffer, 0, GenType.LEGAL);
            for (int i = 0; i < count; ++i) moves.EmplaceBack(buffer[i]);
            return moves;
        }
    }
}
