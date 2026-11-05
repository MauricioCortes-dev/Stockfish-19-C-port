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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Portable block kernels over the original local/mapped spans. No parameter replicas.</summary>
    public static class NnueKernels
    {
        // netstandard2.1 lacks Vector(ReadOnlySpan)/CopyTo(Span). These unaligned
        // value loads/stores work on both managed and mapped spans without retaining pointers.
        internal static Vector<T> Load<T>(ReadOnlySpan<T> values) where T : unmanaged => MemoryMarshal.Read<Vector<T>>(MemoryMarshal.AsBytes(values));
        internal static void Store<T>(Vector<T> value, Span<T> values) where T : unmanaged => MemoryMarshal.Write(MemoryMarshal.AsBytes(values), ref value);

        internal static void Products(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights, Span<int> products)
        {
            Vector.Widen(Load<byte>(input), out Vector<ushort> i0, out Vector<ushort> i1);
            Vector.Widen(Load<sbyte>(weights), out Vector<short> w0, out Vector<short> w1);
            Vector.Widen(i0, out Vector<uint> ia, out Vector<uint> ib);
            Vector.Widen(i1, out Vector<uint> ic, out Vector<uint> id);
            Vector.Widen(w0, out Vector<int> wa, out Vector<int> wb);
            Vector.Widen(w1, out Vector<int> wc, out Vector<int> wd);
            int n = Vector<int>.Count;
            Store(Vector.AsVectorInt32(ia) * wa, products);
            Store(Vector.AsVectorInt32(ib) * wb, products.Slice(n));
            Store(Vector.AsVectorInt32(ic) * wc, products.Slice(2 * n));
            Store(Vector.AsVectorInt32(id) * wd, products.Slice(3 * n));
        }

        // A byte * sbyte product fits signed16 even for the public byte domain
        // 0..255. Widen before reducing: summing products in signed16 would wrap.
        // The caller proves that adding this block to its bias cannot overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int BoundedDotProduct(ReadOnlySpan<byte> input, ReadOnlySpan<sbyte> weights)
        {
            Vector.Widen(Load<byte>(input), out Vector<ushort> i0, out Vector<ushort> i1);
            Vector.Widen(Load<sbyte>(weights), out Vector<short> w0, out Vector<short> w1);
            Vector.Widen(Vector.AsVectorInt16(i0) * w0, out Vector<int> p0, out Vector<int> p1);
            Vector.Widen(Vector.AsVectorInt16(i1) * w1, out Vector<int> p2, out Vector<int> p3);
            return Vector.Dot(p0 + p1 + p2 + p3, new Vector<int>(1));
        }

        public static void Add16(Span<short> destination, ReadOnlySpan<short> weights, int sign)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            if (NnueExecution.UseVariable) { VariableKernels.Add16(destination, weights, sign, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseFixed) { FixedKernels.Add16(destination, weights, sign, NnueExecution.FixedTiling); return; }
            int j = 0, width = Vector<short>.Count;
            if (NnueExecution.HardwareVectors)
                while (j <= destination.Length - width)
                {
                    int end = Math.Min(destination.Length - destination.Length % width, j + NnueExecution.AccumulatorTiling.TileHeight);
                    for (; j < end; j += width)
                    {
                        var a = Load<short>(destination.Slice(j)); var b = Load<short>(weights.Slice(j));
                        Store(sign == 1 ? a + b : a - b, destination.Slice(j));
                    }
                }
            for (; j < destination.Length; ++j) destination[j] = unchecked((short)(destination[j] + sign * weights[j]));
        }
        public static void Add8(Span<short> destination, ReadOnlySpan<sbyte> weights, int sign)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            if (NnueExecution.UseVariable) { VariableKernels.Add8(destination, weights, sign, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseFixed) { FixedKernels.Add8(destination, weights, sign, NnueExecution.FixedTiling); return; }
            int j = 0, width = Vector<sbyte>.Count, half = Vector<short>.Count;
            if (NnueExecution.HardwareVectors)
                while (j <= destination.Length - width)
                {
                    int end = Math.Min(destination.Length - destination.Length % width, j + NnueExecution.AccumulatorTiling.TileHeight);
                    for (; j < end; j += width)
                    {
                        Vector.Widen(Load<sbyte>(weights.Slice(j)), out Vector<short> lo, out Vector<short> hi);
                        var a = Load<short>(destination.Slice(j)); var b = Load<short>(destination.Slice(j + half));
                        Store(sign == 1 ? a + lo : a - lo, destination.Slice(j));
                        Store(sign == 1 ? b + hi : b - hi, destination.Slice(j + half));
                    }
                }
            for (; j < destination.Length; ++j) destination[j] = unchecked((short)(destination[j] + sign * weights[j]));
        }
        public static void AddPsqt(Span<int> destination, ReadOnlySpan<int> weights, int sign)
        {
            if (weights.Length < destination.Length || (sign != 1 && sign != -1)) throw new ArgumentException();
            if (NnueExecution.UseVariable)
            {
                // Retain this public API's checked signed32 diagnostic; raw RVV Apply is modular.
                for (int j = 0; j < destination.Length; ++j) _ = checked(destination[j] + sign * weights[j]);
                VariableKernels.AddPsqt(destination, weights, sign, NnueExecution.VariableTiling); return;
            }
            if (NnueExecution.UseFixed) { for (int k = 0; k < destination.Length; ++k) _ = checked(destination[k] + sign * weights[k]); FixedKernels.AddPsqt(destination, weights, sign, NnueExecution.FixedTiling); return; }
            // Keep the managed diagnostic for native signed overflow; PSQT has only eight lanes.
            for (int j = 0; j < destination.Length; ++j) destination[j] = checked(destination[j] + sign * weights[j]);
        }
        public static void Transform(ReadOnlySpan<short> accumulation, Span<byte> output)
        {
            int length = accumulation.Length / 2;
            if (accumulation.Length % 2 != 0 || output.Length < length) throw new ArgumentException();
            if (NnueExecution.UseVariable) { VariableKernels.Transform(accumulation, output, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseFixed) { FixedKernels.Transform(accumulation, output, NnueExecution.FixedTiling); return; }
            int j = 0, width = Vector<short>.Count;
            Span<int> products = stackalloc int[Vector<int>.Count * 2];
            if (NnueExecution.HardwareVectors)
                for (; j <= length - width; j += width)
                {
                    var a = Vector.Min(Vector.Max(Load<short>(accumulation.Slice(j)), Vector<short>.Zero), new Vector<short>(255));
                    var b = Vector.Min(Vector.Max(Load<short>(accumulation.Slice(j + length)), Vector<short>.Zero), new Vector<short>(255));
                    Vector.Widen(a, out Vector<int> a0, out Vector<int> a1);
                    Vector.Widen(b, out Vector<int> b0, out Vector<int> b1);
                    Store(a0 * b0, products); Store(a1 * b1, products.Slice(Vector<int>.Count));
                    for (int k = 0; k < width; ++k) output[j + k] = (byte)(products[k] >> 9);
                }
            for (; j < length; ++j) output[j] = (byte)(Math.Clamp((int)accumulation[j], 0, 255) * Math.Clamp((int)accumulation[j + length], 0, 255) >> 9);
        }
    }
}
