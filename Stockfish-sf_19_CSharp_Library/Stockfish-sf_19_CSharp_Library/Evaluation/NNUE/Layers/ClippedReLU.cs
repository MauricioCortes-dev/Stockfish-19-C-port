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
    public sealed class ClippedReLU
    {
        public int InputDimensions { get; }
        public int OutputDimensions => InputDimensions;
        public int PaddedOutputDimensions { get; }
        public int WeightScaleBitsLocal { get; }

        public ClippedReLU(int dimensions, int scale = NnueConstants.WeightScaleBits)
        {
            if (dimensions <= 0 || scale < 0 || scale > 31)
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
            if (NnueExecution.UseFixed) { FixedKernels.Clipped(input.Slice(0, InputDimensions), output, WeightScaleBitsLocal, NnueExecution.FixedTiling); return; }
            if (NnueExecution.UseVariable)
            { VariableKernels.Clipped(input.Slice(0, InputDimensions), output, WeightScaleBitsLocal, NnueExecution.VariableTiling); return; }
            int i = 0, width = Vector<int>.Count;
            Span<int> clipped = stackalloc int[Vector<int>.Count];
            if (NnueExecution.HardwareVectors && WeightScaleBitsLocal <= 8)
                for (; i <= InputDimensions - width; i += width)
                {
                    int divisor = 1 << WeightScaleBitsLocal;
                    var values = Vector.Min(Vector.Max(NnueKernels.Load<int>(input.Slice(i)), Vector<int>.Zero), new Vector<int>(127 * divisor));
                    NnueKernels.Store(values / new Vector<int>(divisor), clipped);
                    for (int k = 0; k < width; ++k) output[i + k] = (byte)clipped[k];
                }
            for (; i < InputDimensions; ++i)
                output[i] = (byte)Math.Max(0, Math.Min(127, input[i] >> WeightScaleBitsLocal));
        }
    }
}
