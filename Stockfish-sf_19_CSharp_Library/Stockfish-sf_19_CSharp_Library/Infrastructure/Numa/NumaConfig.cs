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
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numa
{
    /// <summary>Immutable logical topology. CPU identifiers retain their operating system numbering.</summary>
    public sealed class NumaConfig
    {
        private readonly ulong[][] nodes;
        public int NodeCount => nodes.Length;
        public int CpuCount { get; }
        public bool CustomAffinity { get; }
        public bool RequiresMemoryReplication => CustomAffinity || NodeCount > 1;
        public NumaConfig() : this(new[] { Enumerable.Range(0, NumaTopology.HardwareConcurrency) }, false) { }
        internal NumaConfig(IEnumerable<IEnumerable<int>> values, bool custom)
            : this(values.Select(node => node.Select(cpu => checked((ulong)cpu))), custom) { }
        internal NumaConfig(IEnumerable<IEnumerable<ulong>> values, bool custom)
        {
            var seen = new HashSet<ulong>();
            nodes = values.Select(v => v.OrderBy(c => c).ToArray()).Where(v => v.Length != 0).ToArray();
            if (nodes.Length == 0) throw new ArgumentException("Topology has no available processors.");
            foreach (ulong c in nodes.SelectMany(n => n))
                if ((UIntPtr.Size == 4 && c > uint.MaxValue) || !seen.Add(c)) throw new ArgumentException("CPU assigned more than once or invalid CPU.");
            CpuCount = seen.Count; CustomAffinity = custom;
        }
        public IReadOnlyList<ulong> CpusInNode(int index) => Array.AsReadOnly(nodes[index]);
        public int CpuCountInNode(NumaIndex index) => CpuCountInNode((int)index);
        public int CpuCountInNode(int index) => nodes[index].Length;
        public bool IsCpuAssigned(ulong cpu) => nodes.Any(n => Array.BinarySearch(n, cpu) >= 0);
        public static NumaConfig FromSystem(NumaAutoPolicy policy, bool respectProcessAffinity = true) => NumaTopology.Discover(policy, respectProcessAffinity);
        // Like C++, invalid components are ignored, but assigning any CPU twice rejects the configuration.
        internal static IEnumerable<ulong> ParseIndices(string value)
        {
            foreach (string item in value.Split(','))
            {
                string[] parts = item.Split('-');
                if (parts.Length == 1 && ParseIndex(parts[0], out ulong one)) yield return one;
                else if (parts.Length == 2 && ParseIndex(parts[0], out ulong first) && ParseIndex(parts[1], out ulong last) && last >= first && last - first < (1 << 20))
                    for (ulong i = first; ; ++i) { yield return i; if (i == last) break; }
            }
        }
        private static bool ParseIndex(string s, out ulong value)
        {
            value = 0;
            if (s.Length == 0 || s[0] == '-') return false;
            bool Space(char c) => " \t\r\n\v\f".IndexOf(c) >= 0;
            int i = 0; while (i < s.Length && Space(s[i])) ++i;
            int first = i;
            if (i < s.Length && (s[i] == '+' || s[i] == '-')) ++i;
            int digits = i; while (i < s.Length && s[i] >= '0' && s[i] <= '9') ++i;
            // strtoull leaves endptr at the original start when no digits were consumed.
            if (i == digits) return Space(s[0]);
            if (i < s.Length && !Space(s[i])) return false;
            return ulong.TryParse(s.Substring(first, i - first), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value) && (UIntPtr.Size == 8 || value <= uint.MaxValue);
        }
        public static NumaConfig? FromString(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            try
            {
                var parsed = value.Split(':').Select(n => ParseIndices(n).ToArray()).Where(n => n.Length != 0).ToArray();
                return parsed.Length == 0 ? null : new NumaConfig(parsed, true);
            }
            catch (ArgumentException) { return null; }
        }
        public override string ToString()
        {
            var text = new StringBuilder();
            foreach (var n in nodes)
            {
                if (text.Length != 0) text.Append(':');
                for (int i = 0; i < n.Length; ++i)
                {
                    if (i != 0) text.Append(',');
                    ulong start = n[i];
                    text.Append(start.ToString(CultureInfo.InvariantCulture));
                    while (i + 1 < n.Length && n[i] != ulong.MaxValue && n[i + 1] == n[i] + 1) ++i;
                    if (start != n[i]) text.Append('-').Append(n[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            return text.ToString();
        }
        public bool SuggestsBindingThreads(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (CustomAffinity) return true;
            if (count <= 1) return false;
            int largest = nodes.Max(n => n.Length), notSmall = nodes.Count(n => (double)n.Length / largest > 0.6);
            return (count > largest / 2 || count >= notSmall * 4) && NodeCount > 1;
        }
        public int[] DistributeThreads(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            var result = new int[count]; var occupation = new int[NodeCount];
            for (int i = 0; i < count; ++i)
            {
                int best = 0; float fill = float.MaxValue;
                for (int n = 0; n < NodeCount; ++n)
                {
                    float candidate = (float)(occupation[n] + 1) / nodes[n].Length;
                    if (candidate < fill) { best = n; fill = candidate; }
                }
                result[i] = best; ++occupation[best];
            }
            return result;
        }
        public NumaReplicatedAccessToken BindCurrentThread(int node)
        {
            NumaTopology.Bind(nodes[node]);
            return new NumaReplicatedAccessToken(node);
        }
        public void ExecuteOnNode(int node, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            ExecuteOnNode(node, () => { action(); return 0; });
        }
        /// <summary>Run allocation on a temporary bound thread; never alters the caller's affinity.</summary>
        public T ExecuteOnNode<T>(int node, Func<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _ = nodes[node]; T result = default!; ExceptionDispatchInfo? failure = null;
            using var thread = Infrastructure.Threading.NativeThread.Create(default, () =>
            {
                try { BindCurrentThread(node); result = action(); }
                catch (Exception e) { failure = ExceptionDispatchInfo.Capture(e); }
            });
            thread.RequireStarted(); thread.Join(); failure?.Throw(); return result;
        }
        public static NumaConfig FromL3Info(IEnumerable<L3Domain> domains, int bundleSize)
        {
            if (domains == null) throw new ArgumentNullException(nameof(domains));
            if (bundleSize < 0) throw new ArgumentOutOfRangeException(nameof(bundleSize));
            var output = new List<IEnumerable<int>>();
            foreach (var group in domains.GroupBy(d => d.SystemNumaIndex).OrderBy(g => g.Key))
            {
                var list = group.Select(d => new SortedSet<int>(d.Cpus)).ToList();
                bool changed;
                do
                {
                    changed = false;
                    for (int j = 0; j + 1 < list.Count; ++j)
                        if ((long)list[j].Count + list[j + 1].Count <= bundleSize)
                        { list[j].UnionWith(list[j + 1]); list.RemoveAt(j + 1); changed = true; }
                } while (changed);
                output.AddRange(list);
            }
            return new NumaConfig(output, false);
        }
    }
}
