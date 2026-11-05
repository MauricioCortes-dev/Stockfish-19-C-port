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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        private static double interpolate(double x, double x0, double x1, double y0, double y1) => y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        private static double Clamp(double x, double min, double max) => Math.Min(max, Math.Max(min, x));
        private bool IterativeDeepening()
        {
            SearchManager? mainThread = IsMainThread ? Manager : null;
            RootPVMoves lastBestMovePV = default;
            Depth lastBestMoveDepth = 0;
            Value lastBestMoveScore = -Value.VALUE_INFINITE;
            Value alpha, beta, bestValue = -Value.VALUE_INFINITE;
            Color us = RootPosition.SideToMove();
            double timeReduction = 1, totBestMoveChanges = 0;
            int delta, iterIdx = 0;
            const int ss = 7;
            if (mainThread != null)
            {
                if (mainThread.BestPreviousScore == Value.VALUE_INFINITE)
                    Array.Fill(mainThread.IterValue, Value.VALUE_ZERO);
                else
                    Array.Fill(mainThread.IterValue, mainThread.BestPreviousScore);
            }

            int multiPV = Options["MultiPV"].AsInt();
            var skill = new Skill(Options["Skill Level"].AsInt(), Options["UCI_LimitStrength"].AsInt() != 0 ? Options["UCI_Elo"].AsInt() : 0, Manager.RandomSource);
            // When playing with strength handicap enable MultiPV search that we will
            // use behind-the-scenes to retrieve a set of possible moves.
            if (skill.Enabled())
                multiPV = Math.Max(multiPV, 4);
            multiPV = Math.Min(multiPV, RootMoves.Count);
            int searchAgainCounter = 0;
            bool uciPvSent = false;
            short low = 102;
            LowPlyHistory.Value.Fill(in low);
            for (int c = 0; c < 2; ++c)
            {
                var cells = QuietHistoryRow.Cells(ref ButterflyHistory.At(ref MainHistory.Value, c));
                for (int i = 0; i < cells.Length; ++i)
                {
                    ref var cell = ref cells[i];
                    cell.Assign((short)(cell.Load() * 729 / 1024));
                }
            }

            // Iterative deepening loop until requested to stop or the target depth is reached
            while (RootDepth + 1 < ChessConstants.MAX_PLY && !Control.IsStopped && !(Limits.Depth != 0 && mainThread != null && RootDepth >= Limits.Depth))
            {
                RootDepth++;
                // Age out PV variability metric and signal the start of a new iteration
                if (mainThread != null)
                {
                    totBestMoveChanges /= 2;
                    uciPvSent = false;
                }

                // Save the last iteration's scores before the first PV line is searched and
                // all the move scores except the (new) PV are set to -Value.VALUE_INFINITE.
                for (int i = 0; i < RootMoves.Count; ++i)
                {
                    RootMoves[i].previousScore = RootMoves[i].score;
                    RootMoves[i].previousPV = RootMoves[i].pv;
                    RootMoves[i].previousScoreExact = i < multiPV;
                }

                int pvFirst = PvLast = 0;
                if (!Control.IncreaseDepth)
                    searchAgainCounter++;
                // MultiPV loop: we perform a full root search for each PV line
                for (PvIndex = 0; PvIndex < multiPV; ++PvIndex)
                {
                    if (PvIndex == PvLast)
                    {
                        pvFirst = PvLast;
                        for (PvLast++; PvLast < RootMoves.Count; PvLast++)
                            if (RootMoves[PvLast].tbRank != RootMoves[pvFirst].tbRank)
                                break;
                    }

                    LastIterationPV = RootMoves[PvIndex].previousPV;
                    // Reset UCI info SelDepth for each depth and each PV line
                    SelDepth = 0;
                    // Reset aspiration window starting size
                    delta = 5 + ThreadIndex % 8 + Math.Abs((int)RootMoves[PvIndex].meanSquaredScore) / 10193;
                    Value avg = RootMoves[PvIndex].averageScore;
                    alpha = Math.Max(avg - delta, -Value.VALUE_INFINITE);
                    beta = Math.Min(avg + delta, Value.VALUE_INFINITE);
                    // Adjust Optimism based on root move's averageScore
                    Optimism[us] = 114 * avg / (Math.Abs((int)avg) + 85);
                    Optimism[~us] = -Optimism[us];
                    // Start with a small aspiration window and, in the case of a fail
                    // high/low, enlarge the window progressively.
                    int failedHighCnt = 0;
                    while (true)
                    {
                        // Adjust the effective depth searched, but ensure at least one
                        // effective increment for every four searchAgain steps (see issue #2717).
                        Depth adjustedDepth = Math.Max(1, RootDepth - failedHighCnt - 3 * (searchAgainCounter + 1) / 4);
                        RootDelta = beta - alpha;
                        bestValue = Search(RootPosition, ss, alpha, beta, adjustedDepth, false, NodeType.Root);
                        // Bring the best move to the front. It is critical that sorting
                        // is done with a stable algorithm because all the values but the
                        // first and eventually the new best one is set to -Value.VALUE_INFINITE
                        // and we want to keep the same order for all the moves except the
                        // new PV that goes to the front. Note that in the case of MultiPV
                        // search the already searched PV lines are preserved.
                        RootMoves.StableSort(PvIndex, PvLast);
                        // If search has been stopped, we break immediately. Sorting is
                        // safe because RootMoves is still valid, although it refers to
                        // the previous iteration.
                        if (Control.IsStopped)
                            break;
                        // When failing high/low give some update before a re-search. To avoid
                        // excessive output that could hang GUIs like Fritz 19, only start
                        // at Nodes > 10M (rather than depth N, which can be reached quickly).
                        if (mainThread != null && multiPV == 1 && (bestValue <= alpha || bestValue >= beta) && Nodes > NodesLimitOutput)
                            Manager.OutputPv(this, RootDepth);
                        // In case of failing low/high increase aspiration window and re-search,
                        // otherwise exit the loop.
                        if (bestValue <= alpha)
                        {
                            beta = alpha;
                            alpha = Math.Max(bestValue - delta, -Value.VALUE_INFINITE);
                            failedHighCnt = 0;
                            if (mainThread != null)
                                mainThread.StopOnPonderHit = false;
                        }
                        else if (bestValue >= beta)
                        {
                            alpha = Math.Max(beta - delta, alpha);
                            beta = Math.Min(bestValue + delta, Value.VALUE_INFINITE);
                            ++failedHighCnt;
                        }
                        else
                            break;
                        delta += 47 * delta / 128;
                        System.Diagnostics.Debug.Assert(alpha >= -Value.VALUE_INFINITE && beta <= Value.VALUE_INFINITE);
                    }

                    if (Control.IsStopped && PvIndex != 0)
                    {
                        // In multiPV analysis we do not let aborted searches spoil
                        // mated-in/TB loss scores from a completed search in an earlier
                        // PV line. Hence we guard against an aborted PvIndex line overtaking
                        // PvIndex - 1 when PvIndex - 1 is a proven loss. Moreover, we do not
                        // trust an exact loss score from an aborted search.
                        if ((RootMoves[PvIndex - 1].score.IsLoss() && RootMoves[PvIndex] < RootMoves[PvIndex - 1]) || RootMoves[PvIndex].IsExactLoss())
                        {
                            // If previousScore is exact and worse than PvIndex - 1, we
                            // can safely use it. If it is equal, we make sure it cannot
                            // overtake PvIndex - 1.
                            if (RootMoves[PvIndex].previousScore != -Value.VALUE_INFINITE && RootMoves[PvIndex].previousScoreExact && RootMoves[PvIndex].previousScore <= RootMoves[PvIndex - 1].score)
                            {
                                RootMoves[PvIndex].score = RootMoves[PvIndex].uciScore = RootMoves[PvIndex].previousScore;
                                RootMoves[PvIndex].previousScore = -Value.VALUE_INFINITE;
                                RootMoves[PvIndex].pv = RootMoves[PvIndex].previousPV;
                                RootMoves[PvIndex].UnsetInexact();
                            }
                            // Otherwise, if we can, we cap the score to the best possible, and mark
                            // the score as inexact (also a valid excuse for the incomplete PV).
                            else
                            {
                                if (RootMoves[PvIndex - 1].score.IsLoss())
                                {
                                    RootMoves[PvIndex].score = RootMoves[PvIndex].uciScore = RootMoves[PvIndex - 1].score;
                                    RootMoves[PvIndex].previousScore = -Value.VALUE_INFINITE;
                                    RootMoves[PvIndex].pv.Resize(1);
                                    RootMoves[PvIndex].inexactUpper = true;
                                }
                                else
                                    RootMoves[PvIndex].inexactUpper = false;
                                RootMoves[PvIndex].inexactLower = !RootMoves[PvIndex].inexactUpper;
                            }
                        }

                        // Finally, we mark all loss scores from partially searched moves as inexact.
                        for (int i = PvIndex + 1; i < multiPV; ++i)
                            if (RootMoves[i].IsExactLoss())
                                RootMoves[i].inexactLower = true;
                    }

                    // Sort the PV lines searched so far and update the GUI
                    RootMoves.StableSort(pvFirst, PvIndex + 1);
                    if (mainThread != null && !Control.IsStopped && (PvIndex + 1 == multiPV || Nodes > NodesLimitOutput))
                    {
                        Manager.OutputPv(this, RootDepth);
                        uciPvSent = (PvIndex + 1 == multiPV);
                    }

                    if (Control.IsStopped)
                        break;
                }

                bool forgottenMate = lastBestMoveScore != -Value.VALUE_INFINITE && lastBestMoveScore.IsMateOrMated() && (Math.Abs((int)RootMoves[0].score) < Math.Abs((int)lastBestMoveScore) || RootMoves[0].IsInexact());
                if (!Control.IsStopped)
                {
                    if (lastBestMovePV.Count == 0 || lastBestMovePV[0] != RootMoves[0].pv[0])
                        lastBestMoveDepth = RootDepth;
                    // Do not replace (shorter) mate scores from a previous iteration
                    if (!forgottenMate)
                    {
                        lastBestMovePV = RootMoves[0].pv;
                        lastBestMoveScore = RootMoves[0].score;
                    }
                }

                bool abortedLossSearch = Control.IsStopped && PvIndex == 0 && RootMoves[0].IsExactLoss();
                // An exact mated-in/TB-loss score from an aborted search cannot be
                // trusted: the loss could be delayed or refuted upon exploring the
                // remaining root-moves. Thus here we roll back to the score from the
                // previous iteration. We do the same if a search has failed to recover
                // a mate score that was found in a previous iteration.
                if (abortedLossSearch || (RootMoves[0].score != -Value.VALUE_INFINITE && forgottenMate))
                {
                    // Bring the last best move to the front for best thread selection
                    if (lastBestMovePV.Count != 0)
                    {
                        Move lastMove = lastBestMovePV[0];
                        RootMoves.MoveToFront(rm => rm == lastMove);
                        RootMoves[0].score = RootMoves[0].uciScore = lastBestMoveScore;
                        RootMoves[0].pv = lastBestMovePV;
                        RootMoves[0].UnsetInexact();
                        if (mainThread != null)
                            uciPvSent = false;
                    }
                    // For an aborted d1 search we label the loss score as inexact
                    else if (abortedLossSearch)
                        RootMoves[0].inexactLower = true;
                }

                // Have we found a "mate in x" after a completed iteration?
                if (Limits.Mate != 0 && !Control.IsStopped && RootMoves[0].score.IsMateOrMated() && Value.VALUE_MATE - Math.Abs((int)RootMoves[0].score) <= 2 * Limits.Mate)
                    Control.RequestStop();
                if (mainThread == null)
                    continue;
                // If the skill level is enabled and time is up, pick a sub-optimal best move
                if (skill.Enabled() && skill.TimeToPick(RootDepth))
                    skill.PickBest(RootMoves, multiPV);
                // Use part of the gained time from a previous stable move for the current move
                totBestMoveChanges += Pool?.TakeBestMoveChanges() ?? TakeBestMoveChanges();
                // Do we have time for the next iteration? Can we stop searching now?
                if (Limits.UseTimeManagement() && !Control.IsStopped && !mainThread.StopOnPonderHit)
                {
                    ulong nodesEffort = unchecked(RootMoves[0].effort * 100000) / Math.Max(1UL, Nodes);
                    double fallingEval = (11.48 + 2.30 * (mainThread.BestPreviousAverageScore - bestValue) + 1.1 * (mainThread.IterValue[iterIdx] - bestValue)) / 100.0;
                    fallingEval = Clamp(fallingEval, 0.576, 1.728);
                    // If the bestMove is stable over several iterations, reduce time accordingly
                    timeReduction = Clamp(interpolate((double)(RootDepth - lastBestMoveDepth), 4.96, 18.79, 0.639, 1.712), 0.629, 1.544);
                    double reduction = (1.468 + mainThread.PreviousTimeReduction) / (2.284 * timeReduction);
                    double bestMoveInstability = 1.077 + 2.229 * totBestMoveChanges / (Pool?.Count ?? 1);
                    double highBestMoveEffort = Clamp(interpolate(unchecked((long)nodesEffort), 75800L, 104510L, 0.969, 0.714), 0.693, 0.838);
                    double totalTime = (long)mainThread.Time.Optimum() * fallingEval * reduction * bestMoveInstability * highBestMoveEffort;
                    if (RootMoves.Count == 1)
                        // Cap used time to 0.5s for a better viewer experience
                        totalTime = Math.Min(500.0, totalTime);
                    TimePoint elapsedTime = Elapsed();
                    // Stop the search if we have exceeded totalTime or maximum time,
                    // or if we know that there are no better moves in the analysed line(s).
                    if (elapsedTime > Math.Min(totalTime, (double)(long)mainThread.Time.Maximum()) || RootMoves[multiPV - 1].score >= Value.MateIn(3) || RootMoves[0].score == Value.MatedIn(2))
                    {
                        // If we are allowed to Ponder do not stop the search now but
                        // keep pondering until the GUI sends "ponderhit" or "stop".
                        if (mainThread.Ponder)
                            mainThread.StopOnPonderHit = true;
                        else
                            Control.RequestStop();
                    }
                    else
                        Control.IncreaseDepth = mainThread.Ponder || elapsedTime <= totalTime * 0.50;
                }

                mainThread.IterValue[iterIdx] = bestValue;
                iterIdx = (iterIdx + 1) & 3;
            }

            if (mainThread == null)
                return false;
            mainThread.PreviousTimeReduction = timeReduction;
            // If the skill level is enabled, swap the best PV line with the sub-optimal one
            if (skill.Enabled())
            {
                Move selected = skill.Best != Move.None() ? skill.Best : skill.PickBest(RootMoves, multiPV);
                RootMoves.SwapElements(0, RootMoves.IndexOf(selected));
            }

            return uciPvSent;
        }
    }
}
