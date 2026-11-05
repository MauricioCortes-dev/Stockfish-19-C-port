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
using System.Numerics;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Immutable process policy. Scalar remains a separately selectable reference.</summary>
    public static class NnueExecution
    {
        private static readonly string Profile = Select();
        public static bool UseBlocks => Profile != "scalar";
        public static bool UseVariable => Profile == "variable";
        public static bool UseFixed => Profile != "blocks" && Profile != "scalar" && Profile != "variable";
        public static FixedTiling FixedTiling { get; } = new FixedTiling(ParseFixed(Profile));
        public static string SelectedProfile => Profile;
        private static NnueProfile ParseFixed(string profile)
        {
            switch (profile)
            {
                case "sse2-32": return NnueProfile.Sse2_32;
                case "ssse3": return NnueProfile.Ssse3;
                case "sse41": return NnueProfile.Sse41;
                case "neon32": return NnueProfile.Neon32;
                case "neon": return NnueProfile.Neon;
                case "neon-dotprod": return NnueProfile.NeonDotProd;
                case "lsx": return NnueProfile.Lsx;
                case "lasx": return NnueProfile.Lasx;
                case "wasm": return NnueProfile.Wasm;
                default: return NnueProfile.Sse2;
            }
        }
        public static VariableTiling VariableTiling { get; } = new VariableTiling(SelectVectorBits());
        public static SIMDTiling AccumulatorTiling { get; } = new SIMDTiling(1024, 1024, 8, Vector<short>.Count * 2, 8);
        public static bool HardwareVectors => UseBlocks && !UseVariable && !UseFixed && Vector.IsHardwareAccelerated;
        private static int SelectVectorBits()
        {
            string? value = Environment.GetEnvironmentVariable("STOCKFISH_NNUE_VECTOR_BITS");
            if (string.IsNullOrEmpty(value)) return 256;
            if (!int.TryParse(value, out int bits)) throw new ArgumentException("Invalid NNUE vector length.");
            return bits;
        }
        private static string Select()
        {
            string? value = Environment.GetEnvironmentVariable("STOCKFISH_NNUE_EXECUTION");
            if (string.IsNullOrEmpty(value)) return Infrastructure.Runtime.RuntimeCapabilities.DefaultNnueProfile;
            if (value == "blocks" || value == "variable" || value == "sse2" || value == "sse2-32" || value == "ssse3" || value == "sse41"
                || value == "neon32" || value == "neon" || value == "neon-dotprod" || value == "lsx" || value == "lasx" || value == "wasm") return value;
            if (value == "scalar") return value;
            throw new ArgumentException("STOCKFISH_NNUE_EXECUTION has an unknown NNUE profile.");
        }
    }

    /// <summary>Register-count arithmetic from simd.h, independent of a native ISA.</summary>
    public readonly struct SIMDTiling
    {
        public int NumRegs { get; }
        public int NumPsqtRegs { get; }
        public int TileHeight { get; }
        public int PsqtTileHeight { get; }
        public SIMDTiling(int dimensions, int halfDimensions, int psqtBuckets, int registerBytes, int maxRegisters, int psqtRegisterBytes = 0)
        {
            if (psqtRegisterBytes == 0) psqtRegisterBytes = Math.Min(registerBytes, 32);
            NumRegs = BestRegisterCount(dimensions, 2, registerBytes, maxRegisters);
            NumPsqtRegs = BestRegisterCount(psqtBuckets, 4, psqtRegisterBytes, maxRegisters);
            TileHeight = NumRegs * registerBytes / 2;
            PsqtTileHeight = NumPsqtRegs * psqtRegisterBytes / 4;
            if (halfDimensions <= 0 || halfDimensions % TileHeight != 0 || psqtBuckets % PsqtTileHeight != 0)
                throw new ArgumentException("Tile heights must divide the corresponding dimensions.");
        }
        public static int BestRegisterCount(int lanes, int laneBytes, int registerBytes, int maxRegisters, int psqtRegisterBytes = 0)
        {
            if (lanes <= 0 || laneBytes <= 0 || registerBytes < laneBytes || registerBytes % laneBytes != 0
                || maxRegisters <= 0 || checked(lanes * laneBytes) % registerBytes != 0)
                throw new ArgumentOutOfRangeException();
            int ideal = lanes * laneBytes / registerBytes;
            if (ideal <= maxRegisters) return ideal;
            for (int divisor = maxRegisters; divisor > 1; --divisor)
                if (ideal % divisor == 0) return divisor;
            return 1;
        }
    }
}
