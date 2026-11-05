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
using System.Buffers;
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Types;
using Threats = Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features.FullThreats;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Portable RVV operations on bounded caller-owned registers, over local or mapped weights.</summary>
    public static class VariableKernels
    {
        // SF19-000306/307: vector arithmetic wraps at the original lane width.
        public static void Apply(int index, Span<short> register, ReadOnlySpan<short> data, int sign)
        {
            Validate(sign, data.Length - index, register.Length);
            for (int j = 0; j < register.Length; ++j) register[j] = unchecked((short)(register[j] + sign * data[index + j]));
        }
        public static void Apply(int index, Span<int> register, ReadOnlySpan<int> data, int sign)
        {
            Validate(sign, data.Length - index, register.Length);
            for (int j = 0; j < register.Length; ++j) register[j] = unchecked(register[j] + sign * data[index + j]);
        }
        private static void Validate(int sign, int available, int count)
        { if ((sign != 1 && sign != -1) || available < count) throw new ArgumentException("Invalid sign or register extent."); }

        // SF19-000308: the weight column is signed8, widened before adding/subtracting.
        public static void ApplyThreatFeatures(int index, Span<short> register, ReadOnlySpan<sbyte> weights,
            int dimensions, in Threats.IndexList indices, int sign)
        {
            Validate(sign, dimensions - index, register.Length);
            // Validate the whole input before mutating the destination register.
            for (int i = 0; i < indices.Size(); ++i)
                if ((long)indices[i] * dimensions + index + register.Length > weights.Length) throw new ArgumentException("Missing weight column.");
            for (int i = 0; i < indices.Size(); ++i)
            {
                int column = indices[i] * dimensions + index;
                for (int j = 0; j < register.Length; ++j)
                    register[j] = unchecked((short)(register[j] + sign * weights[column + j]));
            }
        }
        public static void Add16(Span<short> destination, ReadOnlySpan<short> weights, int sign, VariableTiling tiling)
        {
            Validate(sign, weights.Length, destination.Length);
            int size = tiling.Length(destination.Length, 16, 8); short[]? rented = null;
            Span<short> register = size <= 1024 ? stackalloc short[size] : (rented = ArrayPool<short>.Shared.Rent(size)).AsSpan(0, size);
            try
            {
                for (int j = 0; j < destination.Length; tiling.IncrementIndex(ref j, destination.Length))
                {
                    int n = tiling.LoadTile(j, destination, register);
                    Apply(j, register.Slice(0, n), weights, sign); tiling.StoreTile(j, destination, register);
                }
            }
            finally { if (rented != null) ArrayPool<short>.Shared.Return(rented); }
        }
        internal static void AddThreats(Span<short> destination, ReadOnlySpan<sbyte> weights, int dimensions,
            in Threats.IndexList indices, int sign, VariableTiling tiling)
        {
            int size = tiling.Length(destination.Length, 16, 8); short[]? rented = null;
            Span<short> register = size <= 1024 ? stackalloc short[size] : (rented = ArrayPool<short>.Shared.Rent(size)).AsSpan(0, size);
            try
            {
                for (int j = 0; j < destination.Length; tiling.IncrementIndex(ref j, destination.Length))
                {
                    int n = tiling.LoadTile(j, destination, register);
                    ApplyThreatFeatures(j, register.Slice(0, n), weights, dimensions, indices, sign); tiling.StoreTile(j, destination, register);
                }
            }
            finally { if (rented != null) ArrayPool<short>.Shared.Return(rented); }
        }
        public static void Add8(Span<short> destination, ReadOnlySpan<sbyte> weights, int sign, VariableTiling tiling)
        {
            Validate(sign, weights.Length, destination.Length);
            for (int j = 0; j < destination.Length; tiling.IncrementIndex(ref j, destination.Length))
            {
                int count = tiling.Length(destination.Length - j, 16, 8);
                for (int lane = 0; lane < count; ++lane) destination[j + lane] = unchecked((short)(destination[j + lane] + sign * weights[j + lane]));
            }
        }
        public static void AddPsqt(Span<int> destination, ReadOnlySpan<int> weights, int sign, VariableTiling tiling)
        {
            Validate(sign, weights.Length, destination.Length);
            int size = tiling.Length(destination.Length, 32); int[]? rented = null;
            Span<int> register = size <= 64 ? stackalloc int[size] : (rented = ArrayPool<int>.Shared.Rent(size)).AsSpan(0, size);
            try
            {
                for (int j = 0; j < destination.Length; tiling.IncrementPsqtIndex(ref j, destination.Length))
                {
                    int n = tiling.LoadPsqt(j, destination, register);
                    Apply(j, register.Slice(0, n), weights, sign); tiling.StorePsqt(j, destination, register);
                }
            }
            finally { if (rented != null) ArrayPool<int>.Shared.Return(rented); }
        }
        public static void Transform(ReadOnlySpan<short> accumulation, Span<byte> output, VariableTiling tiling)
        {
            int length = accumulation.Length / 2;
            if (accumulation.Length % 2 != 0 || output.Length < length) throw new ArgumentException();
            for (int j = 0; j < length;)
            {
                int vl = tiling.Length(length - j, 16, 2);
                for (int lane = 0; lane < vl; ++lane)
                {
                    byte a = (byte)Math.Clamp((int)accumulation[j + lane], 0, 255);
                    byte b = (byte)Math.Clamp((int)accumulation[length + j + lane], 0, 255);
                    output[j + lane] = (byte)(((a * b) >> 8) >> 1);
                }
                j += vl;
            }
        }
        public static Bitboard GetChangedPieces(IReadOnlyList<Piece> before, IReadOnlyList<Piece> after, VariableTiling tiling)
        {
            if (before.Count != 64 || after.Count != 64) throw new ArgumentException("A board has 64 squares.");
            int bytes = tiling.Length(int.MaxValue, 8), multiplier = bytes >= 64 ? 1 : bytes == 32 ? 2 : 4;
            int vl = tiling.Length(64, 8, multiplier); ulong mask = 0;
            for (int i = 0; i < vl; ++i) if (before[i] != after[i]) mask |= 1UL << i;
            return mask;
        }
        public static void Dense(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, ReadOnlySpan<int> biases,
            int dimensions, int padded, Span<int> output, VariableTiling tiling)
        {
            if (dimensions <= 0 || padded < dimensions || input.Length < dimensions || output.Length < biases.Length
                || (long)biases.Length * padded > weights.Length) throw new ArgumentException("Invalid affine buffers or shape.");
            int vl1 = tiling.Length(int.MaxValue, 16);
            int block = dimensions <= vl1 ? dimensions : dimensions <= vl1 * 2 ? dimensions
                : dimensions <= vl1 * 4 ? dimensions : tiling.Length(dimensions, 8, 2);
            for (int row = 0; row < biases.Length; ++row)
            {
                int sum = 0;
                for (int j = 0; j < dimensions; j += Math.Min(block, dimensions - j))
                {
                    int vl = Math.Min(block, dimensions - j);
                    for (int k = 0; k < vl; ++k)
                    {
                        short product = (short)(weights[row * padded + j + k] * input[j + k]);
                        sum = unchecked(sum + product);
                    }
                }
                // Native scalar bias + reduction has no defined signed-overflow contract.
                output[row] = checked(biases[row] + sum);
            }
        }
        public static void Clipped(ReadOnlySpan<int> input, Span<byte> output, int scale, VariableTiling tiling)
        {
            if (scale < 0 || scale > 31 || output.Length < input.Length) throw new ArgumentException("Invalid activation buffers or scale.");
            for (int j = 0; j < input.Length;)
            {
                int vl = tiling.Length(input.Length - j, 32, 4);
                for (int k = 0; k < vl; ++k) output[j + k] = (byte)Math.Min(127, Math.Min(short.MaxValue, Math.Max(0, input[j + k]) >> scale));
                j += vl;
            }
        }
        public static void Squared(ReadOnlySpan<int> input, Span<byte> output, int scale, VariableTiling tiling)
        {
            if (scale < 5 || scale > 8 || output.Length < input.Length) throw new ArgumentException("Invalid activation buffers or scale.");
            for (int j = 0; j < input.Length;)
            {
                int vl = tiling.Length(input.Length - j, 32, 4);
                for (int k = 0; k < vl; ++k)
                {
                    int word = Math.Clamp(input[j + k], short.MinValue, short.MaxValue);
                    int high = (word * word) >> 16;
                    output[j + k] = (byte)Math.Min(127, high >> (2 * scale + 7 - 16));
                }
                j += vl;
            }
        }
    }
}
