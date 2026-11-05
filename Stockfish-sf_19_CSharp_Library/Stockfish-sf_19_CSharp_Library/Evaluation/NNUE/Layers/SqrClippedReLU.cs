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
using System.IO;
using System.Numerics;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Layers
{
    public sealed class SqrClippedReLU
    {
        public int InputDimensions { get; }
        public int OutputDimensions => InputDimensions;
        public int PaddedOutputDimensions { get; }
        public int WeightScaleBitsLocal { get; }

        public SqrClippedReLU(int dimensions, int scale = NnueConstants.WeightScaleBits)
        {
            if (dimensions <= 0 || scale < 5 || scale > 8)
                throw new ArgumentOutOfRangeException();
            InputDimensions = dimensions;
            WeightScaleBitsLocal = scale;
            PaddedOutputDimensions = NnueConstants.CeilToMultiple(dimensions, 32);
        }

        public uint GetHashValue(uint previous) => unchecked(0x538D24C7U + previous);
        public UIntPtr GetContentHash() => new UIntPtr(NnueHash.Combine(0, GetHashValue(0)));
        public bool ReadParameters(Stream stream) => true;
        public bool WriteParameters(Stream stream) => true;
        public void Propagate(ReadOnlySpan<int> input, Span<byte> output)
        {
            if (input.Length < InputDimensions || output.Length < OutputDimensions)
                throw new ArgumentException("Insufficient activation buffer.");
            if (NnueExecution.UseFixed) { FixedKernels.Squared(input.Slice(0, InputDimensions), output, WeightScaleBitsLocal, NnueExecution.FixedTiling); return; }
            if (NnueExecution.UseVariable)
            { VariableKernels.Squared(input.Slice(0, InputDimensions), output, WeightScaleBitsLocal, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseBlocks) { PropagateBlocks(input, output); return; }
            for (int i = 0; i < InputDimensions; ++i)
                output[i] = (byte)Math.Min(127L, ((long)input[i] * input[i]) >> (2 * WeightScaleBitsLocal + 7));
        }
        public void PropagateBlocks(ReadOnlySpan<int> input, Span<byte> output)
        {
            if (input.Length < InputDimensions || output.Length < OutputDimensions) throw new ArgumentException("Insufficient activation buffer.");
            int i = 0, end = InputDimensions / 16 * 16;
            // SF19-000991: unpaired SSE2 logical groups and the original scalar tail.
            for (; i < end; ++i)
            {
                int word = Math.Clamp(input[i], short.MinValue, short.MaxValue);
                output[i] = (byte)Math.Min(127, ((word * word) >> 16) >> (2 * WeightScaleBitsLocal + 7 - 16));
            }
            for (; i < InputDimensions; ++i)
                output[i] = (byte)Math.Min(127L, ((long)input[i] * input[i]) >> (2 * WeightScaleBitsLocal + 7));
        }
        /// <summary>Logical paired SIMD activation, normalized to canonical lane order.
        /// Native pack_epi32 saturates to signed16 before multiplication, even at extreme inputs.</summary>
        public void PropagatePair(ReadOnlySpan<int> input, Span<byte> squared, Span<byte> clipped)
        {
            if (input.Length < InputDimensions || squared.Length < OutputDimensions || clipped.Length < OutputDimensions)
                throw new ArgumentException("Insufficient activation buffer.");
            int i = 0, width = Vector<int>.Count;
            Span<int> sq = stackalloc int[Vector<int>.Count], clip = stackalloc int[Vector<int>.Count];
            if (NnueExecution.HardwareVectors)
                for (; i <= InputDimensions - width; i += width)
                {
                    var word = Vector.Min(Vector.Max(NnueKernels.Load<int>(input.Slice(i)), new Vector<int>(short.MinValue)), new Vector<int>(short.MaxValue));
                    NnueKernels.Store(Vector.Min(word * word / new Vector<int>(1 << (2 * WeightScaleBitsLocal + 7)), new Vector<int>(127)), sq);
                    NnueKernels.Store(Vector.Min(Vector.Max(word, Vector<int>.Zero) / new Vector<int>(1 << WeightScaleBitsLocal), new Vector<int>(127)), clip);
                    for (int k = 0; k < width; ++k) { squared[i + k] = (byte)sq[k]; clipped[i + k] = (byte)clip[k]; }
                }
            for (; i < InputDimensions; ++i)
            {
                int word = Math.Clamp(input[i], short.MinValue, short.MaxValue);
                squared[i] = (byte)Math.Min(127, ((long)word * word) >> (2 * WeightScaleBitsLocal + 7));
                clipped[i] = (byte)Math.Min(127, Math.Max(0, word) >> WeightScaleBitsLocal);
            }
        }
    }
}
