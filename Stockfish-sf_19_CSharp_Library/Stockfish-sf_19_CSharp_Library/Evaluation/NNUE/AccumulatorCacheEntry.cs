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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    public sealed class AccumulatorCacheEntry
    {
        public BiasType[] Accumulation { get; } = new BiasType[1024];
        public PSQTWeightType[] PsqtAccumulation { get; } = new PSQTWeightType[8];
        public Piece[] Pieces { get; } = new Piece[64];

        public Bitboard PieceBB;
        public AccumulatorCacheEntry()
        {
        }

        public AccumulatorCacheEntry(AccumulatorCacheEntry other) => CopyFrom(other);
        public void CopyFrom(AccumulatorCacheEntry other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            other.Accumulation.CopyTo(Accumulation, 0);
            other.PsqtAccumulation.CopyTo(PsqtAccumulation, 0);
            other.Pieces.CopyTo(Pieces, 0);
            PieceBB = other.PieceBB;
        }

        public void Clear(ReadOnlySpan<BiasType> biases)
        {
            if (biases.Length != 1024)
                throw new ArgumentException("Expected 1024 biases.", nameof(biases));
            biases.CopyTo(Accumulation);
            Array.Clear(PsqtAccumulation, 0, 8);
            Array.Clear(Pieces, 0, 64);
            PieceBB = 0;
        }
    }
}
