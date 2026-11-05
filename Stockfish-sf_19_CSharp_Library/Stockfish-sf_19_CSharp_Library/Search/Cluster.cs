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
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000679: three 10-byte entries and two explicit padding bytes.</summary>
    /// <remarks>Natural alignment, no packed/unaligned atomic fields. Concurrent copies use CopyFrom(ref).</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct Cluster
    {
        private TTEntry first;
        private TTEntry second;
        private TTEntry third;
        private ushort padding;

        public int Count => TTConstants.ClusterSize;
        public Cluster(ref Cluster source) { this = default; CopyFrom(ref source); }
        public TTEntry this[int index]
        {
            get { var snapshot = default(TTEntry); snapshot.CopyFrom(ref At(ref this, index)); return snapshot; }
            set => At(ref this, index).CopyFrom(ref value);
        }
        public static ref TTEntry At(ref Cluster source, int index)
        {
            switch (index)
            {
                case 0: return ref source.first;
                case 1: return ref source.second;
                case 2: return ref source.third;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        public void CopyFrom(ref Cluster source)
        {
            first.CopyFrom(ref source.first);
            second.CopyFrom(ref source.second);
            third.CopyFrom(ref source.third);
            // Padding is not mutated concurrently by Save. Clearing/replacing an owner requires coordination.
            padding = source.padding;
        }
    }
}
