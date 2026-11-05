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
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Layers
{
    /// <summary>Canonical affine layer with scalar and block propagation; copies own independent parameters.</summary>
    public class AffineTransform : IDisposable
    {
        public int InputDimensions { get; }
        public int OutputDimensions { get; }
        public int PaddedInputDimensions { get; }
        public int PaddedOutputDimensions { get; }

        protected readonly WeightBuffer<int> biases;
        protected readonly WeightBuffer<sbyte> weights;
        private readonly WeightStorage storage;
        private readonly bool ownsStorage;
        public AffineTransform(int inputDimensions, int outputDimensions) : this(inputDimensions, outputDimensions, new WeightStorage(), true) { }
        internal AffineTransform(int inputDimensions, int outputDimensions, WeightStorage storage, bool ownsStorage = false)
        {
            this.storage = storage; this.ownsStorage = ownsStorage;
            if (inputDimensions <= 0 || outputDimensions <= 0)
                throw new ArgumentOutOfRangeException();
            InputDimensions = inputDimensions;
            OutputDimensions = outputDimensions;
            PaddedInputDimensions = NnueConstants.CeilToMultiple(inputDimensions, 32);
            PaddedOutputDimensions = NnueConstants.CeilToMultiple(outputDimensions, 32);
            int weightCount = checked(outputDimensions * PaddedInputDimensions);
            biases = storage.Add<int>(outputDimensions);
            weights = storage.Add<sbyte>(weightCount);
        }

        public void Dispose() { if (ownsStorage) storage.Dispose(); }
        public AffineTransform(AffineTransform other) : this((other ?? throw new ArgumentNullException(nameof(other))).InputDimensions, other.OutputDimensions) => CopyFrom(other);
        public void CopyFrom(AffineTransform other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other.InputDimensions != InputDimensions || other.OutputDimensions != OutputDimensions)
                throw new ArgumentException("Dimensions differ.");
            other.biases.CopyTo(biases, 0);
            other.weights.CopyTo(weights, 0);
        }

        public uint GetHashValue(uint previous) => unchecked((0xCC03DAE4U + (uint)OutputDimensions) ^ (previous >> 1) ^ (previous << 31));
        public int GetWeightIndex(int index)
        {
            if ((uint)index >= weights.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index;
        }

        public virtual int GetWeightIndexScrambled(int index)
        {
            GetWeightIndex(index);
            int input = index % PaddedInputDimensions;
            return input / 4 * OutputDimensions * 4 + index / PaddedInputDimensions * 4 + input % 4;
        }

        public bool ReadParameters(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            try
            {
                NnueBinary.ReadLittleEndian<int>(stream, biases);
                NnueBinary.ReadLittleEndian<sbyte>(stream, weights);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool WriteParameters(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            try
            {
                NnueBinary.WriteLittleEndian<int>(stream, biases);
                NnueBinary.WriteLittleEndian<sbyte>(stream, weights);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public UIntPtr GetContentHash()
        {
            ulong h = NnueHash.Combine(0, NnueHash.Bytes(MemoryMarshal.AsBytes(biases.AsReadOnlySpan())));
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(weights.AsReadOnlySpan())));
            return new UIntPtr(NnueHash.Combine(h, GetHashValue(0)));
        }

        public virtual void Propagate(ReadOnlySpan<byte> input, Span<int> output)
        {
            if (input.Length < InputDimensions || output.Length < OutputDimensions)
                throw new ArgumentException("Insufficient layer buffer.");
            if (NnueExecution.UseFixed && MightOverflow()) { PropagateScalar(input, output); return; }
            if (NnueExecution.UseFixed) { FixedKernels.Dense(input, weights.AsReadOnlySpan(), biases.AsReadOnlySpan(), InputDimensions, PaddedInputDimensions, output, NnueExecution.FixedTiling); return; }
            if (NnueExecution.UseVariable)
            { PropagateVariable(input, output, NnueExecution.VariableTiling); return; }
            if (NnueExecution.UseBlocks) { PropagateBlocks(input, output); return; }
            PropagateScalar(input, output);
        }

        // Preserve the existing public API's checked diagnostic near signed32 limits.
        // The raw SIMD kernels remain modular. Ordinary NNUE biases take the fast path.
        protected bool MightOverflow()
        {
            long margin = (long)InputDimensions * 255 * 128;
            foreach (int bias in biases.AsReadOnlySpan())
                if (bias < int.MinValue + margin || bias > int.MaxValue - margin) return true;
            return false;
        }
        public void PropagateVariable(ReadOnlySpan<byte> input, Span<int> output, VariableTiling tiling)
        {
            if (input.Length < InputDimensions || output.Length < OutputDimensions) throw new ArgumentException("Insufficient layer buffer.");
            VariableKernels.Dense(input, weights.AsReadOnlySpan(), biases.AsReadOnlySpan(), InputDimensions, PaddedInputDimensions, output, tiling);
        }
        public void PropagateScalar(ReadOnlySpan<byte> input, Span<int> output)
        {
            if (input.Length < InputDimensions || output.Length < OutputDimensions) throw new ArgumentException("Insufficient layer buffer.");
            biases.AsReadOnlySpan().CopyTo(output);
            ReadOnlySpan<sbyte> image = weights.AsReadOnlySpan();
            for (int i = 0; i < InputDimensions; ++i)
                if (input[i] != 0)
                    for (int j = 0; j < OutputDimensions; ++j)
                        output[j] = checked(output[j] + image[j * PaddedInputDimensions + i] * input[i]);
        }
        private void PropagateBlocks(ReadOnlySpan<byte> input, Span<int> output)
        {
            ReadOnlySpan<sbyte> image = weights.AsReadOnlySpan();
            ReadOnlySpan<int> bias = biases.AsReadOnlySpan();
            // Apply the sparse layer's same conservative proof to the two dense
            // consumers too. Keep the existing checked order for unsafe biases
            // and aliased output; weights stay in their canonical mapped image.
            if (!MightOverflow() && !bias.Overlaps(output))
            {
                PropagateBoundedBlocks(input, output, image, bias);
                return;
            }
            Span<int> products = stackalloc int[Vector<byte>.Count];
            for (int row = 0; row < OutputDimensions; ++row)
            {
                int sum = bias[row];
                ReadOnlySpan<sbyte> w = image.Slice(row * PaddedInputDimensions, InputDimensions);
                // Canonical rows support direct mapped access; grouping does not require weight scrambling.
                int i = 0;
                if (NnueExecution.HardwareVectors)
                    for (; i <= InputDimensions - Vector<byte>.Count; i += Vector<byte>.Count)
                    {
                        NnueKernels.Products(input.Slice(i), w.Slice(i), products);
                        for (int lane = 0; lane < products.Length; ++lane) sum = checked(sum + products[lane]);
                    }
                for (; i <= InputDimensions - 4; i += 4)
                    for (int lane = 0; lane < 4; ++lane) sum = checked(sum + w[i + lane] * input[i + lane]);
                for (; i < InputDimensions; ++i) sum = checked(sum + w[i] * input[i]);
                output[row] = sum;
            }
        }

        private void PropagateBoundedBlocks(ReadOnlySpan<byte> input, Span<int> output,
            ReadOnlySpan<sbyte> image, ReadOnlySpan<int> bias)
        {
            for (int row = 0; row < OutputDimensions; ++row)
            {
                int sum = bias[row], i = 0;
                ReadOnlySpan<sbyte> w = image.Slice(row * PaddedInputDimensions, InputDimensions);
                if (NnueExecution.HardwareVectors)
                    for (; i <= InputDimensions - Vector<byte>.Count; i += Vector<byte>.Count)
                        sum = unchecked(sum + NnueKernels.BoundedDotProduct(input.Slice(i), w.Slice(i)));
                for (; i <= InputDimensions - 4; i += 4)
                    sum = unchecked(sum + w[i] * input[i] + w[i + 1] * input[i + 1]
                        + w[i + 2] * input[i + 2] + w[i + 3] * input[i + 3]);
                for (; i < InputDimensions; ++i) sum = unchecked(sum + w[i] * input[i]);
                output[row] = sum;
            }
        }
    }
}
