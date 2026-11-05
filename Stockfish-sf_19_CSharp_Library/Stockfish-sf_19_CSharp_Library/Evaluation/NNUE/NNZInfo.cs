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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    public enum NNZIndexOrder { Canonical, Avx512Icl }

    /// <summary>Caller-owned metadata for four-byte chunks or individual RVV bytes. No heap or weight copies.</summary>
    public readonly ref struct NNZInfo
    {
        internal readonly Span<ushort> Indices;
        internal readonly Span<byte> Bits;
        // published count, active cursor, completed perspective mask
        internal readonly Span<int> State;
        public int Dimensions { get; }
        public int ChunkSize { get; }
        public int Count => State[0];
        public bool IsComplete => State[1] == 0 && State[2] == 3;
        public ReadOnlySpan<ushort> NonZeroChunks => Indices.Slice(0, Count);
        public ReadOnlySpan<byte> Bitset => Bits;
        public NNZInfo(int dimensions, Span<ushort> indices, Span<byte> bits, Span<int> state, int chunkSize = 4)
        {
            if ((chunkSize != 1 && chunkSize != 4) || dimensions <= 0 || dimensions % (chunkSize * 2) != 0 || dimensions / chunkSize > ushort.MaxValue + 1
                || indices.Length < dimensions / chunkSize || bits.Length < ((dimensions / chunkSize + 7) / 8) || state.Length < 3)
                throw new ArgumentException("Invalid NNZ dimensions or scratch buffers.");
            Dimensions = dimensions; ChunkSize = chunkSize; Indices = indices.Slice(0, dimensions / chunkSize);
            Bits = bits.Slice(0, (dimensions / chunkSize + 7) / 8); State = state.Slice(0, 3);
            Bits.Clear(); State.Clear();
        }
        public NNZCursor MakeCursor(int perspective, NNZIndexOrder order = NNZIndexOrder.Canonical) => new NNZCursor(this, perspective, order);
        public void Validate(int dimensions)
        {
            if (Dimensions != dimensions || !IsComplete) throw new InvalidOperationException("NNZ metadata is incomplete or has different dimensions.");
        }
    }

    /// <summary>Exclusive stack cursor. Four-byte counts publish on Dispose; byte counts publish each completed record.</summary>
    public ref struct NNZCursor
    {
        private NNZInfo info;
        private readonly int perspective, end;
        private readonly NNZIndexOrder order;
        private int next, count;
        private bool disposed;
        internal NNZCursor(NNZInfo info, int perspective, NNZIndexOrder order)
        {
            if ((uint)perspective > 1 || info.State[1] != 0 || info.State[2] != (perspective == 0 ? 0 : 1))
                throw new InvalidOperationException("Complete perspective zero before perspective one; cursors are exclusive.");
            if (order != NNZIndexOrder.Canonical && order != NNZIndexOrder.Avx512Icl) throw new ArgumentOutOfRangeException(nameof(order));
            if (order == NNZIndexOrder.Avx512Icl && info.ChunkSize != 4) throw new ArgumentException("ICL indices address four-byte chunks.");
            this.info = info; this.perspective = perspective; this.order = order;
            next = perspective * info.Dimensions / (2 * info.ChunkSize); end = next + info.Dimensions / (2 * info.ChunkSize);
            count = info.Count; disposed = false; info.State[1] = 1;
        }
        public void Record2(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
        {
            if (disposed || info.State[1] != 1) throw new ObjectDisposedException(nameof(NNZCursor));
            if (first.Length % info.ChunkSize != 0 || second.Length % info.ChunkSize != 0 || first.Length / info.ChunkSize + second.Length / info.ChunkSize > end - next)
                throw new ArgumentException("Record complete chunks inside this perspective.");
            if (order == NNZIndexOrder.Avx512Icl)
            {
                if (first.Length != 64 || second.Length != 64) throw new ArgumentException("ICL record2 requires two 512-bit registers.");
                // packs_epi32 interleaves four dwords from each register per 128-bit lane.
                for (int i = 0; i < 32; ++i)
                {
                    int chunk = i / 8 * 4 + i % 4 + (i % 8 >= 4 ? 16 : 0);
                    ReadOnlySpan<byte> bytes = chunk < 16 ? first.Slice(chunk * 4, 4) : second.Slice((chunk - 16) * 4, 4);
                    RecordChunk(bytes, next + chunk);
                }
                next += 32;
            }
            else { Record(first); Record(second); }
            // RVV publishes the list prefix after each completed output block.
            if (info.ChunkSize == 1) info.State[0] = count;
        }
        /// <summary>Bitset cursor variants: NEON tests all bits, x86/LoongArch/WASM test positive signed dwords.</summary>
        public void RecordRegisters(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, FixedTiling tiling)
        {
            if (disposed || info.State[1] != 1) throw new ObjectDisposedException(nameof(NNZCursor));
            if (info.ChunkSize != 4 || first.Length != tiling.RegisterBytes || second.Length != tiling.RegisterBytes || (first.Length + second.Length) / 4 > end - next || order != NNZIndexOrder.Canonical)
                throw new ArgumentException("Record two complete fixed registers in a four-byte cursor.");
            RecordRegister(first, tiling); RecordRegister(second, tiling);
        }
        private void RecordRegister(ReadOnlySpan<byte> values, FixedTiling tiling)
        {
            int positive = FixedKernels.Positive32Mask(values);
            for (int j = 0; j < values.Length / 4; ++j, ++next)
            {
                bool nonzero = tiling.IsNeon
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(values.Slice(j * 4)) != 0
                    : (positive & (1 << j)) != 0;
                if (!nonzero) continue;
                info.Indices[count++] = (ushort)next; info.Bits[next / 8] |= (byte)(1 << (next % 8));
            }
        }
        private void Record(ReadOnlySpan<byte> values)
        {
            for (int j = 0; j < values.Length; j += info.ChunkSize, ++next)
                RecordChunk(values.Slice(j, info.ChunkSize), next);
        }
        private void RecordChunk(ReadOnlySpan<byte> bytes, int chunk)
        {
            int nonzero = 0;
            for (int i = 0; i < bytes.Length; ++i) nonzero |= bytes[i];
            if (nonzero == 0) return;
            info.Indices[count++] = (ushort)chunk;
            info.Bits[chunk / 8] |= (byte)(1 << (chunk % 8));
        }
        public void Dispose()
        {
            if (disposed) return;
            info.State[0] = count;
            if (next == end) info.State[2] |= 1 << perspective;
            else info.State[2] = -1; // An abandoned partial cursor must never feed a layer.
            info.State[1] = 0; disposed = true;
        }
    }
}
