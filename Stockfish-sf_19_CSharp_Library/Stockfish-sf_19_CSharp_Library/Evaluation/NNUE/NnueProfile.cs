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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Logical contracts of fixed-width C++ routes, not a claim of native ISA emission.</summary>
    public enum NnueProfile { Sse2, Sse2_32, Ssse3, Sse41, Neon32, Neon, NeonDotProd, Lsx, Lasx, Wasm }

    public readonly struct FixedTiling
    {
        public NnueProfile Profile { get; }
        public int RegisterBytes => Profile == NnueProfile.Lasx ? 32 : 16;
        public int Registers => Profile == NnueProfile.Sse2_32 || Profile == NnueProfile.Wasm ? 6 : IsNeon ? 16 : IsLoongArch ? 24 : 12;
        public bool IsNeon => Profile == NnueProfile.Neon32 || Profile == NnueProfile.Neon || Profile == NnueProfile.NeonDotProd;
        public bool IsLoongArch => Profile == NnueProfile.Lsx || Profile == NnueProfile.Lasx;
        public bool HasSparseBitset => Profile != NnueProfile.Sse2 && Profile != NnueProfile.Sse2_32 && Profile != NnueProfile.Neon32;
        public int SparseChains => Profile == NnueProfile.NeonDotProd ? 3 : 1;
        private readonly SIMDTiling tiles;
        // default(FixedTiling) is the valid SSE2 profile too; never expose zero tile heights.
        public SIMDTiling Tiles => tiles.TileHeight != 0 ? tiles : new SIMDTiling(1024, 1024, 8, RegisterBytes, Registers, RegisterBytes);
        public FixedTiling(NnueProfile profile)
        {
            if ((uint)profile > (uint)NnueProfile.Wasm) throw new ArgumentOutOfRangeException(nameof(profile));
            Profile = profile; tiles = default;
            tiles = new SIMDTiling(1024, 1024, 8, RegisterBytes, Registers, RegisterBytes);
        }
    }
}
