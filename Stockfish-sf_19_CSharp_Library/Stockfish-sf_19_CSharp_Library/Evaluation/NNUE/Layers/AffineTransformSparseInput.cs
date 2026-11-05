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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Layers
{
    /// <summary>Block-sparse affine layer; canonical weights can be local or directly mapped.</summary>
    public sealed class AffineTransformSparseInput : AffineTransform
    {
        public AffineTransformSparseInput(int inputDimensions, int outputDimensions) : base(inputDimensions, Validate(outputDimensions))
        {
        }

        internal AffineTransformSparseInput(int input, int output, WeightStorage storage) : base(input, Validate(output), storage) { }

        public AffineTransformSparseInput(AffineTransformSparseInput other) : base(other)
        {
        }

        private static int Validate(int n)
        {
            if (n <= 0 || n % 16 != 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            return n;
        }

        public override int GetWeightIndexScrambled(int i)
        {
            GetWeightIndex(i);
            return i % PaddedInputDimensions * OutputDimensions + i / PaddedInputDimensions;
        }
        public override void Propagate(ReadOnlySpan<byte> input, Span<int> output)
        {
            if (!NnueExecution.UseBlocks) { PropagateScalar(input, output); return; }
            if (InputDimensions % (NnueExecution.UseVariable ? 2 : 8) != 0) { base.Propagate(input, output); return; }
            int chunkSize = NnueExecution.UseVariable ? 1 : 4;
            Span<ushort> indices = InputDimensions <= 4096 ? stackalloc ushort[InputDimensions / chunkSize] : new ushort[InputDimensions / chunkSize];
            int bitCount = (InputDimensions / chunkSize + 7) / 8;
            Span<byte> bits = InputDimensions <= 4096 ? stackalloc byte[bitCount] : new byte[bitCount];
            Span<int> state = stackalloc int[3];
            var info = new NNZInfo(InputDimensions, indices, bits, state, chunkSize);
            if (input.Length < InputDimensions) throw new ArgumentException("Insufficient input.");
            for (int p = 0; p < 2; ++p)
            {
                var cursor = info.MakeCursor(p);
                try
                {
                    int half = InputDimensions / 2;
                    if (NnueExecution.UseFixed && NnueExecution.FixedTiling.HasSparseBitset && half % (NnueExecution.FixedTiling.RegisterBytes * 2) == 0)
                    {
                        int width = NnueExecution.FixedTiling.RegisterBytes;
                        for (int j = 0; j < half; j += width * 2)
                            cursor.RecordRegisters(input.Slice(p * half + j, width), input.Slice(p * half + j + width, width), NnueExecution.FixedTiling);
                    }
                    else cursor.Record2(input.Slice(p * half, half), ReadOnlySpan<byte>.Empty);
                }
                finally { cursor.Dispose(); }
            }
            Propagate(input, output, info);
        }
        public void PropagateVariable(ReadOnlySpan<byte> input, Span<int> output, NNZInfo info, VariableTiling tiling)
        {
            info.Validate(InputDimensions);
            if (info.ChunkSize != 1 || input.Length < InputDimensions || output.Length < OutputDimensions) throw new ArgumentException("Byte NNZ and complete buffers required.");
            // The original pairwise widening relies on transformed inputs in 0..127.
            // Retain the existing generic API's wider byte domain through its scalar path.
            foreach (ushort index in info.NonZeroChunks)
                if (input[index] > 127) { PropagateScalar(input, output); return; }
            ReadOnlySpan<sbyte> image = weights.AsReadOnlySpan();
            biases.AsReadOnlySpan().CopyTo(output);
            // RVV chooses the smallest LMUL that fits its output; canonical rows avoid a weight replica.
            int vl1 = tiling.Length(int.MaxValue, 32);
            int multiplier = vl1 >= OutputDimensions ? 1 : vl1 * 2 >= OutputDimensions ? 2 : vl1 * 4 >= OutputDimensions ? 4 : 8;
            for (int row = 0; row < OutputDimensions;)
            {
                int vl = tiling.Length(OutputDimensions - row, 32, multiplier);
                for (int i = 0; i < info.Count; i += 2)
                {
                    int a = info.NonZeroChunks[i], other = i + 1 < info.Count ? info.NonZeroChunks[i + 1] : -1;
                    for (int k = 0; k < vl; ++k)
                    {
                        int offset = (row + k) * PaddedInputDimensions;
                        short product = (short)(image[offset + a] * input[a]);
                        if (other >= 0) product = unchecked((short)(product + image[offset + other] * input[other]));
                        output[row + k] = unchecked(output[row + k] + product);
                    }
                }
                row += vl;
            }
        }
        public void Propagate(ReadOnlySpan<byte> input, Span<int> output, NNZInfo info)
        {
            info.Validate(InputDimensions);
            if (input.Length < InputDimensions || output.Length < OutputDimensions) throw new ArgumentException("Insufficient layer buffer.");
            if (info.ChunkSize == 1) { PropagateVariable(input, output, info, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseFixed && MightOverflow()) { PropagateScalar(input, output); return; }
            if (NnueExecution.UseFixed) { FixedKernels.Sparse(input, weights.AsReadOnlySpan(), biases.AsReadOnlySpan(), InputDimensions, PaddedInputDimensions, output, info, NnueExecution.FixedTiling); return; }
            ReadOnlySpan<sbyte> image = weights.AsReadOnlySpan();
            ReadOnlySpan<int> bias = biases.AsReadOnlySpan();
            // Every product has magnitude at most 255 * 128 and there are at most
            // InputDimensions products per row. If every bias has that much margin,
            // no partial sum can overflow, regardless of signs or sparsity.
            // A caller may alias output with biases, so retain the checked path then.
            if (!MightOverflow() && !bias.Overlaps(output))
            {
                for (int row = 0; row < OutputDimensions; ++row)
                {
                    int sum = bias[row];
                    ReadOnlySpan<sbyte> w = image.Slice(row * PaddedInputDimensions, InputDimensions);
                    foreach (ushort chunk in info.NonZeroChunks)
                    {
                        int i = chunk * 4;
                        sum = unchecked(sum + w[i] * input[i] + w[i + 1] * input[i + 1]
                            + w[i + 2] * input[i + 2] + w[i + 3] * input[i + 3]);
                    }
                    output[row] = sum;
                }
                return;
            }
            for (int row = 0; row < OutputDimensions; ++row)
            {
                int sum = bias[row];
                ReadOnlySpan<sbyte> w = image.Slice(row * PaddedInputDimensions, InputDimensions);
                foreach (ushort chunk in info.NonZeroChunks)
                {
                    int i = chunk * 4;
                    // Keep the original per-product order and checked overflow semantics.
                    sum = checked(sum + w[i] * input[i]);
                    sum = checked(sum + w[i + 1] * input[i + 1]);
                    sum = checked(sum + w[i + 2] * input[i + 2]);
                    sum = checked(sum + w[i + 3] * input[i + 3]);
                }
                output[row] = sum;
            }
        }
    }
}
