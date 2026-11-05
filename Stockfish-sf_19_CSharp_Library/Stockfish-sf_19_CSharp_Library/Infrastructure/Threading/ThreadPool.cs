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
using System.Collections.Generic;
using System.Linq;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Numa;
using System.Threading;
using System.Threading.Tasks;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Threading
{
    /// <summary>Managed NUMA-aware pool with persistent workers and coordinated parallel search.</summary>
    /// <remarks>Host commands are serialized; jobs cannot issue blocking pool commands. Workers are borrowed
    /// until the next Set/Dispose. Shared TT/options/network must not be replaced while any job is active.</remarks>
    public sealed partial class ThreadPool : IDisposable
    {
        private readonly object gate = new object();
        private Thread[] threads = Array.Empty<Thread>();
        private SharedState? state;
        private SearchManager? mainManager;
        private Position? setupPosition;
        private bool disposed;
        private int[] boundThreadToNode = Array.Empty<int>();
        public int MaximumThreads { get; }
        public int Count => Volatile.Read(ref threads).Length;
        public bool Empty => Count == 0;
        public Thread this[int index] => Volatile.Read(ref threads)[index];
        public SearchManager MainManager => mainManager ?? throw new InvalidOperationException("Pool is empty.");

        public ThreadPool() : this(NumaTopology.MaximumThreads) { }
        public ThreadPool(int maximumThreads)
        {
            if (maximumThreads <= 0) throw new ArgumentOutOfRangeException(nameof(maximumThreads));
            MaximumThreads = maximumThreads;
        }

        public static UIntPtr NextPowerOfTwo(ulong count)
        {
            ulong result = 1;
            if (count > 1)
            {
                int bit = Bitboards.Msb(count - 1);
                result = unchecked(2UL << bit);
            }
            return UIntPtr.Size == 8 ? new UIntPtr(result) : new UIntPtr(unchecked((uint)result));
        }

        public void Set(SharedState resources, int count, UpdateContextReference? updates = null, SearchManager? searchManager = null)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (!ReferenceEquals(resources.Threads, this)) throw new ArgumentException("SharedState belongs to another pool.", nameof(resources));
            if (count < 0 || count > MaximumThreads) throw new ArgumentOutOfRangeException(nameof(count));
            EnsureHost();
            lock (gate)
            {
                ThrowIfDisposed();
                WaitAllCore();
                RetireCore();
                if (count == 0) return;
                state = resources;
                var created = new List<Thread>();
                try
                {
                    var config = resources.NumaConfig;
                    string policy = resources.Options.Count("NumaPolicy") == UIntPtr.Zero ? "none" : resources.Options["NumaPolicy"].AsString();
                    bool bind = policy != "none" && (policy != "auto" || config.SuggestsBindingThreads(count));
                    // A logical fallback is useful on unsupported hosts, but never report OS binding there.
                    bind &= NumaTopology.SupportsBinding;
                    boundThreadToNode = bind ? config.DistributeThreads(count) : Array.Empty<int>();
                    var counts = new int[config.NodeCount];
                    for (int i = 0; i < count; ++i) ++counts[bind ? boundThreadToNode[i] : 0];
                    for (int node = 0; node < counts.Length; ++node)
                    {
                        if (counts[node] == 0) continue;
                        int n = node;
                        Func<SharedHistories> allocate = () => new SharedHistories(NextPowerOfTwo((ulong)counts[n]));
                        state.InstallHistory(node, bind ? config.ExecuteOnNode(node, allocate) : allocate());
                    }
                    var localIndex = new int[config.NodeCount];
                    mainManager = searchManager ?? new SearchManager(updates ?? new UpdateContextReference());
                    for (int i = 0; i < count; ++i)
                    {
                        int index = i;
                        ISearchManager manager = i == 0 ? (ISearchManager)mainManager : new NullSearchManager();
                        int node = bind ? boundThreadToNode[i] : 0;
                        int partition = localIndex[node]++;
                        Func<Thread> create = () => new Thread(index, () =>
                        {
                            var token = bind ? config.BindCurrentThread(node) : new NumaReplicatedAccessToken(node);
                            return new Worker(resources, manager, index, partition, counts[node], token);
                        }, manager);
                        // The native thread inherits affinity; binding is repeated before allocating the Worker.
                        created.Add(bind ? config.ExecuteOnNode(node, create) : create());
                    }
                    Volatile.Write(ref threads, created.ToArray());
                    ClearCore();
                }
                catch
                {
                    foreach (Thread thread in created) thread.Dispose();
                    Volatile.Write(ref threads, Array.Empty<Thread>());
                    state.InstallHistories(null);
                    boundThreadToNode = Array.Empty<int>();
                    state = null;
                    mainManager = null;
                    throw;
                }
            }
        }

        public int[] GetBoundThreadToNumaNode() => (int[])boundThreadToNode.Clone();
        public int[] GetBoundThreadCountByNumaNode()
        {
            var map = boundThreadToNode;
            if (map.Length == 0) return Array.Empty<int>();
            var counts = new int[map.Max() + 1]; foreach (int n in map) ++counts[n]; return counts;
        }
        public int NumaNodes => Math.Max(1, boundThreadToNode.Distinct().Count());
        public void EnsureNetworkReplicated()
        {
            EnsureHost();
            lock (gate)
            {
                ThrowIfDisposed(); WaitAllCore();
                foreach (var thread in threads) thread.RunCustomJob(thread.Worker.EnsureNetworkReplicated);
                WaitAllCore();
            }
        }

        /// <summary>Run contiguous partitions in stable NUMA order on the already-bound workers.
        /// Holds the lifecycle gate through the completion barrier. Empty pools execute on the caller.</summary>
        internal void RunNumaPartitions(ulong count, Action<int, ulong, ulong> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            EnsureHost();
            lock (gate)
            {
                ThrowIfDisposed();
                WaitAllCore();
                if (threads.Length == 0) { action(-1, 0, count); return; }
                int[] order = PartitionOrder(threads.Length, boundThreadToNode);
                try
                {
                    ulong stride = count / (ulong)threads.Length;
                    for (int i = 0; i < threads.Length; ++i)
                    {
                        int worker = order[i]; ulong start = stride * (ulong)i;
                        ulong length = i + 1 != threads.Length ? stride : count - start;
                        threads[worker].RunCustomJob(() => action(worker, start, length));
                    }
                }
                finally { WaitAllCore(); }
            }
        }
        internal static int[] PartitionOrder(int count, int[] nodes)
        {
            var order = Enumerable.Range(0, count);
            return (nodes.Length == count ? order.OrderBy(i => nodes[i]) : order).ToArray();
        }

        public void RunOnThread(int index, Action<Worker> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            EnsureHost();
            lock (gate)
            {
                ThrowIfDisposed();
                Thread thread = threads[index];
                Worker worker = thread.Worker;
                thread.RunCustomJob(() => action(worker));
            }
        }
        public void WaitOnThread(int index)
        {
            EnsureHost();
            lock (gate) { ThrowIfDisposed(); threads[index].WaitForSearchFinished(); }
        }
        public void WaitForAllJobs()
        {
            EnsureHost();
            lock (gate) { ThrowIfDisposed(); WaitAllCore(); }
        }
        public ulong NodesSearched()
        {
            ulong result = 0;
            foreach (Thread thread in Volatile.Read(ref threads)) result = unchecked(result + thread.Worker.Nodes);
            return result;
        }
        public void Clear()
        {
            EnsureHost();
            lock (gate) { ThrowIfDisposed(); WaitAllCore(); ClearCore(); }
        }
        private void ClearCore()
        {
            foreach (Thread thread in threads) thread.ClearWorker();
            WaitAllCore();
            mainManager?.Clear();
        }

        /// <summary>Prepare private roots and value copies of limits/candidates, without starting any search.</summary>
        public void PrepareRoot(Position position, LimitsType limits)
        {
            if (position == null || limits == null) throw new ArgumentNullException();
            EnsureHost();
            lock (gate)
            {
                ThrowIfDisposed();
                if (threads.Length == 0) throw new InvalidOperationException("Pool is empty.");
                WaitAllCore();
                Position snapshot = Snapshot(position);
                var ownedLimits = new LimitsType(limits);
                var moves = new RootMoves();
                foreach (string text in ownedLimits.SearchMoves)
                {
                    Move move = UciParsing.ToMove(snapshot, text);
                    if (move != Move.None()) moves.EmplaceBack(move);
                }
                if (moves.Empty())
                    foreach (Move move in new MoveList(snapshot, GenType.LEGAL)) moves.EmplaceBack(move);
                var tbConfig = state!.Tablebases?.RankRootMoves(state.Options, snapshot, moves) ?? default;
                setupPosition = snapshot; // Owns the whole immutable pre-root history, even if the caller releases it.
                foreach (Thread thread in threads)
                {
                    Worker worker = thread.Worker;
                    thread.RunCustomJob(() => worker.PrepareRoot(snapshot, ownedLimits, moves, tbConfig));
                }
                WaitAllCore();
            }
        }

        private static Position Snapshot(Position source)
        {
            var states = new List<StateInfo>();
            var seen = new HashSet<StateInfo>();
            for (StateInfo? p = source.State(); p != null; p = p.previous)
            {
                if (!seen.Add(p)) throw new ArgumentException("History contains a cycle.", nameof(source));
                states.Add(p);
            }
            if (states.Count == 0) throw new ArgumentException("Position is not initialized.", nameof(source));
            StateInfo? previous = null;
            for (int i = states.Count - 1; i >= 0; --i)
            {
                var copy = new StateInfo();
                copy.CopyFrom(states[i]);
                copy.previous = previous;
                previous = copy;
            }
            var result = new Position();
            var root = new StateInfo();
            var error = result.Set(source.Fen(), source.IsChess960(), root);
            if (error != null) throw new ArgumentException(error.Message, nameof(source));
            root.CopyFrom(previous!);
            return result;
        }

        private void WaitAllCore()
        {
            List<Exception>? errors = null;
            foreach (Thread thread in threads)
                try { thread.WaitForSearchFinished(); }
                catch (Exception e) { (errors ??= new List<Exception>()).Add(e); }
            if (errors != null) throw new AggregateException(errors);
        }
        private void RetireCore()
        {
            foreach (Thread thread in threads) thread.Dispose();
            Volatile.Write(ref threads, Array.Empty<Thread>());
            boundThreadToNode = Array.Empty<int>();
            state?.InstallHistories(null);
            state = null;
            mainManager = null;
            setupPosition = null;
            secondarySearches = Array.Empty<Task<SearchResult>?>();
        }
        private void EnsureHost()
        {
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            foreach (Thread thread in Volatile.Read(ref threads))
                if (thread.ManagedThreadId == id) throw new InvalidOperationException("Pool commands must run on the host.");
        }
        private void ThrowIfDisposed() { if (disposed) throw new ObjectDisposedException(nameof(ThreadPool)); }
        public void Dispose()
        {
            EnsureHost();
            lock (gate)
            {
                if (disposed) return;
                RetireCore();
                disposed = true;
            }
        }
    }
}
