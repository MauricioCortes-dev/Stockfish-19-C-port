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
using Stockfish_sf_19_CSharp_Library.Board;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public sealed partial class Worker
    {
        private static int B(bool value) => value ? 1 : 0;
        private static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
        private static readonly ConthistBonus[] continuationBonuses =
        {
            new ConthistBonus(1, 520),
            new ConthistBonus(2, 390),
            new ConthistBonus(3, 145),
            new ConthistBonus(4, 251),
            new ConthistBonus(5, 66),
            new ConthistBonus(6, 209)
        };
        private static readonly int[] consistencyMultipliers =
        {
            94,
            103,
            110,
            106,
            119,
            126,
            121
        };
        public int Reduction(bool improving, Depth depth, int moveCount, int delta)
        {
            int scale = checked(Reductions[depth] * Reductions[moveCount]);
            return checked(scale - delta * 577 / RootDelta + B(!improving) * scale * 197 / 512 + 982);
        }

        public bool IsShuffling(Move move, int frame, Position pos)
        {
            if (pos.CaptureStage(move) || pos.Rule50Count() < 10)
                return false;
            if (pos.State()!.pliesFromNull < 6 || Frames[frame].ply < 20)
                return false;
            return move.FromSq() == Frames[frame - 2].currentMove.ToSq() && Frames[frame - 2].currentMove.FromSq() == Frames[frame - 4].currentMove.ToSq();
        }

        public void UpdateContinuationHistories(int frame, Piece pc, Square to, int bonus)
        {
            int positive = 0;
            foreach (var item in continuationBonuses)
            {
                int i = item.index;
                if (Frames[frame].inCheck && i > 2)
                    break;
                if (Frames[frame - i].currentMove.IsOk())
                {
                    ref var cell = ref PieceToHistory.At(ref Frames[frame - i].continuationHistory!.Get(), pc, to);
                    if (cell.Load() > 0)
                        ++positive;
                    cell.Update(checked(bonus * item.weight * consistencyMultipliers[positive]) / 65536 + 73 * B(i < 2));
                }
            }
        }

        public void UpdateQuietHistories(Position pos, int frame, Move move, int bonus)
        {
            ButterflyHistory.At(ref MainHistory.Value, pos.SideToMove(), move.Raw()).Update(bonus);
            if (Frames[frame].ply < HistoryConstants.LOW_PLY_HISTORY_SIZE)
                global::Stockfish_sf_19_CSharp_Library.Search.LowPlyHistory.At(ref LowPlyHistory.Value, Frames[frame].ply, move.Raw()).Update(checked(bonus * 712) / 1024);
            UpdateContinuationHistories(frame, pos.MovedPiece(move), move.ToSq(), checked(bonus * 750) / 1024);
            PawnHistoryEntry.At(ref SharedHistory.PawnEntry(pos), pos.MovedPiece(move), move.ToSq()).Update(checked(bonus * (bonus > -4 ? 1104 : 459)) / 1024);
        }

        public void UpdateAllStats(Position pos, int frame, Move bestMove, Square prevSq, in SearchedList quiets, in SearchedList captures, Depth depth, Move ttMove, bool pvNode)
        {
            Piece movedPiece = pos.MovedPiece(bestMove);
            int bonus = checked(Math.Min(133 * depth - 81, 1487) + 364 * B(bestMove == ttMove) + Frames[frame - 1].statScore / 28);
            int malus = Math.Min(968 * depth - 235, 2244);
            // Native signed/unsigned conversion happens before multiplication, including negative bonus.
            if (!pvNode)
                bonus = unchecked(bonus + (int)(unchecked((ulong)bonus * (ulong)(quiets.Count + captures.Count)) / 256));
            if (!pos.CaptureStage(bestMove))
            {
                UpdateQuietHistories(pos, frame, bestMove, checked(bonus * 899) / 1024);
                int actualMalus = malus * 1159 / 1024;
                foreach (Move move in quiets)
                {
                    actualMalus = actualMalus * 921 / 1024;
                    UpdateQuietHistories(pos, frame, move, -actualMalus);
                }
            }
            else
                CapturePieceToHistory.At(ref CaptureHistory.Value, movedPiece, bestMove.ToSq(), PieceType.TypeOf(pos.PieceOn(bestMove.ToSq()))).Update(checked(bonus * 1427) / 1024);
            if (prevSq != Square.SQ_NONE && Frames[frame - 1].moveCount == 1 + B(Frames[frame - 1].ttHit) && pos.CapturedPiece() == Piece.NO_PIECE)
                UpdateContinuationHistories(frame - 1, pos.PieceOn(prevSq), prevSq, -malus * 713 / 1024);
            foreach (Move move in captures)
                CapturePieceToHistory.At(ref CaptureHistory.Value, pos.MovedPiece(move), move.ToSq(), PieceType.TypeOf(pos.PieceOn(move.ToSq()))).Update(-malus * 1489 / 1024);
        }
    }
}
