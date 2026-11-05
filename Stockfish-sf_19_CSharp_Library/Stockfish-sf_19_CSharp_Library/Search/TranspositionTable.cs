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
using System.Threading.Tasks;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Memory;
using WorkerPool = Stockfish_sf_19_CSharp_Library.Infrastructure.Threading.ThreadPool;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Scalar transposition table shared by search workers; probes may observe mixed field values.</summary>
    /// <remarks>Resize, Clear, Dispose and NewSearch require all search workers to be quiescent.</remarks>
    public sealed class TranspositionTable : IDisposable
    {
        private Storage? storage;
        private readonly Func<int, bool, BufferOwner> allocate;
        public TranspositionTable() : this((bytes, hint) => AlignedMemory.Allocate(bytes, 64, true, hint)) { }
        internal TranspositionTable(Func<int, bool, BufferOwner> allocate)
        { this.allocate = allocate ?? throw new ArgumentNullException(nameof(allocate)); }
        private byte generation8;
        private bool disposed;

        public UIntPtr ClusterCount => new UIntPtr(storage == null ? 0UL : storage.Count);
        public byte Generation { get { ThrowIfDisposed(); return generation8; } }

        /// <summary>Allocate the requested positive MiB and reset the table and generation.</summary>
        /// <remarks>Allocation failures throw and preserve the previous table instead of exiting the process.</remarks>
        public void Resize(UIntPtr mbSize, int workerCount = 1)
        {
            ValidateWorkers(workerCount);
            ResizeCore(mbSize, 1, replacement => replacement.Clear(workerCount));
        }
        /// <summary>Allocate aligned storage and first-touch contiguous partitions on the bound pool workers.</summary>
        public void Resize(UIntPtr mbSize, WorkerPool workers)
        {
            if (workers == null) throw new ArgumentNullException(nameof(workers));
            ResizeCore(mbSize, workers.NumaNodes, replacement => replacement.Clear(workers));
        }
        private void ResizeCore(UIntPtr mbSize, int nodes, Action<Storage> clear)
        {
            ThrowIfDisposed();
            ulong mib = mbSize.ToUInt64();
            ulong nativeMax = UIntPtr.Size == 4 ? uint.MaxValue : ulong.MaxValue;
            if (mib == 0 || mib > nativeMax / (1024 * 1024))
                throw new ArgumentOutOfRangeException(nameof(mbSize));
            ulong bytes = mib * 1024 * 1024;
            var replacement = new Storage(bytes / 32, UseHugePageHint(bytes, nodes), allocate);
            try { clear(replacement); }
            catch { replacement.Retire(); throw; }
            storage?.Retire();
            storage = replacement;
            generation8 = 0;
        }
        // Divide instead of multiplying nodes: avoid overflow at native-size boundaries.
        internal static bool UseHugePageHint(ulong bytes, int nodes)
        {
            if (nodes <= 0) throw new ArgumentOutOfRangeException(nameof(nodes));
            return bytes / (ulong)nodes >= (ulong)AlignedMemory.HugePageSize * 8;
        }

        /// <summary>Zero every byte, including padding. All partitions finish before this method returns.</summary>
        public void Clear(int workerCount = 1)
        {
            ValidateWorkers(workerCount);
            Storage current = Current;
            generation8 = 0;
            current.Clear(workerCount);
        }
        public void Clear(WorkerPool workers)
        {
            if (workers == null) throw new ArgumentNullException(nameof(workers));
            Storage current = Current;
            current.Clear(workers);
            generation8 = 0;
        }

        public void NewSearch()
        {
            ThrowIfDisposed();
            generation8 = (byte)((generation8 + 1) & TTConstants.GENERATION_MASK);
        }

        public int Hashfull(int maxAge = 0)
        {
            Storage current = Current;
            int cnt = 0;
            for (int i = 0; i < 1000; ++i)
                for (int j = 0; j < TTConstants.ClusterSize; ++j)
                {
                    ref TTEntry entry = ref current.Entry((ulong)i, j);
                    if (entry.IsOccupied() && entry.RelativeAge(generation8) <= maxAge) ++cnt;
                }
            GC.KeepAlive(current);
            return cnt / TTConstants.ClusterSize;
        }

        public (bool Found, TTData Data, TTWriter Writer) Probe(Key key)
        {
            Storage current = Current;
            ulong index = WideMath.MulHi64(key, new UIntPtr(current.Count)).ToUInt64();
            ushort key16 = unchecked((ushort)(ulong)key);
            for (int i = 0; i < TTConstants.ClusterSize; ++i)
            {
                ref TTEntry entry = ref current.Entry(index, i);
                if (entry.ReadKey16() == key16)
                    return (entry.IsOccupied(), entry.Read(), new TTWriter(current, index, i));
            }

            int replace = 0;
            for (int i = 1; i < TTConstants.ClusterSize; ++i)
            {
                ref TTEntry victim = ref current.Entry(index, replace);
                ref TTEntry candidate = ref current.Entry(index, i);
                if (victim.ReadDepth8() - 8 * victim.RelativeAge(generation8)
                    > candidate.ReadDepth8() - 8 * candidate.RelativeAge(generation8))
                    replace = i;
            }
            return (false, new TTData(Move.None(), Value.VALUE_NONE, Value.VALUE_NONE,
                Depth.DEPTH_NONE, Bound.BOUND_NONE, false), new TTWriter(current, index, replace));
        }

        /// <summary>Advisory prefetch while the backing owner is live. Resize/Dispose still require quiescence.</summary>
        public unsafe void Prefetch(Key key)
        {
            if (!CachePrefetch.Enabled || storage == null) return;
            Storage current = Current;
            ulong index = WideMath.MulHi64(key, new UIntPtr(current.Count)).ToUInt64();
            fixed (TTEntry* address = &current.Entry(index, 0)) CachePrefetch.Prefetch(new IntPtr(address));
            GC.KeepAlive(current);
        }

        /// <summary>Live first entry of the selected cluster. An escaped ref expires on Resize/Dispose.</summary>
        public ref TTEntry FirstEntry(Key key)
        {
            Storage current = Current;
            ulong index = WideMath.MulHi64(key, new UIntPtr(current.Count)).ToUInt64();
            return ref current.Entry(index, 0);
        }

        /// <summary>Retain the backing allocation for a first-entry reference across retirement.
        /// A naked FirstEntry ref must be discarded before Resize/Dispose, as in C++.</summary>
        public EntryLease BorrowFirstEntry(Key key)
        {
            Storage current = Current;
            ulong index = WideMath.MulHi64(key, new UIntPtr(current.Count)).ToUInt64();
            return current.Borrow(index);
        }
        public sealed unsafe class EntryLease : IDisposable
        {
            private readonly BufferOwner owner;
            private readonly int offset;
            internal EntryLease(BufferOwner owner, int offset) { this.owner = owner; this.offset = offset; }
            public ref TTEntry Entry => ref *(TTEntry*)((byte*)owner.Address + offset);
            public void Dispose() => owner.Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            storage?.Retire();
            storage = null;
            disposed = true;
        }

        private Storage Current
        {
            get
            {
                ThrowIfDisposed();
                return storage ?? throw new InvalidOperationException("Resize must allocate the table before use.");
            }
        }
        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(TranspositionTable));
        }
        private static void ValidateWorkers(int workerCount)
        {
            if (workerCount <= 0) throw new ArgumentOutOfRangeException(nameof(workerCount));
        }

        // One directory entry per at most 1 GiB; indices remain global native-size values.
        // Each aligned allocation has independent deterministic ownership. The final block can be small.
        internal sealed unsafe class Storage
        {
            internal const int ClustersPerSegment = (1 << 30) / 32;
            private BufferOwner[] segments;
            private readonly int clustersPerSegment;
            private bool retired;
            internal ulong Count { get; }

            internal Storage(ulong count) : this(count, false, (bytes, hint) => AlignedMemory.Allocate(bytes, 64, true, hint)) { }
            internal Storage(ulong count, bool hint, Func<int, bool, BufferOwner> allocate, int clustersPerSegment = ClustersPerSegment)
            {
                if (clustersPerSegment <= 0 || clustersPerSegment > ClustersPerSegment) throw new ArgumentOutOfRangeException(nameof(clustersPerSegment));
                this.clustersPerSegment = clustersPerSegment;
                Count = count;
                ulong segmentCount = count / (ulong)clustersPerSegment + (count % (ulong)clustersPerSegment == 0 ? 0UL : 1UL);
                if (count == 0 || segmentCount > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(count));
                segments = new BufferOwner[(int)segmentCount];
                try
                {
                    for (int i = 0; i < segments.Length; ++i)
                    {
                        int length = checked((int)Math.Min((ulong)clustersPerSegment, count - (ulong)i * (ulong)clustersPerSegment) * 32);
                        segments[i] = allocate(length, hint);
                        if (segments[i].Length != length || ((ulong)segments[i].Address.ToInt64() & 63) != 0)
                            throw new InvalidOperationException("TT allocator must return an aligned buffer of the requested length.");
                    }
                }
                catch { Retire(); throw; }
            }
            private void Check() { if (retired) throw new ObjectDisposedException(nameof(TranspositionTable)); }
            internal ref TTEntry Entry(ulong index, int slot)
            {
                Check();
                if (index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if ((uint)slot >= TTConstants.ClusterSize) throw new ArgumentOutOfRangeException(nameof(slot));
                BufferOwner owner = segments[(int)(index / (ulong)clustersPerSegment)];
                return ref *(TTEntry*)((byte*)owner.Address + (int)(index % (ulong)clustersPerSegment) * 32 + slot * 10);
            }
            internal EntryLease Borrow(ulong index)
            {
                Check();
                BufferOwner lease = segments[(int)(index / (ulong)clustersPerSegment)].Retain();
                try { return new EntryLease(lease, (int)(index % (ulong)clustersPerSegment) * 32); }
                catch { lease.Dispose(); throw; }
            }
            internal void Clear(WorkerPool workers) => workers.RunNumaPartitions(Count, (_, start, length) => ClearRange(start, length));
            internal void Clear(int workerCount) => Parallel.For(0, workerCount, i =>
            {
                ulong stride = Count / (ulong)workerCount;
                ulong start = stride * (ulong)i;
                ClearRange(start, i + 1 != workerCount ? stride : Count - start);
            });
            internal void ClearRange(ulong start, ulong length)
            {
                Check();
                if (start > Count || length > Count - start) throw new ArgumentOutOfRangeException(nameof(length));
                while (length != 0)
                {
                    BufferOwner block = segments[(int)(start / (ulong)clustersPerSegment)];
                    int offset = (int)(start % (ulong)clustersPerSegment) * 32;
                    int clusters = (int)Math.Min((ulong)((block.Length - offset) / 32), length);
                    block.Bytes.Slice(offset, clusters * 32).Clear();
                    start += (ulong)clusters; length -= (ulong)clusters;
                }
            }
            internal void Retire()
            {
                retired = true;
                foreach (var owner in segments) owner?.Dispose();
                segments = Array.Empty<BufferOwner>();
            }
        }
    }
}
