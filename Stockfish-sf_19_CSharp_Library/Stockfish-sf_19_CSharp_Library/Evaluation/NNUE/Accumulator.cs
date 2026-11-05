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
using System.Collections.Generic;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Owned accumulation buffers; use the copy constructor for native value-copy semantics.</summary>
    public class Accumulator
    {
        public IReadOnlyList<short[]> Accumulation { get; } = Array.AsReadOnly(new[] { new short[1024], new short[1024] });
        public IReadOnlyList<int[]> PsqtAccumulation { get; } = Array.AsReadOnly(new[] { new int[8], new int[8] });
        public bool[] Computed { get; } = new bool[2];

        public Accumulator()
        {
        }

        public Accumulator(Accumulator other) => CopyFrom(other);
        public void CopyFrom(Accumulator other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            for (int c = 0; c < 2; ++c)
            {
                other.Accumulation[c].CopyTo(Accumulation[c], 0);
                other.PsqtAccumulation[c].CopyTo(PsqtAccumulation[c], 0);
                Computed[c] = other.Computed[c];
            }
        }
    }
}
