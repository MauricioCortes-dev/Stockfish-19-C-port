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
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Evaluation;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Worker-local state, main search, quiescence and iterative control, scheduled by ThreadPool or an exclusive caller.</summary>
    /// <remarks>Exclusive worker access. Network weights are shared read-only; TT lifecycle requires quiescence.</remarks>
    public sealed partial class Worker
    {
        public readonly ButterflyHistory.Owner MainHistory = new ButterflyHistory.Owner();
        public readonly LowPlyHistory.Owner LowPlyHistory = new LowPlyHistory.Owner();
        public readonly CapturePieceToHistory.Owner CaptureHistory = new CapturePieceToHistory.Owner();
        public readonly ContinuationCorrectionHistory ContinuationCorrectionHistory = new ContinuationCorrectionHistory();
        public TTMoveHistory TTMoveHistory;
        public SharedHistories SharedHistory { get; }
        public Network Network { get; }
        public TranspositionTable TT { get; }
        public Position RootPosition { get; } = new Position();
        public StateInfo RootState { get; } = new StateInfo();
        public RootMoves RootMoves { get; } = new RootMoves();
        public AccumulatorStack AccumulatorStack { get; } = new AccumulatorStack();
        public AccumulatorCaches RefreshTable { get; }

        public readonly Stack[] Frames = new Stack[ChessConstants.MAX_PLY + 10];
        public readonly int[] Reductions = new int[ChessConstants.MAX_MOVES];
        public readonly int[] Optimism = new int[2];
        private RelaxedAtomic<ulong, AtomicUInt64> nodes;
        public ulong Nodes => nodes.Load();
        private RelaxedAtomic<ulong, AtomicUInt64> tbHits;
        public ulong TbHits => tbHits.Load();
        public int SelDepth { get; private set; }

        private readonly UIntPtr numaThreadIdx, numaTotal;
        // One slot per active invocation, including recursive calls at the same frame.
        private sealed class SearchScratch
        {
            internal readonly StateInfo State = new StateInfo();
            internal readonly PVMovesReference.Owner Pv = new PVMovesReference.Owner();
            internal readonly ExtMove[] ScoredMoves = new ExtMove[ChessConstants.MAX_MOVES];
            internal readonly Move[] GeneratedMoves = new Move[ChessConstants.MAX_MOVES];
            internal readonly PieceToHistoryReference[] Continuations = new PieceToHistoryReference[6];
            internal readonly MovePicker Picker;
            internal SearchScratch() { Picker = new MovePicker(ScoredMoves, GeneratedMoves); }
        }
        private readonly List<SearchScratch> scratch = new List<SearchScratch>();
        private int scratchDepth;
        private SearchScratch AcquireScratch()
        {
            if (scratchDepth == scratch.Count) scratch.Add(new SearchScratch());
            return scratch[scratchDepth++];
        }
        private void ReleaseScratch()
        {
            if (--scratchDepth != 0) return;
            // Inner callers may still need their parents' PVs and state chains.
            // Detach only once the complete outer activation has returned.
            foreach (var slot in scratch)
            {
                slot.State.previous = null;
                slot.Picker.Detach();
                Array.Clear(slot.Continuations, 0, slot.Continuations.Length);
            }
        }
        private readonly PVMovesReference.Owner[] pvs = new PVMovesReference.Owner[ChessConstants.MAX_PLY + 10];
        public Worker(Network network, TranspositionTable tt, SharedHistories shared, int partition = 0, int partitions = 1)
        {
            Network = network ?? throw new ArgumentNullException(nameof(network));
            TT = tt ?? throw new ArgumentNullException(nameof(tt));
            SharedHistory = shared ?? throw new ArgumentNullException(nameof(shared));
            if (partitions <= 0 || (uint)partition >= (uint)partitions)
                throw new ArgumentOutOfRangeException(nameof(partition));
            _ = shared.ContinuationHistory;
            numaThreadIdx = new UIntPtr((uint)partition);
            numaTotal = new UIntPtr((uint)partitions);
            RefreshTable = new AccumulatorCaches(network);
            for (int i = 0; i < Frames.Length; ++i)
            {
                pvs[i] = new PVMovesReference.Owner();
            }

            Clear();
        }

        public void Clear()
        {
            short main = -5, capture = -742;
            MainHistory.Value.Fill(in main);
            CaptureHistory.Value.Fill(in capture);
            SharedHistory.CorrectionHistory.ClearRange(-5, numaThreadIdx, numaTotal);
            SharedHistory.ClearPawnRange(-1338, numaThreadIdx, numaTotal);
            if (numaThreadIdx == UIntPtr.Zero)
                for (int check = 0; check < 2; ++check)
                    for (int captureType = 0; captureType < 2; ++captureType)
                        SharedHistory.ContinuationHistory[check, captureType].Fill(-586);
            TTMoveHistory.Assign(0);
            ContinuationCorrectionHistory.Fill(5);
            for (int i = 1; i < Reductions.Length; ++i)
                Reductions[i] = (int)(2872 / 128.0 * Math.Log(i));
            RefreshTable.Clear(Network);
        }

        /// <summary>Prepare a root frame with the same seven sentinel frames as iterative_deepening.</summary>
        public int PrepareQuiescence(int ply = 0)
        {
            if ((uint)ply > ChessConstants.MAX_PLY)
                throw new ArgumentOutOfRangeException(nameof(ply));
            AccumulatorStack.Reset();
            nodes.Store(0);
            SelDepth = 0;
            Array.Clear(Frames, 0, Frames.Length);
            for (int i = 0; i < Frames.Length; ++i)
            {
                if (i < 7)
                {
                    Frames[i].continuationHistory = SharedHistory.ContinuationHistory[0, 0].Reference(0, 0);
                    Frames[i].continuationCorrectionHistory = ContinuationCorrectionHistory.Reference(0, 0);
                    Frames[i].staticEval = Value.VALUE_NONE;
                }
                else
                    Frames[i].ply = ply + i - 7;
                pvs[i].Value.Clear();
            }

            Frames[7].pv = pvs[7];
            return 7;
        }

        public Value Evaluate(Position pos) => Evaluator.Evaluate(Network, pos, AccumulatorStack, RefreshTable, Optimism[pos.SideToMove()]);
        public void DoMove(Position pos, Move move, StateInfo state, int? frame = null) => DoMove(pos, move, state, pos.GivesCheck(move), frame);
        public void DoMove(Position pos, Move move, StateInfo state, bool givesCheck, int? frame = null)
        {
            bool capture = pos.CaptureStage(move);
            nodes.PostIncrement();
            ref Dirties dirties = ref AccumulatorStack.Push();
            TT.Prefetch(pos.PrefetchKey(move));
            pos.DoMove(move, state, givesCheck, ref dirties);
            if (frame.HasValue)
            {
                ref Stack ss = ref Frames[frame.Value];
                ss.currentMove = move;
                ss.continuationHistory = SharedHistory.ContinuationHistory[ss.inCheck ? 1 : 0, capture ? 1 : 0].Reference(dirties.dirtyPiece.pc, move.ToSq());
                ss.continuationCorrectionHistory = ContinuationCorrectionHistory.Reference(dirties.dirtyPiece.pc, move.ToSq());
            }
        }

        public void DoNullMove(Position pos, StateInfo state, int frame)
        {
            pos.DoNullMove(state);
            Frames[frame].currentMove = Move.Null();
            Frames[frame].continuationHistory = SharedHistory.ContinuationHistory[0, 0].Reference(0, 0);
            Frames[frame].continuationCorrectionHistory = ContinuationCorrectionHistory.Reference(0, 0);
        }

        public void UndoMove(Position pos, Move move)
        {
            pos.UndoMove(move);
            AccumulatorStack.Pop();
        }

        public void UndoNullMove(Position pos) => pos.UndoNullMove();
        public static Value ValueDraw(ulong nodes) => Value.VALUE_DRAW - 1 + (int)(nodes & 2);
        public static Value ToCorrectedStaticEval(Value value, int correction) => Math.Min(Math.Max(checked((int)value + correction / 131072), (int)Value.VALUE_TB_LOSS_IN_MAX_PLY + 1), (int)Value.VALUE_TB_WIN_IN_MAX_PLY - 1);
        public int CorrectionValue(Position pos, int frame)
        {
            Color us = pos.SideToMove();
            Move m = Frames[frame - 1].currentMove;
            int pcv = CorrectionHistoryEntry.At(ref SharedHistory.PawnCorrectionEntry(pos), us).Pawn.Load();
            int micv = CorrectionHistoryEntry.At(ref SharedHistory.MinorPieceCorrectionEntry(pos), us).Minor.Load();
            int wnpcv = CorrectionHistoryEntry.At(ref SharedHistory.NonPawnCorrectionEntry(pos, Color.WHITE), us).NonPawnWhite.Load();
            int bnpcv = CorrectionHistoryEntry.At(ref SharedHistory.NonPawnCorrectionEntry(pos, Color.BLACK), us).NonPawnBlack.Load();
            int cntcv = m.IsOk() ? 8761 * (Frames[frame - 2].continuationCorrectionHistory!.Get()[pos.PieceOn(m.ToSq()), m.ToSq()] + Frames[frame - 4].continuationCorrectionHistory!.Get()[pos.PieceOn(m.ToSq()), m.ToSq()]) : 64049;
            return checked(15341 * pcv + 10569 * micv + 12906 * (wnpcv + bnpcv) + cntcv);
        }

        public void UpdateCorrectionHistory(Position pos, int frame, int bonus)
        {
            Move m = Frames[frame - 1].currentMove;
            Color us = pos.SideToMove();
            CorrectionHistoryEntry.At(ref SharedHistory.PawnCorrectionEntry(pos), us).Pawn.Update(bonus);
            CorrectionHistoryEntry.At(ref SharedHistory.MinorPieceCorrectionEntry(pos), us).Minor.Update(checked(bonus * 150) / 128);
            CorrectionHistoryEntry.At(ref SharedHistory.NonPawnCorrectionEntry(pos, Color.WHITE), us).NonPawnWhite.Update(checked(bonus * 186) / 128);
            CorrectionHistoryEntry.At(ref SharedHistory.NonPawnCorrectionEntry(pos, Color.BLACK), us).NonPawnBlack.Update(checked(bonus * 186) / 128);
            if (m.IsOk())
            {
                Square to = m.ToSq();
                Piece pc = pos.PieceOn(to);
                PieceToCorrectionHistory.At(ref Frames[frame - 2].continuationCorrectionHistory!.Get(), pc, to).Update(checked(bonus * 130) / 128);
                PieceToCorrectionHistory.At(ref Frames[frame - 4].continuationCorrectionHistory!.Get(), pc, to).Update(checked(bonus * 70) / 128);
            }
        }
    }
}
