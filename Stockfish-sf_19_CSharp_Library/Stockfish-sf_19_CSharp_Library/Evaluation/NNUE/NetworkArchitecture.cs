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
using System.Runtime.InteropServices;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Layers;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>The complete scalar 1024 -> 32 -> 32 -> 1 architecture with concatenated activations and skip outputs.</summary>
    public sealed class NetworkArchitecture : IDisposable
    {
        public const int TransformedFeatureDimensions = NnueConstants.L1, FC0Outputs = 32, FC1Outputs = 32;
        public AffineTransformSparseInput Fc0 { get; }
        public SqrClippedReLU AcSqr0 { get; } = new SqrClippedReLU(32, 7);
        public ClippedReLU Ac0 { get; } = new ClippedReLU(32, 7);
        public AffineTransform Fc1 { get; }
        public SqrClippedReLU AcSqr1 { get; } = new SqrClippedReLU(32, 6);
        public ClippedReLU Ac1 { get; } = new ClippedReLU(32, 6);
        public AffineTransform Fc2 { get; }

        private readonly WeightStorage storage;
        private readonly bool ownsStorage;
        public NetworkArchitecture() : this(new WeightStorage(), true) { }
        internal NetworkArchitecture(WeightStorage storage, bool ownsStorage = false)
        {
            this.storage = storage; this.ownsStorage = ownsStorage;
            Fc0 = new AffineTransformSparseInput(1024, 32, storage);
            Fc1 = new AffineTransform(64, 32, storage);
            Fc2 = new AffineTransform(128, 1, storage);
        }
        public void Dispose() { if (ownsStorage) storage.Dispose(); }

        public NetworkArchitecture(NetworkArchitecture other) : this() => CopyFrom(other);
        public void CopyFrom(NetworkArchitecture other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            Fc0.CopyFrom(other.Fc0);
            Fc1.CopyFrom(other.Fc1);
            Fc2.CopyFrom(other.Fc2);
        }

        public uint GetHashValue()
        {
            uint h = 0xEC42E90DU ^ (1024U * 2);
            h = Fc0.GetHashValue(h);
            h = Ac0.GetHashValue(h);
            h = Fc1.GetHashValue(h);
            h = Ac1.GetHashValue(h);
            return Fc2.GetHashValue(h);
        }

        public bool ReadParameters(Stream stream) => Fc0.ReadParameters(stream) && Ac0.ReadParameters(stream) && Fc1.ReadParameters(stream) && Ac1.ReadParameters(stream) && Fc2.ReadParameters(stream);
        public bool WriteParameters(Stream stream) => Fc0.WriteParameters(stream) && Ac0.WriteParameters(stream) && Fc1.WriteParameters(stream) && Ac1.WriteParameters(stream) && Fc2.WriteParameters(stream);
        public int Propagate(ReadOnlySpan<TransformedFeatureType> transformedFeatures) => Propagate(MemoryMarshal.AsBytes(transformedFeatures));
        public override int GetHashCode() => unchecked((int)GetContentHash().ToUInt64());
        public int Propagate(ReadOnlySpan<byte> transformedFeatures)
        {
            Span<int> fc0 = stackalloc int[32], fc1 = stackalloc int[32], fc2 = stackalloc int[1];
            Span<byte> concat = stackalloc byte[128];
            Fc0.Propagate(transformedFeatures, fc0);
            if (NnueExecution.UseBlocks && !NnueExecution.UseVariable && !NnueExecution.UseFixed) AcSqr0.PropagatePair(fc0, concat, concat.Slice(32));
            else { AcSqr0.Propagate(fc0, concat); Ac0.Propagate(fc0, concat.Slice(32)); }
            Fc1.Propagate(concat, fc1);
            if (NnueExecution.UseBlocks && !NnueExecution.UseVariable && !NnueExecution.UseFixed) AcSqr1.PropagatePair(fc1, concat.Slice(64), concat.Slice(96));
            else { AcSqr1.Propagate(fc1, concat.Slice(64)); Ac1.Propagate(fc1, concat.Slice(96)); }
            Fc2.Propagate(concat, fc2);
            int fwd = checked(fc2[0] + checked(fc0[30] - fc0[31]));
            return checked((int)((long)fwd * (600 * NnueConstants.OutputScale) / ((long)NnueConstants.HiddenOneVal * (1 << NnueConstants.WeightScaleBits) * 2)));
        }

        public int Propagate(ReadOnlySpan<byte> transformedFeatures, NNZInfo info)
        {
            if (NnueExecution.UseVariable && info.ChunkSize == 1) return PropagateVariable(transformedFeatures, info, NnueExecution.VariableTiling);
            Span<int> fc0 = stackalloc int[32], fc1 = stackalloc int[32], fc2 = stackalloc int[1];
            Span<byte> concat = stackalloc byte[128];
            Fc0.Propagate(transformedFeatures, fc0, info);
            if (NnueExecution.UseVariable || NnueExecution.UseFixed) { AcSqr0.Propagate(fc0, concat); Ac0.Propagate(fc0, concat.Slice(32)); }
            else AcSqr0.PropagatePair(fc0, concat, concat.Slice(32));
            Fc1.Propagate(concat, fc1);
            if (NnueExecution.UseVariable || NnueExecution.UseFixed) { AcSqr1.Propagate(fc1, concat.Slice(64)); Ac1.Propagate(fc1, concat.Slice(96)); }
            else AcSqr1.PropagatePair(fc1, concat.Slice(64), concat.Slice(96));
            Fc2.Propagate(concat, fc2);
            int fwd = checked(fc2[0] + checked(fc0[30] - fc0[31]));
            return checked((int)((long)fwd * (600 * NnueConstants.OutputScale) / ((long)NnueConstants.HiddenOneVal * (1 << NnueConstants.WeightScaleBits) * 2)));
        }

        public int PropagateVariable(ReadOnlySpan<byte> input, NNZInfo info, VariableTiling tiling)
        {
            Span<int> fc0 = stackalloc int[32], fc1 = stackalloc int[32], fc2 = stackalloc int[1];
            Span<byte> concat = stackalloc byte[128];
            Fc0.PropagateVariable(input, fc0, info, tiling);
            VariableKernels.Squared(fc0, concat, AcSqr0.WeightScaleBitsLocal, tiling);
            VariableKernels.Clipped(fc0, concat.Slice(32), Ac0.WeightScaleBitsLocal, tiling);
            Fc1.PropagateVariable(concat, fc1, tiling);
            VariableKernels.Squared(fc1, concat.Slice(64), AcSqr1.WeightScaleBitsLocal, tiling);
            VariableKernels.Clipped(fc1, concat.Slice(96), Ac1.WeightScaleBitsLocal, tiling);
            Fc2.PropagateVariable(concat, fc2, tiling);
            int fwd = checked(fc2[0] + checked(fc0[30] - fc0[31]));
            return checked((int)((long)fwd * (600 * NnueConstants.OutputScale) / ((long)NnueConstants.HiddenOneVal * (1 << NnueConstants.WeightScaleBits) * 2)));
        }
        public UIntPtr GetContentHash()
        {
            ulong h = 0;
            h = NnueHash.Combine(h, Fc0.GetContentHash().ToUInt64());
            h = NnueHash.Combine(h, AcSqr0.GetContentHash().ToUInt64());
            h = NnueHash.Combine(h, Ac0.GetContentHash().ToUInt64());
            h = NnueHash.Combine(h, Fc1.GetContentHash().ToUInt64());
            h = NnueHash.Combine(h, Ac1.GetContentHash().ToUInt64());
            h = NnueHash.Combine(h, Fc2.GetContentHash().ToUInt64());
            return new UIntPtr(NnueHash.Combine(h, GetHashValue()));
        }
    }
}
