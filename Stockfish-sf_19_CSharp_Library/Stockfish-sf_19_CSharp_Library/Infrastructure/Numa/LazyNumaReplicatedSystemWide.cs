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
using Stockfish_sf_19_CSharp_Library.Infrastructure.Memory;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numa
{
    /// <summary>Immutable system-wide replicas, materialized on the target node and deduplicated by physical domain.</summary>
    /// <remarks>The host must retire workers before mutation. Readers requiring a longer lifetime must acquire
    /// their own lease from T. The shared factory must not reenter this context from its allocation thread.</remarks>
    public sealed class LazyNumaReplicatedSystemWide<T> : NumaReplicatedBase where T : class, IDisposable
    {
        private readonly Func<T, string, T> share;
        private readonly Func<T, T> copy;
        private T?[] instances = Array.Empty<T?>();
        private T?[]? prepared;
        public LazyNumaReplicatedSystemWide(NumaReplicationContext context, T source, Func<T, string, T> sharedFactory, Func<T, T> privateCopy) : base(context)
        {
            share = sharedFactory ?? throw new ArgumentNullException(nameof(sharedFactory));
            copy = privateCopy ?? throw new ArgumentNullException(nameof(privateCopy));
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
                    return instances[node] ?? (instances[node] = OnNode(Context.Config, node, instances[0]!));
                }
            }
        }
        public static string PhysicalDiscriminator(NumaConfig logical, int node)
        {
            var system = NumaConfig.FromSystem(NumaAutoPolicy.System, false);
            ulong cpu = logical.CpusInNode(node)[0]; int physical = 0;
            for (int n = 0; n < system.NodeCount; ++n) if (system.CpusInNode(n).Contains(cpu)) { physical = n; break; }
            return system + "$" + physical.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        private T OnNode(NumaConfig config, int node, T source) => config.ExecuteOnNode(node, () => share(source, PhysicalDiscriminator(config, node)) ?? throw new InvalidOperationException("Shared factory returned null."));
        private T?[] Prepare(NumaConfig config, T source)
        {
            var result = new T?[config.NodeCount]; result[0] = OnNode(config, 0, source); return result;
        }
        private static void Retire(T?[] values) { foreach (var value in values) value?.Dispose(); }
        internal override Action PrepareConfig(NumaConfig config)
        {
            Check(); prepared = Prepare(config, instances[0]!);
            return () => { var old = instances; instances = prepared!; prepared = null; Retire(old); };
        }
        internal override void CancelPreparedConfig() { if (prepared != null) Retire(prepared); prepared = null; }
        public void Replace(T source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            lock (Context.Gate) { Check(); var next = Prepare(Context.Config, source); var old = instances; instances = next; Retire(old); }
        }
        public void ModifyAndReplicate(Action<T> change)
        {
            if (change == null) throw new ArgumentNullException(nameof(change));
            lock (Context.Gate) { Check(); using var source = copy(instances[0]!); change(source); Replace(source); }
        }
        public (SystemWideSharedConstantAllocationStatus Status, string? Error)[] GetStatusAndErrors(Func<T, (SystemWideSharedConstantAllocationStatus, string?)> status)
        {
            lock (Context.Gate) { Check(); return instances.Select(value => value == null ? (SystemWideSharedConstantAllocationStatus.NoAllocation, (string?)null) : status(value)).ToArray(); }
        }
        protected override void Release() { CancelPreparedConfig(); Retire(instances); instances = Array.Empty<T?>(); }
    }
}
