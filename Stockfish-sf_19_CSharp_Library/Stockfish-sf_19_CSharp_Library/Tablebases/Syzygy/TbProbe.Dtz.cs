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
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    public sealed partial class TbProbe
    {
        /// <summary>Signed DTZ in plies, including the original cursed/blessed offsets and rounding.</summary>
        public int ProbeDtz(Position pos, out ProbeState result)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            lifetime.EnterReadLock();
            try
            {
                RequireAlive();
                if (!Enabled) { result = ProbeState.FAIL; return 0; }
                if (pos.CanCastle(CastlingRights.ANY_CASTLING)) throw new ArgumentException("Syzygy does not encode castling rights.", nameof(pos));
                result = ProbeState.OK;
                return ProbeDtzCore(pos, ref result);
            }
            finally { lifetime.ExitReadLock(); }
        }

        private static bool CheckDtzStm(TBTable entry, PairsData data, int stm) =>
            (data.Flags & TBFlag.STM) == stm || (entry.Key == entry.Key2 && !entry.HasPawns);

        private static int MapDtzScore(PairsData d, TBTable.Loaded loaded, int value, WDLScore wdl)
        {
            ReadOnlySpan<int> wdlMap = stackalloc int[] { 1, 3, 0, 2, 0 };
            if ((d.Flags & TBFlag.Mapped) != 0)
            {
                int which = wdlMap[wdl + 2];
                if ((uint)value >= d.MapCount[which]) throw loaded.File!.Corrupt("DTZ score-map index");
                int offset = d.MapIdx[which] + value;
                value = (d.Flags & TBFlag.Wide) != 0
                    ? loaded.File!.U16(loaded.Map + 2L * offset)
                    : loaded.File!.U8(loaded.Map + offset);
            }
            if ((wdl == WDLScore.WDLWin && (d.Flags & TBFlag.WinPlies) == 0)
                || (wdl == WDLScore.WDLLoss && (d.Flags & TBFlag.LossPlies) == 0)
                || wdl == WDLScore.WDLCursedWin || wdl == WDLScore.WDLBlessedLoss)
                value *= 2;
            return value + 1;
        }

        private int ProbeDtzTable(Position pos, ref ProbeState result, WDLScore wdl)
        {
            if (pos.Count(PieceType.ALL_PIECES) == 2) return 0;
            TBTable? entry = tables.Get(pos.MaterialKey(), TBType.DTZ);
            if (entry == null) { result = ProbeState.FAIL; return 0; }
            TBTable.Loaded loaded = entry.Map(paths, index);
            if (loaded.File == null) { result = ProbeState.FAIL; return 0; }
            return DoProbeTable(pos, entry, loaded, wdl, ref result);
        }

        private int ProbeDtzCore(Position pos, ref ProbeState result)
        {
            var bank = queryScratch.Value!;
            var slot = bank.Acquire();
            try { return ProbeDtzCoreImpl(pos, ref result, slot); }
            finally { bank.Release(); }
        }

        private int ProbeDtzCoreImpl(Position pos, ref ProbeState result, ProbeScratch slot)
        {
            result = ProbeState.OK;
            WDLScore wdl = Search(pos, ref result, true);
            if (result == ProbeState.FAIL || wdl == WDLScore.WDLDraw) return 0;
            if (result == ProbeState.ZEROING_BEST_MOVE) return wdl.DtzBeforeZeroing();
            int dtz = ProbeDtzTable(pos, ref result, wdl);
            if (result == ProbeState.FAIL) return 0;
            if (result != ProbeState.CHANGE_STM)
                return (dtz + 100 * (wdl == WDLScore.WDLBlessedLoss || wdl == WDLScore.WDLCursedWin ? 1 : 0)) * SyzygyMath.SignOf(wdl);
            var state = slot.State; int minDtz = 0xFFFF;
            int count = MoveGenerator.Generate(pos, slot.Moves, 0, GenType.LEGAL);
            for (int i = 0; i < count; ++i)
            {
                Move move = slot.Moves[i];
                bool zeroing = pos.Capture(move) || PieceType.TypeOf(pos.MovedPiece(move)) == PieceType.PAWN;
                pos.DoMove(move, state);
                try
                {
                    dtz = zeroing ? -Search(pos, ref result, false).DtzBeforeZeroing() : -ProbeDtzCore(pos, ref result);
                    if (dtz == 1 && pos.Checkers() != 0 && MoveGenerator.Generate(pos, slot.Responses, 0, GenType.LEGAL) == 0) minDtz = 1;
                    if (!zeroing) dtz += SyzygyMath.SignOf(dtz);
                    if (dtz < minDtz && SyzygyMath.SignOf(dtz) == SyzygyMath.SignOf(wdl)) minDtz = dtz;
                }
                finally { pos.UndoMove(move); }
                if (result == ProbeState.FAIL) return 0;
            }
            return minDtz == 0xFFFF ? -1 : minDtz;
        }
    }
}
