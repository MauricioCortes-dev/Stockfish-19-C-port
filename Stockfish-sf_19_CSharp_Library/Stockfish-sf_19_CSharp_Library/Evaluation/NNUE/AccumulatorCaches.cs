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
    /// <summary>Per-worker Finny cache. Clear when changing the network weights.</summary>
    public sealed class AccumulatorCaches
    {
        private readonly AccumulatorCacheEntry[, ] entries = new AccumulatorCacheEntry[64, 2];
        public AccumulatorCaches(Network network) : this(network?.FeatureTransformer ?? throw new ArgumentNullException(nameof(network)))
        {
        }

        public AccumulatorCaches(FeatureTransformer ft)
        {
            for (int s = 0; s < 64; ++s)
                for (int c = 0; c < 2; ++c)
                {
                    entries[s, c] = new AccumulatorCacheEntry();
                    entries[s, c].Clear(ft.Biases);
                }
        }

        public AccumulatorCaches(AccumulatorCaches other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            for (int s = 0; s < 64; ++s)
                for (int c = 0; c < 2; ++c)
                    entries[s, c] = new AccumulatorCacheEntry(other.entries[s, c]);
        }

        public AccumulatorCacheEntry this[Square square, Color color]
        {
            get
            {
                if (square >= Square.SQ_NONE || color >= 2)
                    throw new ArgumentOutOfRangeException();
                return entries[square, color];
            }
        }

        public void Clear(Network network) => Clear(network.FeatureTransformer);
        public void Clear(FeatureTransformer ft)
        {
            for (int s = 0; s < 64; ++s)
                for (int c = 0; c < 2; ++c)
                    entries[s, c].Clear(ft.Biases);
        }
    }
}
