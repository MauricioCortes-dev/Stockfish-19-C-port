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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numa
{
    /// <summary>Linux sysfs/scheduler and Windows processor-group adapter. Startup affinity is captured once.</summary>
    public static class NumaTopology
    {
        private static bool Linux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));
        private static bool Windows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IntPtr.Size == 8;
        public static bool SupportsBinding => Linux || Windows;
        public static int HardwareConcurrency { get; } = DetectHardwareConcurrency();
        // Do not start/join threads inside a CLR type initializer. Windows multi-group
        // discovery uses a disposable querying thread and must run after type initialization.
        private static readonly Lazy<WindowsAffinity> windowsStartup = new Lazy<WindowsAffinity>(ReadWindowsAffinity);
        private static readonly Lazy<int[]?> startup = new Lazy<int[]?>(() => Linux ? GetCurrentThreadCpus() : Windows ? windowsStartup.Value.Combined : null);
        public static int MaximumThreads => Math.Max(1024, checked(4 * HardwareConcurrency));
        private static int DetectHardwareConcurrency()
        {
            if (Windows) return Math.Max(1, checked((int)GetActiveProcessorCount(ushort.MaxValue)));
            if (Linux)
            {
                string? online = Read("/sys/devices/system/cpu/online");
                if (online != null) { int n = Indices(online.Trim()).Count(); if (n != 0) return n; }
                long count = sysconf(84); // _SC_NPROCESSORS_ONLN in Linux/glibc and musl.
                if (count > 0 && count <= int.MaxValue) return (int)count;
            }
            return Math.Max(1, Environment.ProcessorCount);
        }
        private static IEnumerable<int> Indices(string text) => NumaConfig.ParseIndices(text).Select(c => checked((int)c));
        private static string? Read(string path)
        {
            try { return File.ReadAllText(path); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }
        public static int[] GetCurrentThreadCpus()
        {
            if (Linux)
            {
                // Grow dynamically for kernels built with a larger CONFIG_NR_CPUS.
                for (int bytes = 128; bytes <= (1 << 20); bytes *= 2)
                {
                    var mask = new byte[bytes];
                    if (sched_getaffinity(0, new UIntPtr((uint)bytes), mask) == 0) return DecodeMask(mask);
                    if (Marshal.GetLastWin32Error() != 22) throw new Win32Exception(Marshal.GetLastWin32Error(), "sched_getaffinity");
                }
                throw new InvalidOperationException("Kernel CPU affinity mask exceeds 8 million CPU identifiers.");
            }
            if (Windows)
            {
                try
                {
                    bool ok = GetThreadSelectedCpuSetMasks(GetCurrentThread(), null, 0, out ushort count);
                    if ((ok || Marshal.GetLastWin32Error() == 122) && count != 0)
                    {
                        var masks = new GroupAffinity[count];
                        if (!GetThreadSelectedCpuSetMasks(GetCurrentThread(), masks, count, out count)) throw new Win32Exception();
                        return masks.SelectMany(Cpus).OrderBy(c => c).ToArray();
                    }
                }
                catch (EntryPointNotFoundException) { }
                if (!GetThreadGroupAffinity(GetCurrentThread(), out GroupAffinity group)) throw new Win32Exception();
                return Cpus(group).ToArray();
            }
            return Enumerable.Range(0, HardwareConcurrency).ToArray();
        }
        private static int[] DecodeMask(byte[] mask)
        {
            var cpus = new List<int>();
            for (int i = 0; i < mask.Length; ++i)
                for (int b = 0; b < 8; ++b) if ((mask[i] & (1 << b)) != 0) cpus.Add(i * 8 + b);
            return cpus.ToArray();
        }
        internal static void Bind(IReadOnlyList<ulong> cpuIds)
        {
            // Freeze the inherited affinity before our first binding. A later hardware
            // lookup from a bound replica allocator must not redefine process startup state.
            _ = startup.Value;
            // Array/mask indexes are narrowed only at the OS boundary, after range validation.
            var cpus = cpuIds.Select(c => checked((int)c)).ToArray();
            if (Linux)
            {
                string? onlineText = Read("/sys/devices/system/cpu/online");
                if (onlineText != null)
                {
                    var online = new HashSet<int>(Indices(onlineText.Trim()));
                    if (cpus.Any(c => !online.Contains(c))) throw new ArgumentException("Affinity contains an offline or nonexistent CPU.", nameof(cpus));
                }
                int highest = cpus.Max();
                var mask = new byte[checked((highest / 64 + 1) * 8)];
                foreach (int c in cpus) mask[c / 8] |= (byte)(1 << (c % 8));
                if (sched_setaffinity(0, new UIntPtr((uint)mask.Length), mask) != 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "sched_setaffinity");
                sched_yield();
            }
            else if (Windows)
            {
                WindowsAffinity initial = windowsStartup.Value; // Capture before modifying this thread.
                var groups = cpus.GroupBy(c => c / 64).Select(g => new GroupAffinity { Group = checked((ushort)g.Key), Mask = g.Aggregate(0UL, (mask, c) => mask | (1UL << (c % 64))) }).ToArray();
                bool modern = false;
                try
                {
                    if (!SetThreadSelectedCpuSetMasks(GetCurrentThread(), groups, checked((ushort)groups.Length))) throw new Win32Exception();
                    modern = true;
                }
                catch (EntryPointNotFoundException) { }
                if (!modern || initial.LikelyUsedOldApi)
                {
                    if (groups.Length != 1) throw new PlatformNotSupportedException("Legacy Windows affinity cannot span processor groups.");
                    if (!SetThreadGroupAffinity(GetCurrentThread(), ref groups[0], IntPtr.Zero)) throw new Win32Exception();
                }
                SwitchToThread();
            }
            // As in the C++ fallback, unsupported hosts use logical domains without OS binding.
        }
        internal static NumaConfig Discover(NumaAutoPolicy policy, bool respect)
        {
            var system = new SortedDictionary<int, List<int>>();
            var initialAffinity = startup.Value;
            var allowed = respect && initialAffinity != null ? new HashSet<int>(initialAffinity) : null;
            bool Accept(int c) => allowed == null || allowed.Contains(c);
            void Add(int n, int c) { if (Accept(c)) { if (!system.TryGetValue(n, out var list)) system[n] = list = new List<int>(); list.Add(c); } }
            if (Linux)
            {
                string? online = Read("/sys/devices/system/node/online");
                bool complete = online != null;
                if (online != null)
                    foreach (int n in Indices(online.Trim()))
                    {
                        string? cpus = Read("/sys/devices/system/node/node" + n + "/cpulist");
                        if (cpus == null) { complete = false; break; }
                        foreach (int c in Indices(cpus.Trim())) Add(n, c);
                    }
                if (!complete) system.Clear();
            }
            else if (Windows)
            {
                for (ushort g = 0; g < GetActiveProcessorGroupCount(); ++g)
                    for (byte c = 0; c < 64; ++c)
                    {
                        var number = new ProcessorNumber { Group = g, Number = c };
                        if (GetNumaProcessorNodeEx(ref number, out ushort n) && n != ushort.MaxValue) Add(n, g * 64 + c);
                    }
            }
            if (system.Count == 0) foreach (int c in Enumerable.Range(0, HardwareConcurrency)) Add(0, c);
            // Fail instead of fabricating a CPU when an explicit process mask excludes every CPU.
            if (system.Count == 0) throw new InvalidOperationException("No CPU is available in the startup affinity.");
            NumaConfig config = new NumaConfig(system.Values, !respect);
            // C++ resolves L3 domains against a system configuration already split by
            // Windows processor groups. Bundling must not merge distinct primary groups.
            if (Windows) config = SplitProcessorGroups(config);
            if (policy.Kind != 0)
            {
                var cpuToNode = Enumerable.Range(0, config.NodeCount)
                    .SelectMany(n => config.CpusInNode(n).Select(c => (Cpu: checked((int)c), Node: n)))
                    .ToDictionary(p => p.Cpu, p => p.Node);
                var domains = new List<L3Domain>(); var seen = new HashSet<int>();
                if (Linux)
                    foreach (int cpu in cpuToNode.Keys.OrderBy(c => c))
                    {
                        if (seen.Contains(cpu)) continue;
                        string? siblings = Read("/sys/devices/system/cpu/cpu" + cpu + "/cache/index3/shared_cpu_list");
                        if (siblings == null) continue;
                        int[] list = Indices(siblings.Trim()).Where(cpuToNode.ContainsKey).ToArray();
                        foreach (int c in list) seen.Add(c);
                        if (list.Length != 0) domains.Add(new L3Domain(cpuToNode[list[list.Length - 1]], list));
                    }
                else if (Windows) domains = ReadWindowsL3(cpuToNode);
                // A partial sysfs snapshot must not silently lose processors.
                if (domains.Count != 0 && domains.SelectMany(d => d.Cpus).Distinct().Count() == config.CpuCount)
                {
                    var l3 = NumaConfig.FromL3Info(domains, policy.BundleSize);
                    config = new NumaConfig(Enumerable.Range(0, l3.NodeCount).Select(l3.CpusInNode), !respect);
                }
            }
            if (Windows) config = SplitProcessorGroups(config);
            return config;
        }
        internal static NumaConfig SplitProcessorGroups(NumaConfig config) => new NumaConfig(
            Enumerable.Range(0, config.NodeCount).SelectMany(n => config.CpusInNode(n)
                .GroupBy(c => c / 64).Select(g => (IEnumerable<ulong>)g)), config.CustomAffinity);
        private static IEnumerable<int> Cpus(GroupAffinity affinity)
        {
            for (int i = 0; i < 64; ++i) if ((affinity.Mask & (1UL << i)) != 0) yield return affinity.Group * 64 + i;
        }
        private sealed class WindowsAffinity
        {
            public int[]? Old, Modern; public bool OldDeterminate = true;
            public bool LikelyUsedOldApi => Old != null || !OldDeterminate;
            public int[]? Combined => Old == null ? Modern : Modern == null ? Old : Old.Intersect(Modern).ToArray();
        }
        private static WindowsAffinity ReadWindowsAffinity()
        {
            var result = new WindowsAffinity(); bool modernAvailable = true;
            try
            {
                bool ok = GetThreadSelectedCpuSetMasks(GetCurrentThread(), null, 0, out ushort count);
                if ((ok || Marshal.GetLastWin32Error() == 122) && count != 0)
                {
                    var masks = new GroupAffinity[count];
                    if (GetThreadSelectedCpuSetMasks(GetCurrentThread(), masks, count, out count)) result.Modern = masks.SelectMany(Cpus).ToArray();
                }
            }
            catch (EntryPointNotFoundException) { modernAvailable = false; }
            if (!GetProcessAffinityMask(GetCurrentProcess(), out UIntPtr process, out UIntPtr system) || process == UIntPtr.Zero) { result.OldDeterminate = false; return result; }
            ushort size = GetActiveProcessorGroupCount(); var groups = new ushort[size];
            if (!GetProcessGroupAffinity(GetCurrentProcess(), ref size, groups)) { result.OldDeterminate = false; return result; }
            if (size == 1)
            {
                if (GetActiveProcessorGroupCount() != 1 || process != system)
                    result.Old = Cpus(new GroupAffinity { Group = groups[0], Mask = process.ToUInt64() }).ToArray();
            }
            else if (modernAvailable)
            {
                // Query on a disposable thread: changing primary groups must not alter the host.
                using var thread = Infrastructure.Threading.NativeThread.Create(default, () =>
                {
                    var cpus = new List<int>(); bool full = true;
                    foreach (ushort group in groups.Take(size))
                    {
                        ulong p = ulong.MaxValue, s = ulong.MaxValue;
                        for (int i = 0; i < Math.Min(GetActiveProcessorCount(group), 2); ++i)
                        {
                            var affinity = new GroupAffinity { Group = group, Mask = 1UL << i };
                            if (!SetThreadGroupAffinity(GetCurrentThread(), ref affinity, IntPtr.Zero)) { result.OldDeterminate = false; return; }
                            SwitchToThread();
                            if (!GetProcessAffinityMask(GetCurrentProcess(), out UIntPtr pi, out UIntPtr si)) { result.OldDeterminate = false; return; }
                            p &= pi.ToUInt64(); s &= si.ToUInt64();
                        }
                        full &= p == s; cpus.AddRange(Cpus(new GroupAffinity { Group = group, Mask = p }));
                    }
                    if (!full) result.Old = cpus.ToArray();
                }); thread.RequireStarted(); thread.Join();
            }
            return result;
        }
        private static List<L3Domain> ReadWindowsL3(Dictionary<int, int> cpuToNode)
        {
            var result = new List<L3Domain>(); uint size = 0;
            GetLogicalProcessorInformationEx(2, IntPtr.Zero, ref size); // RelationCache
            if (size == 0 || Marshal.GetLastWin32Error() != 122) return result;
            IntPtr data = Marshal.AllocHGlobal(checked((int)size));
            try
            {
                if (!GetLogicalProcessorInformationEx(2, data, ref size)) return result;
                for (int offset = 0; offset + 8 <= size;)
                {
                    IntPtr entry = IntPtr.Add(data, offset); int bytes = Marshal.ReadInt32(entry, 4);
                    if (bytes < 8 || (long)offset + bytes > size) return new List<L3Domain>();
                    if (Marshal.ReadInt32(entry) == 2 && bytes >= 56 && Marshal.ReadByte(entry, 8) == 3)
                    {
                        // CACHE_RELATIONSHIP: GroupCount at 30; GroupMasks at 32, after the 8-byte header.
                        int groups = Math.Max(1, (int)(ushort)Marshal.ReadInt16(entry, 38));
                        if (40L + groups * 16L > bytes) return new List<L3Domain>();
                        var cpus = new List<int>();
                        for (int i = 0; i < groups; ++i)
                        {
                            var mask = Marshal.PtrToStructure<GroupAffinity>(IntPtr.Add(entry, 40 + i * 16));
                            cpus.AddRange(Cpus(mask).Where(cpuToNode.ContainsKey));
                        }
                        if (cpus.Count != 0) result.Add(new L3Domain(cpuToNode[cpus[0]], cpus));
                    }
                    offset += bytes;
                }
            }
            finally { Marshal.FreeHGlobal(data); }
            return result;
        }
        [StructLayout(LayoutKind.Sequential)] private struct GroupAffinity { public ulong Mask; public ushort Group, Reserved0, Reserved1, Reserved2; }
        [StructLayout(LayoutKind.Sequential)] private struct ProcessorNumber { public ushort Group; public byte Number, Reserved; }
        [DllImport("libc", SetLastError = true)] private static extern int sched_getaffinity(int pid, UIntPtr size, [Out] byte[] mask);
        [DllImport("libc", SetLastError = true)] private static extern int sched_setaffinity(int pid, UIntPtr size, byte[] mask);
        [DllImport("libc")] private static extern int sched_yield();
        [DllImport("libc")] private static extern long sysconf(int name);
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetProcessAffinityMask(IntPtr process, out UIntPtr mask, out UIntPtr system);
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetProcessGroupAffinity(IntPtr process, ref ushort count, [Out] ushort[] groups);
        [DllImport("kernel32")] private static extern ushort GetActiveProcessorGroupCount();
        [DllImport("kernel32")] private static extern uint GetActiveProcessorCount(ushort group);
        [DllImport("kernel32")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32")] private static extern IntPtr GetCurrentThread();
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetNumaProcessorNodeEx(ref ProcessorNumber cpu, out ushort node);
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetThreadGroupAffinity(IntPtr thread, out GroupAffinity affinity);
        [DllImport("kernel32", SetLastError = true)] private static extern bool SetThreadGroupAffinity(IntPtr thread, ref GroupAffinity affinity, IntPtr previous);
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetThreadSelectedCpuSetMasks(IntPtr thread, [Out] GroupAffinity[]? masks, ushort count, out ushort required);
        [DllImport("kernel32", SetLastError = true)] private static extern bool SetThreadSelectedCpuSetMasks(IntPtr thread, GroupAffinity[] masks, ushort count);
        [DllImport("kernel32", SetLastError = true)] private static extern bool GetLogicalProcessorInformationEx(int relation, IntPtr data, ref uint length);
        [DllImport("kernel32")] private static extern bool SwitchToThread();
    }
}
