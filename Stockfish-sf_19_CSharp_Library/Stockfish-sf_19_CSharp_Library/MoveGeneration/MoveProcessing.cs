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

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>Immutable query-independent policy, selected before the first use.</summary>
    public static class MoveProcessing
    {
        public static bool UseBlocks { get; } = Select();
        private static bool Select()
        {
            string? value = Environment.GetEnvironmentVariable("STOCKFISH_MOVE_PROCESSING");
            if (string.IsNullOrEmpty(value) || value == "blocks") return true;
            if (value == "scalar") return false;
            throw new ArgumentException("STOCKFISH_MOVE_PROCESSING must be blocks or scalar.");
        }
    }
}
