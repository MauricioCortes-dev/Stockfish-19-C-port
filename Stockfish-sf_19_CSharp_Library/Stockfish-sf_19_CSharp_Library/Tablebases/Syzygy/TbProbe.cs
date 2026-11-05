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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Owned Syzygy WDL/DTZ service. Concurrent queries require separate Position objects.</summary>
    /// <remarks>Init/Dispose exclude in-flight queries. Engine lends one instance to its search workers.</remarks>
    public sealed partial class TbProbe : IDisposable
    {
        public const int MaxSupportedPieces = 7;
        private readonly ReaderWriterLockSlim lifetime = new ReaderWriterLockSlim();
        private readonly SyzygyIndex index = new SyzygyIndex();
        private TBTables tables = new TBTables();
        private string[] paths = new string[0];
        private bool disposed;

        public bool Enabled { get; }
        public TbProbe(string paths = "", bool enabled = true)
        {
#if STOCKFISH_NO_TABLEBASES
            Enabled = false;
#else
            Enabled = enabled;
#endif
            Init(paths);
        }
        public int MaxCardinality { get { lifetime.EnterReadLock(); try { RequireAlive(); return tables.MaxCardinality; } finally { lifetime.ExitReadLock(); } } }
        public int WdlFileCount { get { lifetime.EnterReadLock(); try { RequireAlive(); return tables.FoundWdlFiles; } finally { lifetime.ExitReadLock(); } } }
        public int DtzFileCount { get { lifetime.EnterReadLock(); try { RequireAlive(); return tables.FoundDtzFiles; } finally { lifetime.ExitReadLock(); } } }
        public string Info { get { lifetime.EnterReadLock(); try { RequireAlive(); return tables.Info(); } finally { lifetime.ExitReadLock(); } } }

        /// <summary>Discovers canonical material files up to seven pieces; loads no table payload.</summary>
        public void Init(string paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            string[] nextPaths = paths.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            lifetime.EnterWriteLock();
            try
            {
                RequireAlive(); tables.Clear(); this.paths = nextPaths;
                if (Enabled) tables.Initialize(nextPaths);
            }
            finally { lifetime.ExitWriteLock(); }
        }

        /// <summary>WDL from the side to move; FAIL is distinct from a successful draw.</summary>
        /// <remarks>Like C++, probing assumes no castling rights. Rule50 is not adjudicated here.
        /// Corrupt tables throw InvalidDataException rather than terminating the host process.</remarks>
        public WDLScore ProbeWdl(Position pos, out ProbeState result)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            lifetime.EnterReadLock();
            try
            {
                RequireAlive();
                if (!Enabled) { result = ProbeState.FAIL; return WDLScore.WDLDraw; }
                result = ProbeState.OK;
                if (pos.CanCastle(CastlingRights.ANY_CASTLING)) throw new ArgumentException("Syzygy does not encode castling rights.", nameof(pos));
                return Search(pos, ref result, false);
            }
            finally { lifetime.ExitReadLock(); }
        }

        // Local capture/zeroing search, unrelated to Worker alpha-beta. true includes DTZ zeroing moves.
        internal WDLScore Search(Position pos, ref ProbeState result, bool checkZeroingMoves)
        {
            var bank = queryScratch.Value!;
            var slot = bank.Acquire();
            try { return SearchCore(pos, ref result, checkZeroingMoves, slot); }
            finally { bank.Release(); }
        }

        private WDLScore SearchCore(Position pos, ref ProbeState result, bool checkZeroingMoves, ProbeScratch slot)
        {
            WDLScore value, bestValue = WDLScore.WDLLoss;
            var state = slot.State;
            int count = MoveGenerator.Generate(pos, slot.Moves, 0, GenType.LEGAL);
            int moveCount = 0;
            for (int i = 0; i < count; ++i)
            {
                Move move = slot.Moves[i];
                if (!pos.Capture(move) && (!checkZeroingMoves || PieceType.TypeOf(pos.MovedPiece(move)) != PieceType.PAWN)) continue;
                ++moveCount;
                pos.DoMove(move, state);
                try { value = -Search(pos, ref result, false); }
                finally { pos.UndoMove(move); }
                if (result == ProbeState.FAIL) return WDLScore.WDLDraw;
                if (value > bestValue)
                {
                    bestValue = value;
                    if (value >= WDLScore.WDLWin) { result = ProbeState.ZEROING_BEST_MOVE; return value; }
                }
            }
            bool noMoreMoves = moveCount != 0 && moveCount == count;
            if (noMoreMoves) value = bestValue;
            else
            {
                value = ProbeTable(pos, ref result);
                if (result == ProbeState.FAIL) return WDLScore.WDLDraw;
            }
            if (bestValue >= value)
            {
                result = bestValue > WDLScore.WDLDraw || noMoreMoves ? ProbeState.ZEROING_BEST_MOVE : ProbeState.OK;
                return bestValue;
            }
            result = ProbeState.OK; return value;
        }

        private WDLScore ProbeTable(Position pos, ref ProbeState result)
        {
            if (pos.Count(PieceType.ALL_PIECES) == 2) return WDLScore.WDLDraw;
            Debug.Assert(pos.MaterialKeyIsOk());
            TBTable? entry = tables.Get(pos.MaterialKey(), TBType.WDL);
            if (entry == null) { result = ProbeState.FAIL; return WDLScore.WDLDraw; }
            TBTable.Loaded loaded = entry.Map(paths, index);
            if (loaded.File == null) { result = ProbeState.FAIL; return WDLScore.WDLDraw; }
            return DoProbeTable(pos, entry, loaded, WDLScore.WDLDraw, ref result);
        }
        private void RequireAlive() { if (disposed) throw new ObjectDisposedException(nameof(TbProbe)); }
        public void Dispose()
        {
            lifetime.EnterWriteLock();
            try { if (!disposed) { disposed = true; tables.Dispose(); queryScratch.Dispose(); paths = new string[0]; } }
            finally { lifetime.ExitWriteLock(); }
            // Keep the lock usable so late callers deterministically observe ObjectDisposedException.
        }
    }
}
