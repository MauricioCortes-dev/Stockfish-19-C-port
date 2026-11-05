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
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Pawn-entry operations and compatibility constructor for an explicitly pawn-only owner.</summary>
    public sealed partial class SharedHistories
    {
        private readonly PawnHistory pawnHistory;
        private readonly ulong pawnHistSizeMinus1;
        // Compatibility injection explicitly creates a pawn-only owner for consumers that need only PawnEntry.
        public SharedHistories(PawnHistory pawnHistory)
        {
            this.pawnHistory = pawnHistory ?? throw new ArgumentNullException(nameof(pawnHistory));
            ulong size = pawnHistory.GetSize().ToUInt64();
            if (size == 0 || (size & (size - 1)) != 0)
                throw new ArgumentException("The original SharedHistories owner requires a positive power-of-two thread count.", nameof(pawnHistory));
            pawnHistSizeMinus1 = size - 1;
        }
        public ref PawnHistoryEntry PawnEntry(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            return ref pawnHistory[new UIntPtr((ulong)position.PawnKey() & pawnHistSizeMinus1)];
        }
        // C# has no const-qualified member overload. Preserve live read access through a bound view.
        public PawnHistory.ReadOnlyEntry ReadPawnEntry(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            return pawnHistory.ReadOnlyAt(new UIntPtr((ulong)position.PawnKey() & pawnHistSizeMinus1));
        }
    }
}
