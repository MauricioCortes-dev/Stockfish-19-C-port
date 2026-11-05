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
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Fixed list of the first 32 searched non-best moves; native value semantics.</summary>
    public struct SearchedList
    {
        public const int Capacity = 32;
        private ValueList<Move, ValueStorage32<Move>> values;
        public int Count => values.Size();

        public Move this[int i] => values[i];
        public void PushBack(Move move) => values.PushBack(move);
        public ValueList<Move, ValueStorage32<Move>>.Enumerator GetEnumerator() => values.GetEnumerator();
    }
}
