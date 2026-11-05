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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>Stable, zero-initialized allocations. Large pages are hints with ordinary-page fallback.</summary>
    public static unsafe class AlignedMemory
    {
        public const long HugePageSize = 1L << 30;
        public static void ReportFailedAllocation(long bytes) => throw new OutOfMemoryException("Failed to allocate " + bytes.ToString(System.Globalization.CultureInfo.InvariantCulture) + " bytes.");
        public static ulong AlignUp(ulong value, ulong alignment)
        {
            if (alignment == 0 || (alignment & (alignment - 1)) != 0) throw new ArgumentOutOfRangeException(nameof(alignment));
            return checked(value + alignment - 1) & ~(alignment - 1);
        }
        public static T LoadAs<T>(ReadOnlySpan<byte> bytes) where T : unmanaged => MemoryMarshal.Read<T>(bytes);
        public static BufferOwner Allocate(int length, int alignment = 64, bool largePages = false, bool hugePageHint = false)
            => Allocate(length, alignment, largePages, hugePageHint, null);

        internal static BufferOwner Allocate(int length, int alignment, bool largePages, bool hugePageHint, Action<BufferOwner>? beforePublish)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (alignment <= 0) throw new ArgumentOutOfRangeException(nameof(alignment));
            AlignUp(0, (ulong)alignment);
            BufferOwner? result = null;
            if (largePages && alignment <= 4096 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                long size = checked((long)AlignUp((ulong)Math.Max(length, 1), 2 * 1024 * 1024));
                IntPtr p = new IntPtr(-1);
                if (hugePageHint && length >= HugePageSize && IntPtr.Size == 8)
                {
                    size = (long)AlignUp((ulong)length, (ulong)HugePageSize);
                    p = mmap(IntPtr.Zero, new UIntPtr((ulong)size), 3, 0x22 | 0x40000 | (30 << 26), -1, IntPtr.Zero);
                }
                if (p == new IntPtr(-1))
                {
                    size = (long)AlignUp((ulong)Math.Max(length, 1), 2 * 1024 * 1024);
                    p = MapHugeAligned(size, 0x22, -1);
                    if (p != new IntPtr(-1)) madvise(p, new UIntPtr((ulong)size), 14);
                }
                if (p != new IntPtr(-1))
                {
                    try
                    {
                        IntPtr allocation = p; long allocated = size;
                        result = new BufferOwner(p, length, () => munmap(allocation, new UIntPtr((ulong)allocated)));
                    }
                    catch { munmap(p, new UIntPtr((ulong)size)); throw; }
                }
            }
            else if (largePages && alignment <= 4096 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr p = WithLargePagePrivileges(page => VirtualAlloc(IntPtr.Zero,
                    new UIntPtr(AlignUp((ulong)Math.Max(length, 1), page)), 0x20003000, 4), () => IntPtr.Zero);
                if (p == IntPtr.Zero) p = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)Math.Max(length, 1)), 0x3000, 4);
                if (p != IntPtr.Zero)
                {
                    try { var allocation = p; result = new BufferOwner(p, length, () => VirtualFree(allocation, UIntPtr.Zero, 0x8000)); }
                    catch { VirtualFree(p, UIntPtr.Zero, 0x8000); throw; }
                }
            }
            if (result == null)
            {
                IntPtr raw = Marshal.AllocHGlobal(checked(Math.Max(length, 1) + alignment - 1));
                try
                {
                    IntPtr aligned = new IntPtr(unchecked((long)AlignUp(unchecked((ulong)raw.ToInt64()), (ulong)alignment)));
                    result = new BufferOwner(aligned, length, () => Marshal.FreeHGlobal(raw));
                    result.Bytes.Clear();
                }
                catch { if (result == null) Marshal.FreeHGlobal(raw); else result.Dispose(); throw; }
            }
            bool charged = false;
            try
            {
                if (length > 0) { GC.AddMemoryPressure(length); charged = true; }
                beforePublish?.Invoke(result); // Instance-local failure injection; never enabled by the public API.
                BufferOwner allocationOwner = result;
                // One pressure charge per allocation, independent of the number of retained leases.
                return new BufferOwner(result.Address, length, () =>
                {
                    allocationOwner.Dispose();
                    if (length > 0) GC.RemoveMemoryPressure(length);
                });
            }
            catch
            {
                result.Dispose();
                if (charged) GC.RemoveMemoryPressure(length);
                throw;
            }
        }
        public static bool HasLargePages => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && WithLargePagePrivileges(page =>
            {
                IntPtr block = VirtualAlloc(IntPtr.Zero, new UIntPtr(page), 0x20003000, 4);
                if (block == IntPtr.Zero) return false;
                VirtualFree(block, UIntPtr.Zero, 0x8000); return true;
            }, () => false));
        internal static IntPtr MapHugeAligned(long size, int flags, int fd)
        {
            const ulong alignment = 2 * 1024 * 1024;
            long rounded = (long)AlignUp((ulong)size, (ulong)Environment.SystemPageSize);
            if (size >= (long)alignment)
            {
                long total = checked(rounded + (long)alignment);
                IntPtr reservation = mmap(IntPtr.Zero, new UIntPtr((ulong)total), 0, 0x22, -1, IntPtr.Zero);
                if (reservation != new IntPtr(-1))
                {
                    long address = (long)AlignUp((ulong)reservation.ToInt64(), alignment);
                    IntPtr mapped = mmap(new IntPtr(address), new UIntPtr((ulong)size), 3, flags | 0x10, fd, IntPtr.Zero);
                    if (mapped != new IntPtr(-1))
                    {
                        long prefix = address - reservation.ToInt64(), suffix = total - prefix - rounded;
                        if (prefix != 0) munmap(reservation, new UIntPtr((ulong)prefix));
                        if (suffix != 0) munmap(new IntPtr(address + rounded), new UIntPtr((ulong)suffix));
                        return mapped;
                    }
                    munmap(reservation, new UIntPtr((ulong)total));
                }
            }
            return mmap(IntPtr.Zero, new UIntPtr((ulong)size), 3, flags, fd, IntPtr.Zero);
        }
        // Serializes process-token privilege changes and always restores/closes even on exceptions.
        internal static ulong WindowsLargePageMinimum => GetLargePageMinimum().ToUInt64();
        private static readonly object privilegeGate = new object();
        internal static T WithLargePagePrivileges<T>(Func<ulong, T> yes, Func<T> no)
        {
            lock (privilegeGate)
            {
                ulong page = GetLargePageMinimum().ToUInt64();
                if (IntPtr.Size != 8 || page == 0 || !OpenProcessToken(GetCurrentProcess(), 0x28, out IntPtr token)) return no();
                try
                {
                    if (!LookupPrivilegeValue(null, "SeLockMemoryPrivilege", out long luid)) return no();
                    var requested = new TokenPrivileges { Count = 1, Luid = luid, Attributes = 2 };
                    if (!AdjustTokenPrivileges(token, false, ref requested, Marshal.SizeOf<TokenPrivileges>(), out var previous, out int _) || Marshal.GetLastWin32Error() != 0) return no();
                    try { return yes(page); }
                    finally { AdjustTokenPrivileges(token, false, ref previous, 0, out var _, out int _); }
                }
                finally { CloseHandle(token); }
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 4)] private struct TokenPrivileges { internal int Count; internal long Luid; internal int Attributes; }
        [DllImport("libc", SetLastError = true)] internal static extern IntPtr mmap(IntPtr address, UIntPtr size, int protection, int flags, int fd, IntPtr offset);
        [DllImport("libc", SetLastError = true)] internal static extern int munmap(IntPtr address, UIntPtr size);
        [DllImport("libc")] private static extern int madvise(IntPtr address, UIntPtr size, int advice);
        [DllImport("kernel32.dll")] private static extern UIntPtr GetLargePageMinimum();
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr VirtualAlloc(IntPtr address, UIntPtr size, uint type, uint protection);
        [DllImport("kernel32.dll")] private static extern bool VirtualFree(IntPtr address, UIntPtr size, uint type);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool LookupPrivilegeValue(string? system, string name, out long luid);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool AdjustTokenPrivileges(IntPtr token, bool disable, ref TokenPrivileges value, int size, out TokenPrivileges previous, out int returned);
    }
}
