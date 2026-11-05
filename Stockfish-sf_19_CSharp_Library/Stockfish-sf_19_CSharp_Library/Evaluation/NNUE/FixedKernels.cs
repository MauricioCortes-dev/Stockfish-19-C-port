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
using System.Buffers.Binary;
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Fixed-register algorithms with canonical boundary order and directly borrowed weight spans.</summary>
    public static class FixedKernels
    {
        // 366: load low64 and zero the high half, including the x86-32 substitute.
        public static void LoadLow64(long value, Span<byte> output)
        {
            if (output.Length < 16) throw new ArgumentException();
            BinaryPrimitives.WriteInt64LittleEndian(output, value); output.Slice(8, 8).Clear();
        }
        // 367/379: unpack low signed bytes and sign bits (SSE2); widening shift (LSX).
        public static void WidenLow64(ulong value, Span<short> output)
        {
            if (output.Length < 8) throw new ArgumentException();
            for (int i = 0; i < 8; ++i) output[i] = unchecked((sbyte)(value >> (8 * i)));
        }
        // 374: LASX extends all sixteen signed input bytes into sixteen signed words.
        public static void Widen128(ReadOnlySpan<sbyte> input, Span<short> output)
        {
            if (input.Length < 16 || output.Length < 16) throw new ArgumentException();
            Span<sbyte> value = stackalloc sbyte[16]; input.Slice(0, 16).CopyTo(value);
            for (int i = 0; i < 16; ++i) output[i] = value[i];
        }
        // 368/369: ARMv7 high-half compatibility helpers, modular signed16 arithmetic.
        public static void AddHigh8(Span<short> destination, ReadOnlySpan<sbyte> input, int sign)
        {
            if (destination.Length < 8 || input.Length < 16 || (sign != 1 && sign != -1)) throw new ArgumentException();
            Span<sbyte> high = stackalloc sbyte[8]; input.Slice(8, 8).CopyTo(high);
            for (int i = 0; i < 8; ++i) destination[i] = unchecked((short)(destination[i] + sign * high[i]));
        }
        // 370/371: value loads/stores; the caller retains ownership and mapping lifetime.
        public static void Load256(ReadOnlySpan<byte> source, Span<byte> register)
        {
            if (source.Length < 32 || register.Length < 32) throw new ArgumentException();
            source.Slice(0, 32).CopyTo(register);
        }
        public static void Store256(Span<byte> destination, ReadOnlySpan<byte> register) => Load256(register, destination);

        // 372/376 and 373/377: native narrowing is independent in each 128-bit lane.
        public static void PackUnsigned16(ReadOnlySpan<short> a, ReadOnlySpan<short> b, Span<byte> output, int registerBytes)
        {
            if ((registerBytes != 16 && registerBytes != 32) || a.Length < registerBytes / 2 || b.Length < registerBytes / 2 || output.Length < registerBytes) throw new ArgumentException();
            Span<short> av = stackalloc short[16], bv = stackalloc short[16];
            a.Slice(0, registerBytes / 2).CopyTo(av); b.Slice(0, registerBytes / 2).CopyTo(bv);
            for (int block = 0; block < registerBytes / 16; ++block)
                for (int i = 0; i < 8; ++i)
                { output[block * 16 + i] = (byte)Math.Clamp((int)av[block * 8 + i], 0, 255); output[block * 16 + 8 + i] = (byte)Math.Clamp((int)bv[block * 8 + i], 0, 255); }
        }
        public static void PackUnsigned32(ReadOnlySpan<int> a, ReadOnlySpan<int> b, Span<ushort> output, int registerBytes)
        {
            if ((registerBytes != 16 && registerBytes != 32) || a.Length < registerBytes / 4 || b.Length < registerBytes / 4 || output.Length < registerBytes / 2) throw new ArgumentException();
            Span<int> av = stackalloc int[8], bv = stackalloc int[8];
            a.Slice(0, registerBytes / 4).CopyTo(av); b.Slice(0, registerBytes / 4).CopyTo(bv);
            for (int block = 0; block < registerBytes / 16; ++block)
                for (int i = 0; i < 4; ++i)
                { output[block * 8 + i] = (ushort)Math.Clamp(av[block * 4 + i], 0, 65535); output[block * 8 + 4 + i] = (ushort)Math.Clamp(bv[block * 4 + i], 0, 65535); }
        }
        // 375/378: these instructions compare signed dwords >0, not arbitrary bytes !=0.
        public static int Positive32Mask(ReadOnlySpan<byte> register)
        {
            if (register.Length != 16 && register.Length != 32) throw new ArgumentException();
            int mask = 0;
            for (int i = 0; i < register.Length / 4; ++i)
                if (BinaryPrimitives.ReadInt32LittleEndian(register.Slice(i * 4)) > 0) mask |= 1 << i;
            return mask;
        }
        // 380: vdotq_s32 has signed inputs and modular signed32 accumulation.
        public static void DotSigned(Span<int> accumulator, ReadOnlySpan<sbyte> a, ReadOnlySpan<sbyte> b)
        {
            if (accumulator.Length < 4 || a.Length < 16 || b.Length < 16) throw new ArgumentException();
            Span<sbyte> av = stackalloc sbyte[16], bv = stackalloc sbyte[16];
            a.Slice(0, 16).CopyTo(av); b.Slice(0, 16).CopyTo(bv);
            for (int k = 0; k < 4; ++k)
                for (int i = 0; i < 4; ++i) accumulator[k] = unchecked(accumulator[k] + av[k * 4 + i] * bv[k * 4 + i]);
        }
        /// <summary>m128/m256/m512 and LSX/LASX unsigned-byte dot helpers; native pair arithmetic is observable outside the NNUE input domain.</summary>
        public static void DotUnsigned(Span<int> accumulator, ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, bool saturatePairs, bool wrapPairs = false)
        {
            int n = checked(accumulator.Length * 4);
            if ((n != 16 && n != 32 && n != 64) || input.Length < n || weights.Length < n || saturatePairs && wrapPairs) throw new ArgumentException();
            Span<byte> av = stackalloc byte[64]; Span<sbyte> bv = stackalloc sbyte[64];
            input.Slice(0, n).CopyTo(av); weights.Slice(0, n).CopyTo(bv);
            for (int k = 0; k < accumulator.Length; ++k)
            {
                int i = k * 4;
                accumulator[k] = unchecked(accumulator[k] + DotChunk(av.Slice(i), bv.Slice(i), saturatePairs, wrapPairs));
            }
        }
        public static void DotSignedPairwise(Span<int> accumulator, ReadOnlySpan<sbyte> input, ReadOnlySpan<sbyte> weights)
        {
            if (accumulator.Length < 4 || input.Length < 16 || weights.Length < 16) throw new ArgumentException();
            Span<sbyte> av = stackalloc sbyte[16], bv = stackalloc sbyte[16];
            input.Slice(0, 16).CopyTo(av); weights.Slice(0, 16).CopyTo(bv);
            for (int k = 0; k < 4; ++k)
            {
                int i = k * 4;
                short a = unchecked((short)(av[i] * bv[i] + av[i + 1] * bv[i + 1]));
                short b = unchecked((short)(av[i + 2] * bv[i + 2] + av[i + 3] * bv[i + 3]));
                accumulator[k] = unchecked(accumulator[k] + a + b);
            }
        }
        public static int HorizontalAdd(ReadOnlySpan<int> lanes, int bias)
        {
            int sum = bias; foreach (int value in lanes) sum = unchecked(sum + value); return sum;
        }
        public static void Add16(Span<short> destination, ReadOnlySpan<short> weights, int sign, FixedTiling tiling)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            int width = tiling.RegisterBytes / 2;
            for (int tile = 0; tile < destination.Length; tile += tiling.Tiles.TileHeight)
                for (int reg = tile; reg < Math.Min(destination.Length, tile + tiling.Tiles.TileHeight); reg += width)
                    for (int lane = reg; lane < Math.Min(destination.Length, reg + width); ++lane)
                        destination[lane] = unchecked((short)(destination[lane] + sign * weights[lane]));
        }
        public static void Add8(Span<short> destination, ReadOnlySpan<sbyte> weights, int sign, FixedTiling tiling)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            Span<short> widened = stackalloc short[16];
            int j = 0, width = tiling.Profile == NnueProfile.Lasx || tiling.IsNeon || tiling.Profile == NnueProfile.Lsx ? 16 : 8;
            for (; j <= destination.Length - width; j += width)
            {
                if (tiling.IsNeon)
                {
                    for (int k = 0; k < 8; ++k) destination[j + k] = unchecked((short)(destination[j + k] + sign * weights[j + k]));
                    AddHigh8(destination.Slice(j + 8), weights.Slice(j), sign);
                    continue;
                }
                if (width == 16) Widen128(weights.Slice(j), widened);
                else WidenLow64(BinaryPrimitives.ReadUInt64LittleEndian(System.Runtime.InteropServices.MemoryMarshal.AsBytes(weights.Slice(j))), widened);
                for (int k = 0; k < width; ++k) destination[j + k] = unchecked((short)(destination[j + k] + sign * widened[k]));
            }
            for (; j < destination.Length; ++j) destination[j] = unchecked((short)(destination[j] + sign * weights[j]));
        }
        public static void AddPsqt(Span<int> destination, ReadOnlySpan<int> weights, int sign, FixedTiling tiling)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            for (int tile = 0; tile < destination.Length; tile += tiling.Tiles.PsqtTileHeight)
                for (int k = tile; k < Math.Min(destination.Length, tile + tiling.Tiles.PsqtTileHeight); ++k)
                    destination[k] = unchecked(destination[k] + sign * weights[k]);
        }
        public static void Transform(ReadOnlySpan<short> accumulation, Span<byte> output, FixedTiling tiling)
        {
            int n = accumulation.Length / 2;
            if (accumulation.Length % 2 != 0 || output.Length < n) throw new ArgumentException();
            for (int block = 0; block < n; block += tiling.RegisterBytes)
                for (int j = block; j < Math.Min(n, block + tiling.RegisterBytes); ++j)
                {
                    int a = Math.Clamp((int)accumulation[j], 0, 255), b = accumulation[j + n];
                    if (tiling.IsNeon || tiling.IsLoongArch || tiling.Profile == NnueProfile.Wasm)
                    { b = Math.Clamp(b, 0, 255); output[j] = (byte)(((a * b) >> 8) >> 1); }
                    else
                    { b = Math.Min(b, 255); output[j] = (byte)Math.Clamp(((a << 7) * b) >> 16, 0, 255); }
                }
        }
        public static void Clipped(ReadOnlySpan<int> input, Span<byte> output, int scale, FixedTiling tiling)
        {
            if (output.Length < input.Length || (uint)scale > 31) throw new ArgumentException();
            int group = tiling.IsNeon ? 8 : tiling.RegisterBytes;
            int end = input.Length / group * group;
            for (int i = 0; i < input.Length; ++i)
            {
                int value = input[i];
                if (i < end)
                {
                    if (tiling.IsNeon) value >>= scale; // vqshrn then vqmovn
                    else if (tiling.Profile == NnueProfile.Sse2 || tiling.Profile == NnueProfile.Sse2_32 || tiling.Profile == NnueProfile.Ssse3)
                        value = Math.Clamp(value, short.MinValue, short.MaxValue) >> scale;
                    else value = Math.Clamp(value, 0, 65535) >> scale;
                    output[i] = (byte)Math.Clamp(value, 0, 127);
                }
                else output[i] = (byte)Math.Clamp(value >> scale, 0, 127);
            }
        }
        public static void Squared(ReadOnlySpan<int> input, Span<byte> output, int scale, FixedTiling tiling)
        {
            if (output.Length < input.Length || scale < 5 || scale > 8) throw new ArgumentException();
            int end = input.Length / tiling.RegisterBytes * tiling.RegisterBytes;
            for (int i = 0; i < input.Length; ++i)
            {
                if (i >= end) { output[i] = (byte)Math.Min(127L, ((long)input[i] * input[i]) >> (2 * scale + 7)); continue; }
                int word = Math.Clamp(input[i], short.MinValue, short.MaxValue);
                int high = tiling.IsNeon ? Math.Min(short.MaxValue, (int)(((long)word * word * 2) >> 16)) : (word * word) >> 16;
                output[i] = (byte)Math.Min(127, high >> (2 * scale + 7 - 16 + (tiling.IsNeon ? 1 : 0)));
            }
        }
        // On 0..127 transformed bytes, native signed/unsigned dot products agree.
        // Keep each native pair's widening/saturation and dependency-chain grouping explicit.
        private static int DotChunk(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, bool saturatePairs, bool wrapPairs)
        {
            int a = input[0] * weights[0] + input[1] * weights[1];
            int b = input[2] * weights[2] + input[3] * weights[3];
            if (wrapPairs) { a = unchecked((short)a); b = unchecked((short)b); }
            else if (saturatePairs) { a = Math.Clamp(a, short.MinValue, short.MaxValue); b = Math.Clamp(b, short.MinValue, short.MaxValue); }
            return a + b;
        }
        private static int Dot4(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, FixedTiling tiling)
            => DotChunk(input, weights, !tiling.IsNeon && !tiling.IsLoongArch, tiling.IsLoongArch);
        public static void Dense(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, ReadOnlySpan<int> biases, int dimensions, int padded, Span<int> output, FixedTiling tiling)
        {
            if (dimensions <= 0 || padded < dimensions || input.Length < dimensions || output.Length < biases.Length || weights.Length < checked(padded * biases.Length)) throw new ArgumentException();
            bool generic = false; for (int j = 0; j < dimensions; ++j) generic |= input[j] > 127;
            for (int row = 0; row < biases.Length; ++row)
            {
                int sum = biases[row], i = 0; var w = weights.Slice(row * padded, dimensions);
                // NEON without dot product and SSE2 use their widening fallback (no pair saturation).
                if (!generic && (tiling.Profile == NnueProfile.NeonDotProd || tiling.IsLoongArch || tiling.HasSparseBitset && !tiling.IsNeon))
                    for (; i <= dimensions - 4; i += 4) sum = unchecked(sum + Dot4(input.Slice(i), w.Slice(i), tiling));
                for (; i < dimensions; ++i) sum = unchecked(sum + input[i] * w[i]);
                output[row] = sum;
            }
        }
        public static void Sparse(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, ReadOnlySpan<int> biases, int dimensions, int padded, Span<int> output, NNZInfo info, FixedTiling tiling)
        {
            info.Validate(dimensions);
            if (input.Length < dimensions || output.Length < biases.Length || weights.Length < checked(padded * biases.Length) || info.ChunkSize != 4) throw new ArgumentException();
            if (!tiling.HasSparseBitset) { Dense(input, weights, biases, dimensions, padded, output, tiling); return; }
            bool generic = false; for (int i = 0; i < dimensions; ++i) generic |= input[i] > 127;
            if (generic) { Dense(input, weights, biases, dimensions, padded, output, tiling); return; }
            for (int row = 0; row < biases.Length; ++row)
            {
                int a = biases[row], b = 0, c = 0, chain = 0;
                var w = weights.Slice(row * padded, dimensions);
                // Pop bits in their original 64-bit word order; no dense scan of weights.
                for (int block = 0; block < info.Bitset.Length; block += 8)
                {
                    ulong bits = 0; int bytes = Math.Min(8, info.Bitset.Length - block);
                    for (int k = 0; k < bytes; ++k) bits |= (ulong)info.Bitset[block + k] << (8 * k);
                    while (bits != 0)
                    {
                        int bit = Bitboards.ConstexprLsb(bits);
                        int i = (block * 8 + bit) * 4; bits &= bits - 1;
                        int product = Dot4(input.Slice(i), w.Slice(i), tiling);
                        if (chain == 0) a = unchecked(a + product); else if (chain == 1) b = unchecked(b + product); else c = unchecked(c + product);
                        chain = (chain + 1) % tiling.SparseChains;
                    }
                }
                output[row] = unchecked(a + b + c);
            }
        }
        public static Bitboard GetChangedPieces(IReadOnlyList<Piece> oldPieces, IReadOnlyList<Piece> newPieces, FixedTiling tiling)
        {
            if (oldPieces.Count < 64 || newPieces.Count < 64) throw new ArgumentException();
            ulong changed = 0;
            if (tiling.IsNeon)
            {
                // vld4 deinterleaves the square array into four independent streams.
                for (int lane = 0; lane < 16; ++lane)
                {
                    int mask = 0; for (int stream = 0; stream < 4; ++stream)
                        if (oldPieces[lane * 4 + stream] != newPieces[lane * 4 + stream]) mask |= 1 << stream;
                    changed |= (ulong)mask << (lane * 4);
                }
            }
            else
                for (int block = 0; block < 64; block += tiling.RegisterBytes)
                {
                    uint mask = 0;
                    for (int lane = 0; lane < tiling.RegisterBytes; ++lane)
                        if (oldPieces[block + lane] != newPieces[block + lane]) mask |= 1u << lane;
                    changed |= (ulong)mask << block;
                }
            return (Bitboard)changed;
        }
    }
}
