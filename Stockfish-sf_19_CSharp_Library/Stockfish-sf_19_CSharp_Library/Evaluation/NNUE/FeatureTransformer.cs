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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Input transformer with scalar and portable block paths over canonical weights.</summary>
    public sealed class FeatureTransformer : IDisposable
    {
        public const int HalfDimensions = NnueConstants.L1, OutputDimensions = HalfDimensions, BufferSize = OutputDimensions;
        public const int ThreatInputDimensions = (int)FullThreats.Dimensions, PairInputDimensions = (int)PP_3Wide.Dimensions, PsqDimensions = (int)HalfKAv2_hm.Dimensions;
        public const int ThreatAndPpDimensions = ThreatInputDimensions + PairInputDimensions, InputDimensions = PsqDimensions + ThreatAndPpDimensions;
        public const int ThreatWeightSize = ThreatInputDimensions * HalfDimensions, PairWeightSize = PairInputDimensions * HalfDimensions;
        public const int ThreatPsqtWeightSize = ThreatInputDimensions * 8, PairPsqtWeightSize = PairInputDimensions * 8;
        public const int ThreatAndPpWeightSize = ThreatWeightSize + PairWeightSize, ThreatAndPpPsqtSize = ThreatPsqtWeightSize + PairPsqtWeightSize;
        public WeightBuffer<BiasType> Biases { get; }
        public WeightBuffer<WeightType> Weights { get; }
        public WeightBuffer<ThreatWeightType> ThreatAndPpWeights { get; }
        public WeightBuffer<PSQTWeightType> PsqtWeights { get; }
        public WeightBuffer<PSQTWeightType> ThreatAndPpPsqtWeights { get; }

        private readonly WeightStorage storage;
        private readonly bool ownsStorage;
        public FeatureTransformer() : this(new WeightStorage(), true) { }
        internal FeatureTransformer(WeightStorage storage, bool ownsStorage = false)
        {
            this.storage = storage; this.ownsStorage = ownsStorage;
            Biases = storage.Add<BiasType>(HalfDimensions);
            Weights = storage.Add<WeightType>(HalfDimensions * PsqDimensions);
            ThreatAndPpWeights = storage.Add<ThreatWeightType>(ThreatAndPpWeightSize);
            PsqtWeights = storage.Add<PSQTWeightType>(8 * PsqDimensions);
            ThreatAndPpPsqtWeights = storage.Add<PSQTWeightType>(ThreatAndPpPsqtSize);
        }
        public void Dispose() { if (ownsStorage) storage.Dispose(); }

        public FeatureTransformer(FeatureTransformer other) : this() => CopyFrom(other);
        public void CopyFrom(FeatureTransformer other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            other.Biases.CopyTo(Biases, 0);
            other.Weights.CopyTo(Weights, 0);
            other.ThreatAndPpWeights.CopyTo(ThreatAndPpWeights, 0);
            other.PsqtWeights.CopyTo(PsqtWeights, 0);
            other.ThreatAndPpPsqtWeights.CopyTo(ThreatAndPpPsqtWeights, 0);
        }

        public Span<ThreatWeightType> ThreatWeightData() => ThreatAndPpWeights.AsSpan(0, ThreatWeightSize);
        public Span<ThreatWeightType> PawnPairWeightData() => ThreatAndPpWeights.AsSpan(ThreatWeightSize);
        public Span<PSQTWeightType> ThreatPsqtData() => ThreatAndPpPsqtWeights.AsSpan(0, ThreatPsqtWeightSize);
        public Span<PSQTWeightType> PawnPairPsqtData() => ThreatAndPpPsqtWeights.AsSpan(ThreatPsqtWeightSize);
        public static uint CombineHash(params uint[] hashes)
        {
            uint h = 0;
            foreach (uint v in hashes)
                h = ((h << 1) | (h >> 31)) ^ v;
            return h;
        }

        public static uint GetHashValue() => CombineHash(FullThreats.HashValue, PP_3Wide.HashValue, HalfKAv2_hm.HashValue) ^ (OutputDimensions * 2U);
        public void PermuteWeights()
        {
        } // Scalar Packus order is the identity.

        public void UnpermuteWeights()
        {
        }

        public bool ReadParameters(Stream stream)
        {
            try
            {
                NnueBinary.ReadLeb128<BiasType>(stream, Biases);
                NnueBinary.ReadLittleEndian<ThreatWeightType>(stream, ThreatWeightData());
                NnueBinary.ReadLeb128<PSQTWeightType>(stream, ThreatPsqtData());
                NnueBinary.ReadLittleEndian<ThreatWeightType>(stream, PawnPairWeightData());
                NnueBinary.ReadLeb128<PSQTWeightType>(stream, PawnPairPsqtData());
                NnueBinary.ReadLeb128<WeightType>(stream, Weights);
                NnueBinary.ReadLeb128<PSQTWeightType>(stream, PsqtWeights);
                PermuteWeights();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool WriteParameters(Stream stream)
        {
            // Identity layout permits streaming directly, without the native SIMD unpermutation copy.
            try
            {
                NnueBinary.WriteLeb128<BiasType>(stream, Biases);
                NnueBinary.WriteLittleEndian<ThreatWeightType>(stream, ThreatAndPpWeights.AsReadOnlySpan().Slice(0, ThreatWeightSize));
                NnueBinary.WriteLeb128<PSQTWeightType>(stream, ThreatAndPpPsqtWeights.AsReadOnlySpan().Slice(0, ThreatPsqtWeightSize));
                NnueBinary.WriteLittleEndian<ThreatWeightType>(stream, ThreatAndPpWeights.AsReadOnlySpan().Slice(ThreatWeightSize));
                NnueBinary.WriteLeb128<PSQTWeightType>(stream, ThreatAndPpPsqtWeights.AsReadOnlySpan().Slice(ThreatPsqtWeightSize));
                NnueBinary.WriteLeb128<WeightType>(stream, Weights);
                NnueBinary.WriteLeb128<PSQTWeightType>(stream, PsqtWeights);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public UIntPtr GetContentHash()
        {
            ulong h = 0;
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(Biases.AsReadOnlySpan())));
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(Weights.AsReadOnlySpan())));
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(PsqtWeights.AsReadOnlySpan())));
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(ThreatAndPpWeights.AsReadOnlySpan())));
            h = NnueHash.Combine(h, NnueHash.Bytes(MemoryMarshal.AsBytes(ThreatAndPpPsqtWeights.AsReadOnlySpan())));
            return new UIntPtr(NnueHash.Combine(h, GetHashValue()));
        }

        public override int GetHashCode() => unchecked((int)GetContentHash().ToUInt64());
        public int Transform(Position pos, AccumulatorStack stack, AccumulatorCaches cache, Span<TransformedFeatureType> output, int bucket) => Transform(pos, stack, cache, MemoryMarshal.AsBytes(output), bucket);
        public int Transform(Position pos, AccumulatorStack stack, AccumulatorCaches cache, Span<byte> output, int bucket)
        {
            if (output.Length < OutputDimensions)
                throw new ArgumentException("Output buffer too short.", nameof(output));
            if ((uint)bucket >= NnueConstants.PSQTBuckets)
                throw new ArgumentOutOfRangeException(nameof(bucket));
            stack.Evaluate(pos, this, cache);
            AccumulatorState state = stack.Latest;
            Color side = pos.SideToMove();
            int psqt = checked(state.PsqtAccumulation[side][bucket] - state.PsqtAccumulation[~side][bucket]) / 2;
            TransformPerspective(state.Accumulation[side], output, 0);
            TransformPerspective(state.Accumulation[~side], output, 1);
            return psqt;
        }

        public int Transform(Position pos, AccumulatorStack stack, AccumulatorCaches cache, Span<byte> output, int bucket, NNZInfo info)
        {
            if (info.Dimensions != OutputDimensions) throw new ArgumentException("NNZ dimensions differ.");
            int psqt = Transform(pos, stack, cache, output, bucket);
            for (int perspective = 0; perspective < 2; ++perspective)
            {
                var cursor = info.MakeCursor(perspective);
                try
                {
                    // Record final canonical bytes, after clipping/multiplication/packing.
                    int start = perspective * HalfDimensions / 2;
                    for (int j = 0; j < HalfDimensions / 2;)
                    {
                        if (NnueExecution.UseFixed && NnueExecution.FixedTiling.HasSparseBitset && info.ChunkSize == 4)
                        {
                            int width = NnueExecution.FixedTiling.RegisterBytes;
                            cursor.RecordRegisters(output.Slice(start + j, width), output.Slice(start + j + width, width), NnueExecution.FixedTiling);
                            j += width * 2; continue;
                        }
                        int count = info.ChunkSize == 1
                            ? NnueExecution.VariableTiling.Length(HalfDimensions / 2 - j, 16, 2) : Math.Min(64, HalfDimensions / 2 - j);
                        cursor.Record2(output.Slice(start + j, count), ReadOnlySpan<byte>.Empty);
                        j += count;
                    }
                }
                finally { cursor.Dispose(); }
            }
            return psqt;
        }

        public static void TransformPerspective(ReadOnlySpan<short> accumulation, Span<byte> output, int perspective)
        {
            if (accumulation.Length < HalfDimensions || (uint)perspective > 1 || output.Length < (perspective + 1) * (HalfDimensions / 2))
                throw new ArgumentException("Invalid transform buffers or perspective.");
            int offset = (HalfDimensions / 2) * perspective;
            if (NnueExecution.UseBlocks) { NnueKernels.Transform(accumulation.Slice(0, HalfDimensions), output.Slice(offset)); return; }
            for (int j = 0; j < HalfDimensions / 2; ++j)
            {
                int a = Math.Clamp((int)accumulation[j], 0, NnueConstants.FtMaxVal), b = Math.Clamp((int)accumulation[j + HalfDimensions / 2], 0, NnueConstants.FtMaxVal);
                output[offset + j] = (byte)((uint)(a * b) / 512);
            }
        }
    }
}
