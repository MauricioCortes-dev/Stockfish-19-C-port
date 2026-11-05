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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Full managed owner; the legacy PawnHistory-injected constructor retains its pawn-only contract.</summary>
    public sealed partial class SharedHistories
    {
        private readonly UnifiedCorrectionHistory? correctionHistory;
        private readonly ContinuationHistoryBlock? continuationHistoryBlock;
        private readonly ulong sizeMinus1;
        private static UIntPtr Threads(int count)
        {
            if (count <= 0 || (count & (count - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            return new UIntPtr((uint)count);
        }

        public SharedHistories(int threadCount) : this(Threads(threadCount))
        {
        }

        private static UIntPtr Threads(UIntPtr count)
        {
            ulong n = count.ToUInt64();
            if (n == 0 || (n & (n - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            return count;
        }

        public SharedHistories(UIntPtr threadCount) : this(new PawnHistory(Threads(threadCount)))
        {
            correctionHistory = new UnifiedCorrectionHistory(Threads(threadCount));
            continuationHistoryBlock = new ContinuationHistoryBlock();
            sizeMinus1 = correctionHistory.GetSize().ToUInt64() - 1;
        }

        public UnifiedCorrectionHistory CorrectionHistory => correctionHistory ?? throw new InvalidOperationException("Pawn-only owner has no correction bank.");
        public ContinuationHistoryBlock ContinuationHistory => continuationHistoryBlock ?? throw new InvalidOperationException("Pawn-only owner has no continuation bank.");

        public UIntPtr GetSize()
        {
            _ = CorrectionHistory;
            return new UIntPtr(sizeMinus1 + 1);
        }

        private UIntPtr Index(Key key) => new UIntPtr((ulong)key & sizeMinus1);
        public ref CorrectionHistoryEntry PawnCorrectionEntry(Position pos) => ref CorrectionHistory[Index(pos.PawnKey())];
        public ref CorrectionHistoryEntry MinorPieceCorrectionEntry(Position pos) => ref CorrectionHistory[Index(pos.MinorPieceKey())];
        public ref CorrectionHistoryEntry NonPawnCorrectionEntry(Position pos, Color color) => ref CorrectionHistory[Index(pos.NonPawnKey(color))];
        public UnifiedCorrectionHistory.ReadOnlyEntry ReadPawnCorrectionEntry(Position pos) => CorrectionHistory.ReadOnlyAt(Index(pos.PawnKey()));
        public UnifiedCorrectionHistory.ReadOnlyEntry ReadMinorPieceCorrectionEntry(Position pos) => CorrectionHistory.ReadOnlyAt(Index(pos.MinorPieceKey()));
        public UnifiedCorrectionHistory.ReadOnlyEntry ReadNonPawnCorrectionEntry(Position pos, Color color) => CorrectionHistory.ReadOnlyAt(Index(pos.NonPawnKey(color)));
        internal void ClearPawnRange(int value, UIntPtr thread, UIntPtr total) => pawnHistory.ClearRange(value, thread, total);
    }
}
