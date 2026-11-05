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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    public static class HistoryConstants
    {
        public const int CORRECTION_HISTORY_LIMIT = 1024;
        public const int CORRHIST_BASE_SIZE = UINT_16_HISTORY_SIZE;
        public const int PAWN_HISTORY_BASE_SIZE = 8192;
        public const int UINT_16_HISTORY_SIZE = 65536;
        public const int LOW_PLY_HISTORY_SIZE = 5;
    }
}
