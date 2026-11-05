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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Native table[2][2], indexed by inCheck and capture category.</summary>
    public sealed class ContinuationHistoryBlock
    {
        private readonly ContinuationHistory[, ] table = new ContinuationHistory[2, 2];
        public ContinuationHistoryBlock()
        {
            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 2; ++j)
                    table[i, j] = new ContinuationHistory();
        }

        public ContinuationHistoryBlock(ContinuationHistoryBlock source) : this()
        {
            CopyFrom(source);
        }

        public ContinuationHistory this[int inCheck, int capture] => table[inCheck, capture];
        public void CopyFrom(ContinuationHistoryBlock source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 2; ++j)
                    table[i, j].CopyFrom(source.table[i, j]);
        }
    }
}
