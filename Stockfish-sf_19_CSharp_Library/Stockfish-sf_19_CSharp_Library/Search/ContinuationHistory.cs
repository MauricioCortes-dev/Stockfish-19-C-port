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
    /// <summary>16x64 live tables; explicit copies retain native value semantics.</summary>
    public sealed class ContinuationHistory
    {
        private readonly PieceToHistory[] values = new PieceToHistory[16 * 64];
        private readonly PieceToHistoryReference[] references = new PieceToHistoryReference[16 * 64];
        public ContinuationHistory()
        {
            for (int i = 0; i < references.Length; ++i)
                references[i] = new Cell(this, i);
        }

        public ContinuationHistory(ContinuationHistory source) : this()
        {
            CopyFrom(source);
        }

        private static int Index(int piece, int square)
        {
            if ((uint)piece >= 16 || (uint)square >= 64)
                throw new ArgumentOutOfRangeException();
            return piece * 64 + square;
        }

        public ref PieceToHistory this[int piece, int square] => ref values[Index(piece, square)];
        public PieceToHistoryReference Reference(int piece, int square) => references[Index(piece, square)];
        public void Fill(short value)
        {
            for (int i = 0; i < values.Length; ++i)
                values[i].Fill(in value);
        }

        public void CopyFrom(ContinuationHistory source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            for (int i = 0; i < values.Length; ++i)
                values[i].CopyFrom(ref source.values[i]);
        }

        private sealed class Cell : PieceToHistoryReference
        {
            private readonly ContinuationHistory owner;
            private readonly int index;
            public Cell(ContinuationHistory owner, int index)
            {
                this.owner = owner;
                this.index = index;
            }

            public override ref PieceToHistory Get() => ref owner.values[index];
        }
    }
}
