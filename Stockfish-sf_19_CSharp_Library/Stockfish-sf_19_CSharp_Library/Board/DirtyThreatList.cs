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

using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>SF19-000732: own identity for ValueList&lt;DirtyThreat,96&gt;, with inline value storage.</summary>
    public struct DirtyThreatList
    {
        private ValueList<DirtyThreat, ValueStorage96<DirtyThreat>> values;
        public int Capacity => values.Capacity;
        public int Size() => values.Size();
        public int SSize() => values.SSize();
        public DirtyThreat this[int index] { get => values[index]; set => values[index] = value; }
        public void PushBack(DirtyThreat value) => values.PushBack(value);
        public int MakeSpace(int count) => values.MakeSpace(count);
        public ValueList<DirtyThreat, ValueStorage96<DirtyThreat>>.Enumerator GetEnumerator() => values.GetEnumerator();
    }
}
