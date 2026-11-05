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
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>SF19-000226: stateful scalar pseudo-legal move ordering. Assignment aliases this noncopyable owner.</summary>
    /// <remarks>The position must be restored between calls. Local histories and cursors belong to one worker;
    /// shared histories retain their per-cell atomic loads. No concurrent use of a picker is supported.</remarks>
    public sealed class MovePicker
    {
        private Position pos = null!;
        private ButterflyHistory.Owner? mainHistory;
        private LowPlyHistory.Owner? lowPlyHistory;
        private CapturePieceToHistory.Owner? captureHistory;
        private PieceToHistoryReference[]? continuationHistory;
        private SharedHistories? sharedHistory;
        private Move ttMove;
        private Depth depth;
        private int ply, threshold;
        private readonly ExtMove[] moves;
        private readonly Move[] generatedMoves;
        private int cur, endCur, endBadCaptures, endCaptures, endGenerated;
        private Stages stage;
        private bool skipQuiets;

        // Private search instances must be Reset before reuse. Do not retain a caller's Position.
        internal void Detach()
        {
            pos = null!;
            mainHistory = null; lowPlyHistory = null; captureHistory = null;
            continuationHistory = null; sharedHistory = null;
        }

        /// <summary>SF19-000219: main search (depth > 0), quiescence, or evasions when in check.</summary>
        /// <remarks>Nullable history pointers may be omitted only on paths that never read them.
        /// Continuation slots 0,1,2,3,5 are read for quiet scoring; evasions read slot 0 only.
        /// The array itself remains live, as does the native pointer array. Quiet scoring requires a nonnegative ply.</remarks>
        public MovePicker(Position position, Move ttm, Depth d, ButterflyHistory.Owner? mh,
            LowPlyHistory.Owner? lph, CapturePieceToHistory.Owner? cph,
            PieceToHistoryReference[]? ch, SharedHistories? sh, int pl)
            : this(position, ttm, d, mh, lph, cph, ch, sh, pl, new ExtMove[ChessConstants.MAX_MOVES], new Move[ChessConstants.MAX_MOVES]) { }
        internal MovePicker(ExtMove[] scored, Move[] generated) { moves = scored; generatedMoves = generated; }
        internal MovePicker(Position position, Move ttm, Depth d, ButterflyHistory.Owner? mh,
            LowPlyHistory.Owner? lph, CapturePieceToHistory.Owner? cph,
            PieceToHistoryReference[]? ch, SharedHistories? sh, int pl, ExtMove[] scored, Move[] generated)
            : this(scored, generated) => Reset(position, ttm, d, mh, lph, cph, ch, sh, pl);

        internal void Reset(Position position, Move ttm, Depth d, ButterflyHistory.Owner? mh,
            LowPlyHistory.Owner? lph, CapturePieceToHistory.Owner? cph,
            PieceToHistoryReference[]? ch, SharedHistories? sh, int pl)
        {
            pos = position ?? throw new ArgumentNullException(nameof(position));
            mainHistory = mh; lowPlyHistory = lph; captureHistory = cph;
            continuationHistory = ch; sharedHistory = sh; ttMove = ttm; depth = d; ply = pl; threshold = 0;
            cur = endCur = endBadCaptures = endCaptures = endGenerated = 0; skipQuiets = false;
            stage = pos.Checkers() != 0 ? Stages.EVASION_TT : depth > 0 ? Stages.MAIN_TT : Stages.QSEARCH_TT;
            if (!((bool)ttm && pos.PseudoLegal(ttm, generatedMoves))) ++stage;
        }

        /// <summary>SF19-000220: ProbCut, outside check. Only generated captures are filtered by SEE.</summary>
        public MovePicker(Position position, Move ttm, int th, CapturePieceToHistory.Owner? cph)
            : this(new ExtMove[ChessConstants.MAX_MOVES], new Move[ChessConstants.MAX_MOVES]) => Reset(position, ttm, th, cph);
        internal void Reset(Position position, Move ttm, int th, CapturePieceToHistory.Owner? cph)
        {
            pos = position ?? throw new ArgumentNullException(nameof(position));
            if (pos.Checkers() != 0) throw new ArgumentException("ProbCut requires a position outside check.", nameof(position));
            mainHistory = null; lowPlyHistory = null; continuationHistory = null; sharedHistory = null;
            captureHistory = cph; ttMove = ttm; threshold = th; depth = default; ply = 0;
            cur = endCur = endBadCaptures = endCaptures = endGenerated = 0; skipQuiets = false;
            stage = Stages.PROBCUT_TT;
            if (!((bool)ttm && pos.CaptureStage(ttm) && pos.PseudoLegal(ttm, generatedMoves))) ++stage;
        }

        // SF19-000221: the three template instantiations share the original loop and expression order.
        private int Score(MoveList list, GenType type)
        {
            for (int i = 0; i < list.Count; ++i) generatedMoves[i] = list[i];
            return ScoreBuffer(list.Count, type);
        }
        private int ScoreGenerated(GenType type)
        {
            int count = MoveGenerator.Generate(pos, generatedMoves, 0, type);
            return ScoreBuffer(count, type);
        }
        private int ScoreBuffer(int count, GenType type)
        {
            Color us = pos.SideToMove();
            Span<Bitboard> threatByLesser = stackalloc Bitboard[7];
            if (type == GenType.QUIETS)
            {
                threatByLesser[PieceType.PAWN] = 0;
                threatByLesser[PieceType.KNIGHT] = threatByLesser[PieceType.BISHOP] = pos.AttacksBy(PieceType.PAWN, ~us);
                threatByLesser[PieceType.ROOK] = pos.AttacksBy(PieceType.KNIGHT, ~us)
                    | pos.AttacksBy(PieceType.BISHOP, ~us) | threatByLesser[PieceType.KNIGHT];
                threatByLesser[PieceType.QUEEN] = pos.AttacksBy(PieceType.ROOK, ~us) | threatByLesser[PieceType.ROOK];
                threatByLesser[PieceType.KING] = 0;
            }
            int it = cur;
            for (int index = 0; index < count; ++index)
            {
                Move move = generatedMoves[index];
                ref ExtMove m = ref moves[it++];
                m.Assign(move);
                Square from = m.FromSq(), to = m.ToSq();
                Piece pc = pos.MovedPiece(m), capturedPiece = pos.PieceOn(to);
                PieceType pt = PieceType.TypeOf(pc);
                if (type == GenType.CAPTURES)
                    m.value = captureHistory!.Value[pc, to, PieceType.TypeOf(capturedPiece)]
                        + 7 * (int)Values.PieceValue[capturedPiece];
                else if (type == GenType.QUIETS)
                {
                    m.value = 2 * mainHistory!.Value[us, m.Raw()];
                    m.value += 2 * sharedHistory!.ReadPawnEntry(pos)[pc, to];
                    m.value += continuationHistory![0].Get()[pc, to];
                    m.value += continuationHistory[1].Get()[pc, to];
                    m.value += continuationHistory[2].Get()[pc, to];
                    m.value += continuationHistory[3].Get()[pc, to];
                    m.value += continuationHistory[5].Get()[pc, to];
                    m.value += ((pos.CheckSquares(pt) & to) != 0 && pos.SeeGe(m, -75)) ? 16384 : 0;
                    int v = 20 * (((threatByLesser[pt] & from) != 0 ? 1 : 0)
                        - ((threatByLesser[pt] & to) != 0 ? 1 : 0));
                    m.value += (int)Values.PieceValue[pt] * v;
                    if (ply < HistoryConstants.LOW_PLY_HISTORY_SIZE)
                        m.value += 8 * lowPlyHistory!.Value[ply, m.Raw()] / (1 + ply);
                }
                else // EVASIONS
                    m.value = pos.CaptureStage(m) ? (int)Values.PieceValue[capturedPiece] + (1 << 28)
                        : mainHistory!.Value[us, m.Raw()] + continuationHistory![0].Get()[pc, to];
            }
            return it;
        }

        // Fixed predicates replace C++ lambdas without allocating delegates on every NextMove call.
        private enum Selection { All, GoodCapture, GoodQuiet, BadQuiet, ProbCut }
        // SF19-000222: suppress TT before the predicate, advance once for rejection and once on emission.
        private Move Select(Selection filter)
        {
            for (; cur < endCur; ++cur)
                if (moves[cur] != ttMove && Accept(filter)) return moves[cur++];
            return Move.None();
        }
        private bool Accept(Selection filter)
        {
            switch (filter)
            {
                case Selection.GoodCapture:
                    if (pos.SeeGe(moves[cur], -moves[cur].value / 18)) return true;
                    ExtMove tmp = moves[endBadCaptures];
                    moves[endBadCaptures++] = moves[cur]; moves[cur] = tmp;
                    return false;
                case Selection.GoodQuiet: return moves[cur].value > -14000;
                case Selection.BadQuiet: return moves[cur].value <= -14000;
                case Selection.ProbCut: return pos.SeeGe(moves[cur], threshold);
                default: return true;
            }
        }

        /// <summary>SF19-000223: next pseudo-legal move, or none on exhaustion. Never mutates the position.</summary>
        public Move NextMove()
        {
            while (true)
                switch (stage)
                {
                    case Stages.MAIN_TT:
                    case Stages.EVASION_TT:
                    case Stages.QSEARCH_TT:
                    case Stages.PROBCUT_TT:
                        ++stage; return ttMove;
                    case Stages.CAPTURE_INIT:
                    case Stages.PROBCUT_INIT:
                    case Stages.QCAPTURE_INIT:
                        cur = endBadCaptures = 0;
                        endCur = endCaptures = ScoreGenerated(GenType.CAPTURES);
                        MoveOrdering.PartialInsertionSort(moves.AsSpan(cur, endCur - cur), int.MinValue);
                        ++stage; continue;
                    case Stages.GOOD_CAPTURE:
                        if (Select(Selection.GoodCapture)) return moves[cur - 1];
                        ++stage; continue;
                    case Stages.QUIET_INIT:
                        if (!skipQuiets)
                        {
                            endCur = endGenerated = ScoreGenerated(GenType.QUIETS);
                            MoveOrdering.PartialInsertionSort(moves.AsSpan(cur, endCur - cur), checked(-3560 * (int)depth));
                        }
                        ++stage; continue;
                    case Stages.GOOD_QUIET:
                        if (!skipQuiets && (bool)Select(Selection.GoodQuiet)) return moves[cur - 1];
                        cur = 0; endCur = endBadCaptures;
                        ++stage; continue;
                    case Stages.BAD_CAPTURE:
                        if (Select(Selection.All)) return moves[cur - 1];
                        cur = endCaptures; endCur = endGenerated;
                        ++stage; continue;
                    case Stages.BAD_QUIET:
                        return !skipQuiets ? Select(Selection.BadQuiet) : Move.None();
                    case Stages.EVASION_INIT:
                        cur = 0;
                        endCur = endGenerated = ScoreGenerated(GenType.EVASIONS);
                        MoveOrdering.PartialInsertionSort(moves.AsSpan(cur, endCur - cur), int.MinValue);
                        ++stage; continue;
                    case Stages.EVASION:
                    case Stages.QCAPTURE: return Select(Selection.All);
                    case Stages.PROBCUT: return Select(Selection.ProbCut);
                    default: throw new InvalidOperationException("Invalid MovePicker stage.");
                }
        }

        /// <summary>SF19-000224: suppress future quiet stages, including any remaining sorted quiet moves.</summary>
        public void SkipQuietMoves() => skipQuiets = true;
    }
}
