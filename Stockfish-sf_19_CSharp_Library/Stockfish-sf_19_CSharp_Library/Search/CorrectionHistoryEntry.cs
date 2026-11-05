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
using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Inline pair of shared correction bundles. Access fields through a live ref.</summary>
    public struct CorrectionHistoryEntry
    {
        private CorrectionBundle<short, AtomicInt16, PieceToCorrectionHistory.Limit1024> white, black;
        public static ref CorrectionBundle<short, AtomicInt16, PieceToCorrectionHistory.Limit1024> At(ref CorrectionHistoryEntry entry, int color)
        {
            if ((uint)color >= 2)
                throw new ArgumentOutOfRangeException(nameof(color));
            if (color == 0)
                return ref entry.white;
            return ref entry.black;
        }

        public void Fill(short value)
        {
            white.Assign(value);
            black.Assign(value);
        }
    }
}
