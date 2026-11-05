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
    public sealed class ContinuationCorrectionHistory
    {
        private readonly PieceToCorrectionHistory[] values = new PieceToCorrectionHistory[16 * 64];
        private readonly PieceToCorrectionHistoryReference[] references = new PieceToCorrectionHistoryReference[16 * 64];
        public ContinuationCorrectionHistory()
        {
            for (int i = 0; i < references.Length; ++i)
                references[i] = new Cell(this, i);
        }

        public ContinuationCorrectionHistory(ContinuationCorrectionHistory source) : this()
        {
            CopyFrom(source);
        }

        private static int Index(int piece, int square)
        {
            if ((uint)piece >= 16 || (uint)square >= 64)
                throw new ArgumentOutOfRangeException();
            return piece * 64 + square;
        }

        public ref PieceToCorrectionHistory this[int piece, int square] => ref values[Index(piece, square)];
        public PieceToCorrectionHistoryReference Reference(int piece, int square) => references[Index(piece, square)];
        public void Fill(short value)
        {
            for (int i = 0; i < values.Length; ++i)
                values[i].Fill(in value);
        }

        public void CopyFrom(ContinuationCorrectionHistory source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            for (int i = 0; i < values.Length; ++i)
                values[i].CopyFrom(ref source.values[i]);
        }

        private sealed class Cell : PieceToCorrectionHistoryReference
        {
            private readonly ContinuationCorrectionHistory owner;
            private readonly int index;
            public Cell(ContinuationCorrectionHistory owner, int index)
            {
                this.owner = owner;
                this.index = index;
            }

            public override ref PieceToCorrectionHistory Get() => ref owner.values[index];
        }
    }
}
