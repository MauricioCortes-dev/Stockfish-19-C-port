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

using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000535: per-node search state. Copies share pointer targets, not scalar fields.</summary>
    /// <remarks>default matches native value initialization (Stack{}), not an uninitialized C++ local.
    /// Each frame and its local PV/correction targets require exclusive access.</remarks>
    public struct Stack
    {
        public PVMovesReference? pv;
        public PieceToHistoryReference? continuationHistory;
        public PieceToCorrectionHistoryReference? continuationCorrectionHistory;
        public int ply;
        public Move currentMove;
        public Move excludedMove;
        public Value staticEval;
        public int statScore;
        public int moveCount;
        public bool inCheck;
        public bool ttPv;
        public bool ttHit;
        public bool followPV;
        public int cutoffCnt;
        public int reduction;
    }
}
