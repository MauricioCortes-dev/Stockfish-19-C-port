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
using System.Collections.ObjectModel;

namespace Stockfish_sf_19_CSharp_Library.Types
{
    /// <summary>Valores materiales originales y tabla por código Piece, independientes de Position.</summary>
    public static class Values
    {
        public static readonly Value PawnValue = 208;
        public static readonly Value KnightValue = 781;
        public static readonly Value BishopValue = 825;
        public static readonly Value RookValue = 1276;
        public static readonly Value QueenValue = 2538;

        // Array.AsReadOnly impide modificar la tabla pública incluso mediante IList<Value>.
        public static readonly ReadOnlyCollection<Value> PieceValue = Array.AsReadOnly(new Value[]
        {
            Value.VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, Value.VALUE_ZERO, Value.VALUE_ZERO,
            Value.VALUE_ZERO, PawnValue, KnightValue, BishopValue, RookValue, QueenValue, Value.VALUE_ZERO, Value.VALUE_ZERO
        });
    }
}
