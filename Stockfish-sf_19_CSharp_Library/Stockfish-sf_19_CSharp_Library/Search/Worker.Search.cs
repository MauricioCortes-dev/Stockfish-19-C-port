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
using Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy;
using static Stockfish_sf_19_CSharp_Library.Types.Value;
using static Stockfish_sf_19_CSharp_Library.Types.ChessConstants;
using static Stockfish_sf_19_CSharp_Library.Types.Bound;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        private const ulong NodesLimitOutput = 10_000_000UL;
        private static readonly int[] lmrDivisor =
        {
            3637,
            2787,
            2761,
            2939,
            3171,
            3347,
            3147,
            2762,
            2772,
            3106,
            3107,
            3060,
            3112,
            2991,
            3090,
            3542
        };
        /// <summary>Main search for Root, PV and NonPV; PrepareSearch establishes the standalone context.</summary>
        /// <remarks>Supports Syzygy when a borrowed service and configuration are supplied. The caller owns root ordering and iterative control.</remarks>
        public Value Search(Position pos, int frame, Value alpha, Value beta, Depth depth, bool cutNode, NodeType nodeType)
        {
            var local = AcquireScratch();
            try { return SearchCore(pos, frame, alpha, beta, depth, cutNode, nodeType, local); }
            finally { ReleaseScratch(); }
        }
        private Value SearchCore(Position pos, int frame, Value alpha, Value beta, Depth depth, bool cutNode, NodeType nodeType, SearchScratch local)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos));
            if (nodeType != NodeType.Root && nodeType != NodeType.PV && nodeType != NodeType.NonPV)
                throw new ArgumentOutOfRangeException(nameof(nodeType));
            if (alpha < -VALUE_INFINITE || alpha >= beta || beta > VALUE_INFINITE || (nodeType == NodeType.NonPV && alpha != beta - 1) || (nodeType != NodeType.NonPV && cutNode))
                throw new ArgumentException("Invalid search window or cut node.");
            if (frame < 7 || frame + 2 >= Frames.Length || (uint)Frames[frame].ply > MAX_PLY)
                throw new ArgumentOutOfRangeException(nameof(frame));
            if (nodeType == NodeType.Root && (PvIndex < 0 || PvIndex >= PvLast || PvLast > RootMoves.Count))
                throw new InvalidOperationException("Root requires a nonempty candidate range.");
            if (nodeType != NodeType.NonPV && Frames[frame].pv == null)
                throw new InvalidOperationException("PV nodes require a live PV reference.");
            if (RootDelta <= 0)
                throw new InvalidOperationException("RootDelta must be positive.");
            ref Stack ss = ref Frames[frame];
            checked
            {
                bool PvNode = nodeType != NodeType.NonPV;
                bool rootNode = nodeType == NodeType.Root;
                bool allNode = !(PvNode || cutNode);
                bool seekMate = RootDepth >= 16 && Math.Abs(RootMoves[PvIndex].score) >= 2000;
                // Dive into quiescence search when the depth reaches zero
                if (depth <= 0)
                    return Quiescence(pos, frame, alpha, beta, PvNode ? NodeType.PV : NodeType.NonPV);
                // Limit the depth if extensions made it too large
                depth = Math.Min(depth, MAX_PLY - 1);
                // Check if we have an upcoming move that draws by repetition
                if (!rootNode && alpha < VALUE_DRAW && pos.UpcomingRepetition(ss.ply))
                {
                    alpha = ValueDraw(Nodes);
                    if (alpha >= beta)
                        return alpha;
                }

                var pv = local.Pv;
                pv.Value.Clear();
                var st = local.State;
                Key posKey;
                Move move, excludedMove, bestMove;
                Depth extension, newDepth;
                Value bestValue, value = default, eval = default, maxValue, probCutBeta;
                bool givesCheck, improving, priorCapture, opponentWorsening;
                bool capture, ttCapture;
                int priorReduction;
                Piece movedPiece;
                SearchedList capturesSearched = default;
                SearchedList quietsSearched = default;
                // Step 1. Initialize node
                ss.inCheck = pos.Checkers() != 0;
                priorCapture = pos.CapturedPiece() != Piece.NO_PIECE;
                Color us = pos.SideToMove();
                ss.moveCount = 0;
                bestValue = -VALUE_INFINITE;
                maxValue = VALUE_INFINITE;
                ss.followPV = rootNode || (Frames[frame - 1].followPV && (unchecked((ulong)(ss.ply - 1)) < (ulong)LastIterationPV.Count && Frames[frame - 1].currentMove == LastIterationPV[ss.ply - 1]));
                // Check for the available remaining time
                if (IsMainThread) Control.Check(this);
                // Used to send SelDepth info to GUI (SelDepth counts from 1, ply from 0)
                if (PvNode && SelDepth < ss.ply + 1)
                    SelDepth = ss.ply + 1;
                if (!rootNode)
                {
                    // Step 2. Check for aborted search or immediate draw
                    if (Control.IsStopped || pos.IsDraw(ss.ply, local.GeneratedMoves) || ss.ply >= MAX_PLY)
                        return (ss.ply >= MAX_PLY && !ss.inCheck) ? Evaluate(pos) : ValueDraw(Nodes);
                    // Step 3. Mate distance pruning. Even if we mate at the next move our score
                    // would be at best MateIn(ss.ply + 1), but if alpha is already bigger because
                    // a shorter mate was found upward in the tree then there is no need to search
                    // because we will never beat the current alpha. Equal and opposite logic applies
                    // when being mated. In either case, return a fail-high score.
                    alpha = Math.Max(MatedIn(ss.ply), alpha);
                    beta = Math.Min(MateIn(ss.ply + 1), beta);
                    if (alpha >= beta)
                        return alpha;
                }

                Square prevSq = (Frames[frame - 1].currentMove).IsOk() ? (Frames[frame - 1].currentMove).ToSq() : Square.SQ_NONE;
                bestMove = Move.None();
                priorReduction = Frames[frame - 1].reduction;
                Frames[frame - 1].reduction = 0;
                ss.statScore = 0;
                Frames[frame + 2].cutoffCnt = 0;
                int correctionValue = CorrectionValue(pos, frame);
                // Step 4. Transposition table lookup
                excludedMove = ss.excludedMove;
                posKey = pos.Key();
                var(ttHit, ttData, ttWriter) = TT.Probe(posKey);
                ss.ttHit = ttHit;
                ttData.move = rootNode ? RootMoves[PvIndex].pv[0] : ttHit ? ttData.move : Move.None();
                ttData.value = ttHit ? TranspositionValues.ValueFromTT(ttData.value, ss.ply, pos.Rule50Count()) : VALUE_NONE;
                ss.ttPv = excludedMove != Move.None() ? ss.ttPv : PvNode || (ttHit && ttData.is_pv);
                ttCapture = ttData.move != Move.None() && pos.CaptureStage(ttData.move);
                // Step 5. Static evaluation of the position
                Value unadjustedStaticEval = VALUE_NONE;
                // Skip early pruning when in check
                if (ss.inCheck)
                    ss.staticEval = eval = Frames[frame - 2].staticEval;
                else if (excludedMove != Move.None())
                    unadjustedStaticEval = eval = ss.staticEval;
                else if (ss.ttHit)
                {
                    // Never assume anything about values stored in TT
                    unadjustedStaticEval = ttData.eval;
                    if (!unadjustedStaticEval.IsValid())
                        unadjustedStaticEval = Evaluate(pos);
                    ss.staticEval = eval = ToCorrectedStaticEval(unadjustedStaticEval, correctionValue);
                    // ttValue can be used as a better position evaluation
                    if (ttData.value.IsValid() && (ttData.bound & (ttData.value > eval ? BOUND_LOWER : BOUND_UPPER)) != 0)
                        eval = ttData.value;
                }
                else
                {
                    unadjustedStaticEval = Evaluate(pos);
                    ss.staticEval = eval = ToCorrectedStaticEval(unadjustedStaticEval, correctionValue);
                    // Static evaluation is saved as it was before adjustment by correction history
                    ttWriter.Write(posKey, VALUE_NONE, ss.ttPv, BOUND_NONE, Depth.DEPTH_UNSEARCHED, Move.None(), unadjustedStaticEval, TT.Generation);
                }

                // Set up the improving flag, which is true if current static evaluation is
                // bigger than the previous static evaluation at our turn (if we were in
                // check at our previous move we go back until we weren't in check) and is
                // false otherwise. The improving flag is used in various pruning heuristics.
                // Similarly, opponentWorsening is true if our static evaluation is better
                // for us than at the last ply.
                improving = ss.staticEval > Frames[frame - 2].staticEval;
                opponentWorsening = ss.staticEval > -Frames[frame - 1].staticEval;
                // Hindsight adjustment of reductions based on static evaluation difference
                if (priorReduction >= 3 && !opponentWorsening)
                    depth++;
                if (priorReduction >= 2 && depth >= 2 && ss.staticEval + Frames[frame - 1].staticEval > 166)
                    depth--;
                // Step 6. At non-PV nodes we check for an early TT cutoff. Note that we
                //         always check the validity of the TT value because of access races.
                if (!PvNode && excludedMove == Move.None() && ttData.depth > depth - B(ttData.value <= beta) && ttData.value.IsValid() && (ttData.bound & (ttData.value >= beta ? BOUND_LOWER : BOUND_UPPER)) != 0 && (cutNode == (ttData.value >= beta) || depth > 4))
                {
                    // If the ttMove is quiet, update move sorting heuristics on TT hit
                    if (ttData.move != Move.None() && ttData.value >= beta)
                    {
                        // Bonus for a quiet ttMove that fails high
                        if (!ttCapture)
                            UpdateQuietHistories(pos, frame, ttData.move, Math.Min(112 * depth, 695));
                        // Extra penalty for early quiet moves of the previous ply
                        if (prevSq != Square.SQ_NONE && Frames[frame - 1].moveCount < 5 && !priorCapture)
                            UpdateContinuationHistories(frame - 1, pos.PieceOn(prevSq), prevSq, -2210);
                    }

                    // Partial workaround for the graph history interaction problem.
                    // For high rule50 counts don't produce transposition table cutoffs.
                    if (pos.Rule50Count() < 96)
                    {
                        if (depth >= 7 && ttData.move != Move.None() && pos.PseudoLegal(ttData.move, local.GeneratedMoves) && pos.Legal(ttData.move) && !ttData.value.IsDecisive())
                        {
                            pos.DoMove(ttData.move, st);
                            TTData ttDataNext;
                            try
                            {
                                ttDataNext = TT.Probe(pos.Key()).Data;
                            }
                            finally
                            {
                                pos.UndoMove(ttData.move);
                            }

                            // Check that the ttValue after the TT move would also trigger a cutoff
                            if (!ttDataNext.value.IsValid())
                                return ttData.value;
                            if ((ttData.value >= beta) == (-ttDataNext.value >= beta))
                                return ttData.value;
                        }
                        else
                            return ttData.value;
                    }
                } // No cutoff, but why? Compare the aspiration window to the inexact bound
                else if (!PvNode && excludedMove == Move.None() && ttData.depth > depth - B(ttData.value <= beta) && ttData.value.IsValid() && ttData.bound != BOUND_EXACT && (ttData.bound & (ttData.value >= beta ? BOUND_UPPER : BOUND_LOWER)) != 0 && depth > 5)
                {
                    // If such a mismatch is the only reason cutoff failed, the TT entry is now useless
                    ttWriter.Penalize(1);
                }

                // Step 7. Tablebases probe
                if (!rootNode && excludedMove == Move.None() && TbConfig.cardinality != 0 && Tablebases != null)
                {
                    int piecesCount = pos.Count(PieceType.ALL_PIECES);
                    if (piecesCount <= TbConfig.cardinality
                        && (piecesCount < TbConfig.cardinality || depth >= TbConfig.probeDepth)
                        && pos.Rule50Count() == 0 && !pos.CanCastle(CastlingRights.ANY_CASTLING))
                    {
                        WDLScore wdl = Tablebases.ProbeWdl(pos, out ProbeState err);
                        if (IsMainThread) Manager.CallsCount = 0;
                        if (err != ProbeState.FAIL)
                        {
                            tbHits.Store(unchecked(tbHits.Load() + 1));
                            int drawScore = TbConfig.useRule50 ? 1 : 0;
                            Value tbValue = VALUE_TB - ss.ply;
                            value = wdl < -drawScore ? -tbValue : wdl > drawScore ? (int)tbValue : VALUE_DRAW + 2 * wdl * drawScore;
                            Bound bound = wdl < -drawScore ? BOUND_UPPER : wdl > drawScore ? BOUND_LOWER : BOUND_EXACT;
                            if (bound == BOUND_EXACT || (bound == BOUND_LOWER ? value >= beta : value <= alpha))
                            {
                                ttWriter.Write(posKey, TranspositionValues.ValueToTT(value, ss.ply), ss.ttPv, bound,
                                    Math.Min(MAX_PLY - 1, depth + 6), Move.None(), VALUE_NONE, TT.Generation);
                                return value;
                            }
                            if (PvNode)
                            {
                                if (bound == BOUND_LOWER) { bestValue = value; alpha = Math.Max(alpha, bestValue); }
                                else maxValue = value;
                            }
                        }
                    }
                }
                if (ss.inCheck)
                    goto moves_loop;
                // Use static evaluation difference to improve quiet move ordering
                if ((Frames[frame - 1].currentMove).IsOk() && !Frames[frame - 1].inCheck && !priorCapture)
                {
                    int evalDiff = Clamp(-(int)(Frames[frame - 1].staticEval + ss.staticEval), -189, 194) + 60;
                    ButterflyHistory.At(ref MainHistory.Value, ~us, Frames[frame - 1].currentMove.Raw()).Update(evalDiff * 11);
                    if (!ttHit && PieceType.TypeOf(pos.PieceOn(prevSq)) != PieceType.PAWN && (Frames[frame - 1].currentMove).TypeOf() != MoveType.PROMOTION)
                        PawnHistoryEntry.At(ref SharedHistory.PawnEntry(pos), pos.PieceOn(prevSq), prevSq).Update(evalDiff * 13);
                }

                // Step 8. Razoring
                // If eval is really low, skip search entirely and return the qsearch value
                if (!PvNode && eval < alpha - 482 * depth * depth)
                    return Quiescence(pos, frame, alpha, beta, NodeType.NonPV);
                // Step 9. Futility pruning: child node
                // The depth condition is important for mate finding. It should NOT be tuned.
                if (!ss.ttPv && depth < (seekMate ? 6 : 19) && eval >= beta && (ttData.move == Move.None() || ttCapture) && !beta.IsLoss() && !eval.IsWin())
                {
                    Value futilityMult = Math.Min(45 + depth * 4, 85);
                    futilityMult -= 20 * B(!ss.ttHit);
                    Value futilityMargin = futilityMult * depth - (2789 * B(improving) + 335 * B(opponentWorsening)) * futilityMult / 1024 + Math.Abs(correctionValue) / 198435;
                    if (eval - futilityMargin >= beta)
                        return (661 * beta + 363 * eval) / 1024;
                }

                // Step 10. Null move search with verification search
                if (cutNode && ss.staticEval >= beta - 13 * depth - 47 * B(improving) + 365 && excludedMove == Move.None() && pos.NonPawnMaterial(us) != 0 && ss.ply >= nmpMinPly && beta >= -2000)
                {
                    // Null move dynamic reduction based on depth
                    Depth R = 7 + depth / 3 + Math.Max((ss.staticEval - beta) / 256, 0);
                    DoNullMove(pos, st, frame);
                    Value nullValue;
                    try
                    {
                        nullValue = -Search(pos, frame + 1, -beta, -beta + 1, depth - R, false, NodeType.NonPV);
                    }
                    finally
                    {
                        UndoNullMove(pos);
                    }

                    // Do not return unproven mate or TB scores
                    if (nullValue >= beta && !nullValue.IsWin())
                    {
                        if (nmpMinPly != 0 || depth < 16)
                            return nullValue;
                        // Recursive verification is not allowed
                        // Do verification search at high depths, with null move pruning
                        // disabled until ply exceeds nmpMinPly.
                        nmpMinPly = ss.ply + 3 * (depth - R) / 4;
                        Value v;
                        try
                        {
                            v = Search(pos, frame, beta - 1, beta, depth - R, false, NodeType.NonPV);
                        }
                        finally
                        {
                            nmpMinPly = 0;
                        }

                        if (v >= beta)
                            return nullValue;
                    }
                }

                improving |= ss.staticEval >= beta;
                // Step 11. Internal iterative reductions
                // At sufficient depth, reduce depth for PV/Cut Nodes without a TTMove.
                // (*Scaler) Making IIR more aggressive scales poorly.
                if (!ss.followPV && !allNode && depth >= 6 && ttData.move == Move.None())
                    depth--;
                // Step 12. ProbCut
                // If we have a good enough capture (or queen promotion) and a reduced search
                // returns a value much above beta, we can (almost) safely prune the previous move.
                probCutBeta = beta + 241 - 64 * B(improving);
                if (depth >= 3 && !beta.IsDecisive() && !(ttData.value.IsValid() && ttData.value < probCutBeta))
                {
                    var mpProb = local.Picker;
                    mpProb.Reset(pos, ttData.move, probCutBeta - ss.staticEval, CaptureHistory);
                    Depth probCutDepth = depth - (improving ? 5 : 3);
                    while ((move = mpProb.NextMove()) != Move.None())
                    {
                        if (move == excludedMove || !pos.Legal(move))
                            continue;
                        DoMove(pos, move, st, frame);
                        try
                        {
                            // Perform a preliminary qsearch to verify that the move holds
                            value = -Quiescence(pos, frame + 1, -probCutBeta, -probCutBeta + 1, NodeType.NonPV);
                            // If the qsearch held, perform the regular search
                            if (value >= probCutBeta && probCutDepth > 0)
                                value = -Search(pos, frame + 1, -probCutBeta, -probCutBeta + 1, probCutDepth, !cutNode, NodeType.NonPV);
                        }
                        finally
                        {
                            UndoMove(pos, move);
                        }

                        if (value >= probCutBeta)
                        {
                            // Save ProbCut data into transposition table
                            ttWriter.Write(posKey, TranspositionValues.ValueToTT(value, ss.ply), ss.ttPv, BOUND_LOWER, probCutDepth + 1, move, unadjustedStaticEval, TT.Generation);
                            if (!value.IsDecisive())
                                return value - (probCutBeta - beta);
                        }
                    }
                }

                moves_loop: // When in check, search starts here
                    // Step 13. A small ProbCut idea
                    probCutBeta = beta + 428;
                if ((ttData.bound & BOUND_LOWER) != 0 && ttData.depth >= depth - 4 && ttData.value >= probCutBeta && !beta.IsDecisive() && ttData.value.IsValid() && !ttData.value.IsDecisive())
                    return probCutBeta;
                PieceToHistoryReference[] contHist = local.Continuations;
                for (int i = 0; i < 6; ++i) contHist[i] = Frames[frame - i - 1].continuationHistory!;
                var mp = local.Picker;
                mp.Reset(pos, ttData.move, depth, MainHistory, LowPlyHistory, CaptureHistory, contHist, SharedHistory, ss.ply);
                value = bestValue;
                int moveCount = 0;
                // Step 14. Loop through all pseudo-legal moves until no moves remain
                // or a beta cutoff occurs.
                while ((move = mp.NextMove()) != Move.None())
                {
                    if (move == excludedMove)
                        continue;
                    // Check for legality
                    if (!pos.Legal(move))
                        continue;
                    // At root obey the "searchmoves" option and skip moves not listed in Root
                    // Move List. In MultiPV mode we also skip PV moves that have been already
                    // searched and those of lower "TB rank" if we are in a TB root position.
                    if (rootNode && RootMoves.CountMatches(move, PvIndex, PvLast) == 0)
                        continue;
                    ss.moveCount = ++moveCount;
                    if (rootNode && IsMainThread && Nodes > NodesLimitOutput)
                        Control.OnIteration?.Invoke(depth, move, moveCount + PvIndex);
                    if (PvNode)
                        Frames[frame + 1].pv = null;
                    extension = 0;
                    capture = pos.CaptureStage(move);
                    movedPiece = pos.MovedPiece(move);
                    givesCheck = pos.GivesCheck(move);
                    // Calculate new depth for this move
                    newDepth = depth - 1;
                    int delta = beta - alpha;
                    int r = Reduction(improving, depth, moveCount, delta);
                    // Increase reduction for ttPv nodes
                    // (*Scaler) Larger values scale well.
                    if (ss.ttPv)
                        r += 929;
                    // Step 15. Pruning at shallow depths.
                    // Depth conditions are important for mate finding.
                    if (!rootNode && pos.NonPawnMaterial(us) != 0 && !bestValue.IsLoss())
                    {
                        // Skip quiet moves if movecount exceeds our threshold
                        if (moveCount >= (3 + depth * depth) / (2 - B(improving)))
                            mp.SkipQuietMoves();
                        // Reduced depth of the next LMR search
                        int lmrDepth = newDepth - r / 1024;
                        if (capture || givesCheck)
                        {
                            Piece capturedPiece = pos.PieceOn(move.ToSq());
                            int captHist = CaptureHistory.Value[movedPiece, move.ToSq(), PieceType.TypeOf(capturedPiece)];
                            // Futility pruning for captures
                            if (!givesCheck && lmrDepth < 8)
                            {
                                Value futilityValue = ss.staticEval + 234 + 247 * lmrDepth + Values.PieceValue[capturedPiece] + 134 * captHist / 1024;
                                if (futilityValue <= alpha)
                                    continue;
                            }

                            // SEE based pruning for captures and checks.
                            // Avoid pruning sacrifices of our last piece for stalemate.
                            int margin = 177 * depth + captHist * 34 / 1024;
                            if ((alpha >= VALUE_DRAW || pos.NonPawnMaterial(us) != Values.PieceValue[movedPiece]) && !pos.SeeGe(move, -margin))
                                continue;
                        }
                        else if (!ss.followPV || !PvNode)
                        {
                            int dIndex = Math.Min((int)depth, lmrDivisor.Length) - 1;
                            int history = contHist[0].Get()[movedPiece, move.ToSq()] + contHist[1].Get()[movedPiece, move.ToSq()] + SharedHistory.PawnEntry(pos)[movedPiece, move.ToSq()];
                            // Continuation history based pruning
                            if (history < -4136 * depth)
                                continue;
                            history += 69 * MainHistory.Value[us, move.Raw()] / 32;
                            // (*Scaler): Generally, lower divisors scale well
                            lmrDepth += history / lmrDivisor[dIndex];
                            Value futilityValue = ss.staticEval + 119 * lmrDepth + 90 * B(ss.staticEval > alpha) + 164;
                            // Futility pruning: parent node
                            // (*Scaler): Generally, more frequent futility pruning scales well
                            if (!ss.inCheck && lmrDepth < 12 && futilityValue <= alpha)
                            {
                                if (bestValue <= futilityValue && !bestValue.IsDecisive() && !futilityValue.IsWin())
                                    bestValue = futilityValue;
                                continue;
                            }

                            lmrDepth = Math.Max(lmrDepth, 0);
                            // Prune moves with negative SEE
                            if (!pos.SeeGe(move, -23 * lmrDepth * lmrDepth))
                                continue;
                        }
                    }

                    // Step 16. Singular Extensions
                    //
                    // We check for "only moves": if one move fails high on (alpha, beta) but all
                    // others fail low on (alpha-s, beta-s), then that move is singular. Singular
                    // moves are extended to better estimate the result of the (putatively)
                    // forced line. If it's non-singular, we may do some pruning or reduction.
                    //
                    // Recursive excluded search is avoided. The `excludedMove` mechanism
                    // was historically a hack and remains a bit fragile.
                    //
                    // (*Scaler) Generally, higher singularBeta (i.e closer to ttValue)
                    // and lower extension margins scale well.
                    if (!rootNode && move == ttData.move && excludedMove == Move.None() && depth >= 6 + B(ss.ttPv) && ttData.value.IsValid() && !ttData.value.IsDecisive() && (ttData.bound & BOUND_LOWER) != 0 && ttData.depth >= depth - 3 && !IsShuffling(move, frame, pos) && !seekMate)
                    {
                        Value singularBeta = ttData.value - (59 + 66 * B(ss.ttPv && !PvNode)) * depth / 63;
                        Depth singularDepth = newDepth / 2;
                        ss.excludedMove = move;
                        try
                        {
                            value = Search(pos, frame, singularBeta - 1, singularBeta, singularDepth, cutNode, NodeType.NonPV);
                        }
                        finally
                        {
                            ss.excludedMove = Move.None();
                        }

                        if (value < singularBeta)
                        {
                            int corrValAdj = Math.Abs(correctionValue) / 198368;
                            int doubleMargin = -2 + 204 * B(PvNode) - 152 * B(!ttCapture) - corrValAdj - 1175 * TTMoveHistory / 114178 - B(ss.ply > RootDepth) * 38;
                            int tripleMargin = 70 + 279 * B(PvNode) - 188 * B(!ttCapture) + 81 * B(ss.ttPv) - corrValAdj - B(ss.ply > RootDepth) * 43;
                            extension = 1 + B(value < singularBeta - doubleMargin) + B(value < singularBeta - tripleMargin);
                            depth++;
                        }
                        // Multi-cut pruning
                        // Our ttMove is assumed to fail high based on the bound of the TT entry,
                        // and if after excluding the ttMove with a reduced search we fail high
                        // over the original beta, we assume this expected cut-node is not
                        // singular (multiple moves fail high), and we can prune the whole
                        // subtree by returning a softbound.
                        else if (value >= beta && !value.IsDecisive())
                        {
                            TTMoveHistory.Update(-421 - 110 * depth);
                            if (!ss.inCheck && value > ss.staticEval)
                            {
                                int bonus = Clamp((int)(value - ss.staticEval) * singularDepth * 177 / 1024, -HistoryConstants.CORRECTION_HISTORY_LIMIT / 4, HistoryConstants.CORRECTION_HISTORY_LIMIT / 4);
                                UpdateCorrectionHistory(pos, frame, bonus);
                            }

                            return value;
                        }
                        // Negative extensions
                        // If other moves failed high over (ttValue - margin) without the
                        // ttMove on a reduced search, but we cannot do multi-cut because
                        // (ttValue - margin) is lower than the original beta, we do not know
                        // if the ttMove is singular or can do a multi-cut, so we reduce the
                        // ttMove in favor of other moves based on some conditions:
                        // If the ttMove is assumed to fail high over current beta or
                        // if we are on a cutNode
                        else if (ttData.value >= beta || cutNode)
                            extension = -3;
                    }

                    ulong nodeCount = rootNode ? (ulong)(Nodes) : 0;
                    // Step 17. Make the move
                    DoMove(pos, move, st, givesCheck, frame);
                    try
                    {
                        // Add extension to new depth
                        newDepth += extension;
                        // Step 18. Compute and apply late moves reductions/extensions (LMR)
                        // Decrease reduction for PvNodes (*Scaler)
                        if (ss.ttPv)
                            r -= 3023 + B(PvNode) * 1004 + B(ttData.value > alpha) * 885 + B(ttData.depth >= depth) * (816 + B(cutNode) * 940);
                        // Base reduction offset to compensate for other tweaks
                        r += 697;
                        r -= moveCount * 65;
                        r -= Math.Abs(correctionValue) / 26310;
                        // Increase reduction for cut nodes
                        if (cutNode)
                            r += 4026 + 933 * B(ttData.move == Move.None());
                        // Increase reduction if ttMove is a capture
                        if (ttCapture)
                            r += 1079;
                        // Increase reduction if next ply has a lot of fail high
                        if (Frames[frame + 1].cutoffCnt > 1)
                            r += 264 + 1095 * B(Frames[frame + 1].cutoffCnt > 2) + 1138 * B(allNode);
                        // For first picked move (ttMove) reduce reduction
                        else if (move == ttData.move)
                            r -= 2179;
                        if (capture)
                            ss.statScore = 873 * (int)Values.PieceValue[pos.CapturedPiece()] / 128 + CaptureHistory.Value[movedPiece, move.ToSq(), PieceType.TypeOf(pos.CapturedPiece())];
                        else
                            ss.statScore = (2252 * MainHistory.Value[us, move.Raw()] + 1126 * contHist[0].Get()[movedPiece, move.ToSq()] + 1093 * contHist[1].Get()[movedPiece, move.ToSq()]) / 1024;
                        // Decrease/increase reduction for moves with a good/bad history
                        r -= ss.statScore * 439 / 4096;
                        if (!capture && !alpha.IsDecisive())
                            r += 3 * Clamp(alpha - eval, -64, 96);
                        // Scale up reductions for expected ALL nodes
                        if (allNode)
                            r += r * 276 / (256 * depth + 268);
                        // Apply the computed LMR
                        if (depth >= 2 && moveCount > 1)
                        {
                            // In general we want to cap the LMR depth search at newDepth, but when
                            // reduction is negative, we allow this move a limited search extension
                            // beyond the first move depth.
                            // To prevent problems when the max value is less than the min value,
                            // Clamp has been replaced by a more robust implementation.
                            Depth d = Math.Max(1, Math.Min(newDepth - r / 1024, newDepth + 2)) + B(PvNode);
                            ss.reduction = newDepth - d;
                            try
                            {
                                value = -Search(pos, frame + 1, -(alpha + 1), -alpha, d, true, NodeType.NonPV);
                            }
                            finally
                            {
                                ss.reduction = 0;
                            }

                            // Do a full-depth search when reduced LMR search fails high
                            // (*Scaler) Shallower searches here don't scale well
                            if (value > alpha)
                            {
                                // Adjust full-depth search based on LMR results - if the result was
                                // good enough search deeper, if it was bad enough search shallower.
                                bool doDeeperSearch = d < newDepth && value > bestValue + 53;
                                bool doShallowerSearch = value < bestValue + 8;
                                newDepth += B(doDeeperSearch) - B(doShallowerSearch);
                                if (newDepth > d)
                                    value = -Search(pos, frame + 1, -(alpha + 1), -alpha, newDepth, !cutNode, NodeType.NonPV);
                                // Post LMR continuation history updates
                                UpdateContinuationHistories(frame, movedPiece, move.ToSq(), 1334);
                            }
                        }
                        // Step 19. Full-depth search when LMR is skipped
                        else if (!PvNode || moveCount > 1)
                        {
                            // Increase reduction if ttMove is not present
                            if (ttData.move == Move.None())
                                r += 1127;
                            // If expected reduction is high, we reduce search depth here
                            value = -Search(pos, frame + 1, -(alpha + 1), -alpha, newDepth - B(r > 5234) - B(r > 5487 && newDepth > 2), !cutNode, NodeType.NonPV);
                        }

                        // Step 20. For PV nodes only, do a full PV search on the first move
                        // or after a fail high, otherwise let the parent node fail low with
                        // value <= alpha and try another move.
                        if (PvNode && (moveCount == 1 || value > alpha))
                        {
                            Frames[frame + 1].pv = pv;
                            Frames[frame + 1].pv!.Get().Clear();
                            // Extend move from transposition table if we are about to dive
                            // into qsearch. Decisive score handling improves mate finding
                            // and retrograde analysis.
                            if (move == ttData.move && ((ttData.value.IsValid() && ttData.value.IsDecisive() && ttData.depth > 0) || ttData.depth > 1))
                                newDepth = Math.Max(newDepth, 1);
                            value = -Search(pos, frame + 1, -beta, -alpha, newDepth, false, NodeType.PV);
                        }
                    // Step 21. Undo move
                    }
                    finally
                    {
                        UndoMove(pos, move);
                    }

                    // Step 22. Check for a new best move
                    // If a stop occurred, the value of the search cannot be trusted,
                    // and we return immediately without updating the best move,
                    // principal variation or transposition table.
                    if (Control.IsStopped)
                        return VALUE_ZERO;
                    if (rootNode)
                    {
                        ref RootMove rm = ref RootMoves[RootMoves.IndexOf(move)];
                        rm.effort = unchecked(rm.effort + Nodes - nodeCount);
                        ulong N = unchecked(Nodes - nodeCount);
                        ulong E_prev = Math.Max((ulong)(1), unchecked(rm.effort - N));
                        // Exponential moving average parameters for root move
                        const ulong Scale = 32;
                        const ulong ChiNumerator = 3;
                        const ulong ChiDenominator = 2; // Chi = 3/2 = 1.5
                        const ulong MinWeight = 12; // 37.5% minimum weight
                        const ulong MaxWeight = 24; // 75% maximum weight
                        ulong w = Math.Min(Math.Max(unchecked((Scale * N * ChiDenominator) / (N * ChiDenominator + ChiNumerator * E_prev)), MinWeight), MaxWeight);
                        ulong w_mss = Math.Min(w, (ulong)(16));
                        long v2 = (long)(value) * Math.Abs(value);
                        if (rm.averageScore == -VALUE_INFINITE)
                            rm.averageScore = value;
                        else
                            rm.averageScore = unchecked((int)((unchecked((ulong)(int)value) * w + unchecked((ulong)(int)rm.averageScore) * (Scale - w)) / Scale));
                        if (rm.meanSquaredScore == -VALUE_INFINITE * VALUE_INFINITE)
                            rm.meanSquaredScore = value * Math.Abs(value);
                        else
                            rm.meanSquaredScore = unchecked((int)((unchecked((ulong)v2) * w_mss + unchecked((ulong)(int)rm.meanSquaredScore) * (Scale - w_mss)) / Scale));
                        // PV move or new best move?
                        if (moveCount == 1 || value > alpha)
                        {
                            rm.score = rm.uciScore = value;
                            rm.selDepth = SelDepth;
                            rm.UnsetInexact();
                            if (value >= beta)
                            {
                                rm.inexactLower = true;
                                rm.uciScore = beta;
                            }
                            else if (value <= alpha)
                            {
                                rm.inexactUpper = true;
                                rm.uciScore = alpha;
                            }

                            rm.pv.AssignContinuation(ref Frames[frame + 1].pv!.Get());
                            // We record how often the best move has been changed in each iteration.
                            // This information is used for time management. In MultiPV mode,
                            // we must take care to only do this for the first PV line.
                            if (moveCount > 1 && PvIndex == 0)
                                bestMoveChanges.PostIncrement();
                        }
                        else
                            // All other moves but the PV are set to the lowest value: this
                            // is not a problem when sorting because the sort is stable and the
                            // move position in the list is preserved -- just the PV is pushed up.
                            rm.score = -VALUE_INFINITE;
                    }

                    // If we have an alternative move equal in value to the current bestmove,
                    // we sometimes promote it to bestmove by pretending it just exceeds
                    // alpha (but not beta).
                    int inc = B(value == bestValue && ss.ply + 2 >= RootDepth && (unchecked((int)Nodes) & 14) == 0 && !((Value)(Math.Abs(value) + 1)).IsWin());
                    if (value + inc > bestValue)
                    {
                        bestValue = value;
                        if (value + inc > alpha)
                        {
                            bestMove = move;
                            // Update PV even in fail-high case
                            if (PvNode && !rootNode)
                                ss.pv!.Get().Update(move, Frames[frame + 1].pv?.Get());
                            if (value >= beta)
                            {
                                // (*Scaler) Infrequent and small updates scale well
                                ss.cutoffCnt += B((extension < 2) || PvNode);
                                break;
                            }

                            // Reduce other moves if we have found at least one score improvement
                            if (depth > 3 && depth < 12 && !value.IsDecisive())
                                depth -= 3;
                            alpha = value; // Update alpha! Always alpha < beta
                        }
                    }

                    // If the move is worse than some previously searched move,
                    // remember it, to update its stats later.
                    if (move != bestMove && moveCount <= SearchedList.Capacity)
                    {
                        if (capture)
                            capturesSearched.PushBack(move);
                        else
                            quietsSearched.PushBack(move);
                    }
                }

                // Step 23. Check for mate and stalemate, otherwise update bestmove/countermove stats
                // Adjust best value for fail high cases
                if (bestValue >= beta && !bestValue.IsDecisive() && !alpha.IsDecisive())
                    bestValue = (bestValue * depth + beta) / (depth + 1);
                // All legal moves have been searched: if there are no legal moves, it
                // must be a mate or a stalemate (just a fail low score if we are in a
                // singular extension search).
                if (moveCount == 0)
                    bestValue = excludedMove != Move.None() ? alpha : ss.inCheck ? MatedIn(ss.ply) : VALUE_DRAW;
                // If there is a move that produces search value greater than alpha,
                // we update the stats of searched moves.
                else if (bestMove != Move.None())
                {
                    UpdateAllStats(pos, frame, bestMove, prevSq, quietsSearched, capturesSearched, depth, ttData.move, PvNode);
                    if (!PvNode)
                        TTMoveHistory.Update((bestMove == ttData.move ? 918 : -747));
                }
                // Bonus for prior quiet countermove that caused the fail low
                else if (!priorCapture && prevSq != Square.SQ_NONE)
                {
                    int bonusScale = -241;
                    bonusScale -= Frames[frame - 1].statScore / 98;
                    bonusScale += Math.Min(59 * depth, 420);
                    bonusScale += 186 * B(Frames[frame - 1].moveCount > 9);
                    bonusScale += 142 * B(!ss.inCheck && bestValue <= ss.staticEval - 106);
                    bonusScale += 159 * B(!Frames[frame - 1].inCheck && bestValue <= -Frames[frame - 1].staticEval - 68);
                    bonusScale = Math.Max(bonusScale, 0);
                    // scaledBonus ranges from 0 to roughly 2.3M, overflows happen for
                    // multipliers larger than 900
                    int scaledBonus = Math.Min(150 * depth - 85, 1337) * bonusScale;
                    UpdateContinuationHistories(frame - 1, pos.PieceOn(prevSq), prevSq, scaledBonus * 263 / 16384);
                    ButterflyHistory.At(ref MainHistory.Value, ~us, Frames[frame - 1].currentMove.Raw()).Update(scaledBonus * 215 / 32768);
                    if (PieceType.TypeOf(pos.PieceOn(prevSq)) != PieceType.PAWN && (Frames[frame - 1].currentMove).TypeOf() != MoveType.PROMOTION)
                        PawnHistoryEntry.At(ref SharedHistory.PawnEntry(pos), pos.PieceOn(prevSq), prevSq).Update(scaledBonus * 324 / 8192);
                }
                // Bonus for prior capture countermove that caused the fail low
                else if (priorCapture && prevSq != Square.SQ_NONE)
                {
                    Piece capturedPiece = pos.CapturedPiece();
                    CapturePieceToHistory.At(ref CaptureHistory.Value, pos.PieceOn(prevSq), prevSq, PieceType.TypeOf(capturedPiece)).Update(892);
                }

                if (PvNode)
                    bestValue = Math.Min(bestValue, maxValue);
                // If no good move is found and the previous position was ttPv, then the previous
                // opponent move is probably good and the new position is added to the search tree.
                if (bestValue <= alpha)
                    ss.ttPv = ss.ttPv || Frames[frame - 1].ttPv;
                // Step 24. Write gathered information in transposition table. Note that the
                // static evaluation is saved as it was before correction history.
                if (excludedMove == Move.None() && !(rootNode && PvIndex != 0))
                    ttWriter.Write(posKey, TranspositionValues.ValueToTT(bestValue, ss.ply), ss.ttPv, bestValue >= beta ? BOUND_LOWER : PvNode && bestMove != Move.None() ? BOUND_EXACT : BOUND_UPPER, moveCount != 0 ? (int)depth : Math.Min(MAX_PLY - 1, depth + 6), bestMove, unadjustedStaticEval, TT.Generation);
                // Adjust correction history if the best move is not a capture and
                // the error direction matches whether we are above/below bounds.
                if (!ss.inCheck && !(bestMove != Move.None() && pos.Capture(bestMove)) && (bestValue > ss.staticEval) == (bestMove != Move.None()))
                {
                    int bonus = Clamp((int)(bestValue - ss.staticEval) * depth * (bestMove != Move.None() ? 12 : 18) / 128, -HistoryConstants.CORRECTION_HISTORY_LIMIT / 4, HistoryConstants.CORRECTION_HISTORY_LIMIT / 4);
                    UpdateCorrectionHistory(pos, frame, 1061 * bonus / 1024);
                }

                // The search is now complete
                return bestValue;
            }
        }
    }
}
