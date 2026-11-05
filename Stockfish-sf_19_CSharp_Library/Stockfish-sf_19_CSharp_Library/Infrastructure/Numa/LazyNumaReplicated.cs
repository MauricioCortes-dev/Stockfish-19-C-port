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
using System.Linq;
namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numa
{
    /// <summary>Process-local replicas, copied on the target domain. Exactly one published instance per node.</summary>
    /// <remarks>Cloner must return independent data; readers borrow read-only references until the next
    /// quiescent mutation. No interprocess shared memory or physical-page placement is implied.</remarks>
    public class LazyNumaReplicated<T> : NumaReplicatedBase where T : class
    {
        private readonly Func<T, T> clone;
        private readonly bool eager;
        private T?[] instances = Array.Empty<T?>();
        public LazyNumaReplicated(NumaReplicationContext context, T source, Func<T, T> cloner) : this(context, source, cloner, false) { }
        protected LazyNumaReplicated(NumaReplicationContext context, T source, Func<T, T> cloner, bool eager) : base(context)
        {
            clone = cloner ?? throw new ArgumentNullException(nameof(cloner)); this.eager = eager;
            if (source == null) throw new ArgumentNullException(nameof(source));
            lock (context.Gate) { instances = Prepare(context.Config, source); context.Attach(this); }
        }
        public T Primary { get { lock (Context.Gate) { Check(); return instances[0]!; } } }
        public int[] MaterializedNodes { get { lock (Context.Gate) { Check(); return Enumerable.Range(0, instances.Length).Where(n => instances[n] != null).ToArray(); } } }
        public T this[NumaReplicatedAccessToken token]
        {
            get
            {
                lock (Context.Gate)
                {
                    Check(); int node = token.NumaIndex;
                    if ((uint)node >= (uint)instances.Length) throw new ArgumentOutOfRangeException(nameof(token));
                    // The lock covers creation and publication. No unsynchronized null checks or partial copies.
                    return instances[node] ?? (instances[node] = CopyOnNode(Context.Config, node, instances[0]!));
                }
            }
        }
        private T CopyOnNode(NumaConfig config, int node, T source) => config.ExecuteOnNode(node, () => clone(source) ?? throw new InvalidOperationException("Cloner returned null."));
        private T?[] Prepare(NumaConfig config, T source)
        {
            var result = new T?[config.NodeCount];
            result[0] = config.RequiresMemoryReplication ? CopyOnNode(config, 0, source) : source;
            if (eager) for (int n = 1; n < result.Length; ++n) result[n] = CopyOnNode(config, n, result[0]!);
            return result;
        }
        internal override Action PrepareConfig(NumaConfig config)
        {
            Check(); var next = Prepare(config, instances[0]!); return () => instances = next;
        }
        public void Replace(T source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            lock (Context.Gate) { Check(); instances = Prepare(Context.Config, source); }
        }
        /// <summary>Stages a private copy; failed updates preserve every published replica.</summary>
        public void ModifyAndReplicate(Action<T> change)
        {
            if (change == null) throw new ArgumentNullException(nameof(change));
            lock (Context.Gate)
            {
                Check(); var next = CopyOnNode(Context.Config, 0, instances[0]!); change(next);
                // next was already allocated on node zero; do not copy it a second time.
                var staged = new T?[Context.Config.NodeCount]; staged[0] = next;
                if (eager) for (int n = 1; n < staged.Length; ++n) staged[n] = CopyOnNode(Context.Config, n, next);
                instances = staged;
            }
        }
        protected override void Release() => instances = Array.Empty<T?>();
    }
}
