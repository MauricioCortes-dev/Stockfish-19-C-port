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
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using static Stockfish_sf_19_CSharp_Library.Types.Value;
using static Stockfish_sf_19_CSharp_Library.Types.ChessConstants;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        /// <summary>Original qsearch, PV or NonPV. PrepareQuiescence establishes sentinel references for a standalone call.</summary>
        public Value Quiescence(Position pos, int frame, Value alpha, Value beta, NodeType nodeType)
        {
            var local = AcquireScratch();
            try { return QuiescenceCore(pos, frame, alpha, beta, nodeType, local); }
            finally { ReleaseScratch(); }
        }
        private Value QuiescenceCore(Position pos, int frame, Value alpha, Value beta, NodeType nodeType, SearchScratch local)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos));
            if (nodeType != NodeType.PV && nodeType != NodeType.NonPV)
                throw new ArgumentOutOfRangeException(nameof(nodeType));
            bool PvNode = nodeType == NodeType.PV;
            if (alpha < -VALUE_INFINITE || alpha >= beta || beta > VALUE_INFINITE || (!PvNode && alpha != beta - 1))
                throw new ArgumentException("Invalid search window.");
            if (frame < 7 || frame + 1 >= Frames.Length || (uint)Frames[frame].ply > MAX_PLY)
                throw new ArgumentOutOfRangeException(nameof(frame));
            bool wallClockLimit = Manager.HasWallClockLimit(this);
            if (wallClockLimit)
            {
                if (IsMainThread) Manager.CheckWallTime(this);
                if (Control.IsStopped) return ValueDraw(Nodes);
            }
            ref Stack ss = ref Frames[frame];
            if (alpha < VALUE_DRAW && pos.UpcomingRepetition(ss.ply))
            {
                alpha = ValueDraw(Nodes);
                if (alpha >= beta)
                    return alpha;
            }

            PVMovesReference.Owner pv = local.Pv;
            // Reuse storage, but reproduce PVMoves pv; construction on every invocation.
            pv.Value.Clear();
            StateInfo st = local.State;
            Value bestValue, value, futilityBase = default;
            bool givesCheck, capture;
            int moveCount;
            if (PvNode)
            {
                Frames[frame + 1].pv = pv;
                ss.pv!.Get().Clear();
            }

            Move bestMove = Move.None();
            ss.inCheck = pos.Checkers() != 0;
            moveCount = 0;
            if (PvNode && SelDepth < ss.ply + 1)
                SelDepth = ss.ply + 1;
            if (pos.IsDraw(ss.ply, local.GeneratedMoves) || ss.ply >= MAX_PLY)
                return ss.ply >= MAX_PLY && !ss.inCheck ? Evaluate(pos) : VALUE_DRAW;
            Key posKey = pos.Key();
            var(ttHit, ttData, ttWriter) = TT.Probe(posKey);
            ss.ttHit = ttHit;
            ttData.move = ttHit ? ttData.move : Move.None();
            ttData.value = ttHit ? TranspositionValues.ValueFromTT(ttData.value, ss.ply, pos.Rule50Count()) : VALUE_NONE;
            bool pvHit = ttHit && ttData.is_pv;
            if (!PvNode && ttData.depth >= Depth.DEPTH_QS && ttData.value.IsValid() && (ttData.bound & (ttData.value >= beta ? Bound.BOUND_LOWER : Bound.BOUND_UPPER)) != 0)
                return ttData.value;
            Value unadjustedStaticEval = VALUE_NONE;
            if (ss.inCheck)
                bestValue = futilityBase = -VALUE_INFINITE;
            else
            {
                int correctionValue = CorrectionValue(pos, frame);
                if (ss.ttHit)
                {
                    unadjustedStaticEval = ttData.eval;
                    if (!unadjustedStaticEval.IsValid())
                        unadjustedStaticEval = Evaluate(pos);
                    ss.staticEval = bestValue = ToCorrectedStaticEval(unadjustedStaticEval, correctionValue);
                    if (ttData.value.IsValid() && !ttData.value.IsDecisive() && (ttData.bound & (ttData.value > bestValue ? Bound.BOUND_LOWER : Bound.BOUND_UPPER)) != 0)
                        bestValue = ttData.value;
                }
                else
                {
                    unadjustedStaticEval = Evaluate(pos);
                    ss.staticEval = bestValue = ToCorrectedStaticEval(unadjustedStaticEval, correctionValue);
                }

                if (bestValue >= beta)
                {
                    if (!bestValue.IsDecisive())
                        bestValue = (441 * bestValue + 583 * beta) / 1024;
                    if (!ss.ttHit)
                        ttWriter.Write(posKey, VALUE_NONE, false, Bound.BOUND_LOWER, Depth.DEPTH_UNSEARCHED, Move.None(), unadjustedStaticEval, TT.Generation);
                    return bestValue;
                }

                if (bestValue > alpha)
                    alpha = bestValue;
                futilityBase = ss.staticEval + 306;
            }

            var contHist = local.Continuations;
            contHist[0] = Frames[frame - 1].continuationHistory!;
            Square prevSq = Frames[frame - 1].currentMove.IsOk() ? Frames[frame - 1].currentMove.ToSq() : Square.SQ_NONE;
            var mp = local.Picker;
            mp.Reset(pos, ttData.move, Depth.DEPTH_QS, MainHistory, LowPlyHistory, CaptureHistory, contHist, SharedHistory, ss.ply);
            Move move;
            while ((move = mp.NextMove()) != Move.None())
            {
                if (!pos.Legal(move))
                    continue;
                givesCheck = pos.GivesCheck(move);
                capture = pos.CaptureStage(move);
                ++moveCount;
                if (!bestValue.IsLoss())
                {
                    if (!givesCheck && move.ToSq() != prevSq && !futilityBase.IsLoss() && move.TypeOf() != MoveType.PROMOTION)
                    {
                        if (moveCount > 2)
                            continue;
                        Value futilityValue = futilityBase + Values.PieceValue[pos.PieceOn(move.ToSq())];
                        if (futilityValue <= alpha)
                        {
                            bestValue = Math.Max(bestValue, futilityValue);
                            continue;
                        }

                        if (!pos.SeeGe(move, alpha - futilityBase))
                        {
                            bestValue = Math.Max(bestValue, Math.Min(alpha, futilityBase));
                            continue;
                        }
                    }

                    if (!capture)
                        continue;
                    if (!pos.SeeGe(move, -74))
                        continue;
                }

                DoMove(pos, move, st, givesCheck, frame);
                // Finally preserves the owner's state even if a managed dependency throws.
                try
                {
                    value = -Quiescence(pos, frame + 1, -beta, -alpha, nodeType);
                }
                finally
                {
                    UndoMove(pos, move);
                }

                // Do not publish an interrupted qsearch result to TT/PV. Secondary
                // workers also unwind promptly when the main worker reaches time.
                if (wallClockLimit && Control.IsStopped) return ValueDraw(Nodes);

                if (value > bestValue)
                {
                    bestValue = value;
                    if (value > alpha)
                    {
                        bestMove = move;
                        if (PvNode)
                            ss.pv!.Get().Update(move, Frames[frame + 1].pv?.Get());
                        if (value < beta)
                            alpha = value;
                        else
                            break;
                    }
                }
            }

            if (moveCount == 0)
            {
                if (ss.inCheck)
                    return MatedIn(ss.ply);
                Color us = pos.SideToMove();
                if ((Bitboards.PawnSinglePushBB(us, pos.Pieces(us, PieceType.PAWN)) & ~pos.Pieces()) == 0 && pos.NonPawnMaterial(us) == 0 && PieceType.TypeOf(pos.CapturedPiece()) >= PieceType.KNIGHT && MoveGenerator.Generate(pos, local.GeneratedMoves, 0, GenType.LEGAL) == 0)
                    bestValue = VALUE_DRAW;
            }

            if (!bestValue.IsDecisive() && bestValue > beta)
                bestValue = (462 * bestValue + 562 * beta) / 1024;
            ttWriter.Write(posKey, TranspositionValues.ValueToTT(bestValue, ss.ply), pvHit, bestValue >= beta ? Bound.BOUND_LOWER : Bound.BOUND_UPPER, Depth.DEPTH_QS, bestMove, unadjustedStaticEval, TT.Generation);
            return bestValue;
        }
    }
}
