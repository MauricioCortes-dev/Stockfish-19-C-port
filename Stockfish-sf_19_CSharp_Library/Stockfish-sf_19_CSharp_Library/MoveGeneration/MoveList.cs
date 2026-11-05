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
using System.Collections;
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>SF19-000213: instantánea inmutable de jugadas generadas, con copia observable por valor.</summary>
    /// <remarks>El array privado se rellena solo al construir y nunca se expone ni modifica después.
    /// Copiar la estructura puede compartir ese almacenamiento inmutable. Default es una lista vacía.</remarks>
    public readonly struct MoveList : IReadOnlyList<Move>
    {
        private readonly Move[] moves;
        public int Count { get; }

        public MoveList(Position pos, GenType type)
        {
            moves = new Move[ChessConstants.MAX_MOVES];
            Count = MoveGenerator.Generate(pos, moves, 0, type);
        }

        public int Size() => Count;
        public Move this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return moves[index];
            }
        }
        public bool Contains(Move move)
        {
            for (int i = 0; i < Count; ++i)
                if (moves[i] == move) return true;
            return false;
        }
        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<Move> IEnumerable<Move>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Move>
        {
            private readonly MoveList list;
            private int index;
            internal Enumerator(MoveList list) { this.list = list; index = -1; }
            public bool MoveNext()
            {
                if (index + 1 < list.Count) { ++index; return true; }
                index = list.Count;
                return false;
            }
            public Move Current
            {
                get
                {
                    if (index < 0 || index >= list.Count) throw new InvalidOperationException();
                    return list[index];
                }
            }
            object IEnumerator.Current => Current;
            public void Reset() { index = -1; }
            public void Dispose() { }
        }
    }
}
