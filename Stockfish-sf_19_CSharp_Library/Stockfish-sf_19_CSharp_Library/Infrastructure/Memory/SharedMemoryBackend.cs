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
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>Real OS mappings; publication is serialized and readers map immutable bytes.</summary>
    public sealed class SharedMemoryBackend : IDisposable
    {
        internal BufferOwner? Owner { get; private set; }
        public SharedMemoryStatus Status { get; private set; } = SharedMemoryStatus.NotInitialized;
        public string? Error { get; private set; }
        public string Identity { get; }
        public string? BackingFile { get; private set; }
        public bool UsesLargePages { get; private set; }
        public bool IsValid => Owner != null && Status == SharedMemoryStatus.Success;
        public SharedMemoryBackend(string identity, int length, BufferInitializer initialize, string? unixDirectory = null)
        {
            Identity = StableHash(identity);
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (initialize == null) throw new ArgumentNullException(nameof(initialize));
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) OpenWindows(length, initialize);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Create("FREEBSD"))) OpenUnix(length, initialize, unixDirectory);
                else { Status = SharedMemoryStatus.Unsupported; Error = "Shared memory backend is unavailable on this OS."; return; }
                Status = SharedMemoryStatus.Success;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException || e is Win32Exception || e is ApplicationException || e is DllNotFoundException || e is EntryPointNotFoundException)
            {
                Owner?.Dispose(); Owner = null;
                if (Status == SharedMemoryStatus.NotInitialized) Status = SharedMemoryStatus.FileMappingError;
                Error = e.Message;
            }
        }
        public static string StableHash(string value)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            var text = new StringBuilder(64);
            foreach (byte b in bytes) text.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            return text.ToString();
        }
        // Library MVID identifies the actual managed engine build, rather than the common dotnet host.
        public static string ApplicationIdentity
        {
            get
            {
                try
                {
                    var assembly = typeof(SharedMemoryBackend).Assembly;
                    Guid version = assembly.ManifestModule.ModuleVersionId;
                    return version == Guid.Empty ? "" : version.ToString("N") + ":" + assembly.FullName;
                }
                catch (Exception error) when (error is NotSupportedException || error is MemberAccessException || error is TypeLoadException)
                { return ""; } // An unsupported metadata runtime must use local storage, never an ambiguous shared key.
            }
        }
        public static string GetLastErrorAsString(int error) => error == 0 ? "" : new Win32Exception(error).Message;
        private static unsafe BufferOwner View(MemoryMappedFile file, int length, MemoryMappedFileAccess access, Action? afterClose = null)
        {
            MemoryMappedViewAccessor? view = null;
            byte* pointer = null;
            try
            {
                view = file.CreateViewAccessor(0, length, access);
                view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                var held = view;
                return new BufferOwner(new IntPtr(pointer + view.PointerOffset), length, () =>
                {
                    held.SafeMemoryMappedViewHandle.ReleasePointer(); held.Dispose(); file.Dispose(); afterClose?.Invoke();
                }, access == MemoryMappedFileAccess.Read);
            }
            catch { if (pointer != null) view!.SafeMemoryMappedViewHandle.ReleasePointer(); view?.Dispose(); file.Dispose(); throw; }
        }
        private unsafe void OpenWindows(int length, BufferInitializer initialize)
        {
            string name = "Local\\sf_cs_" + Identity;
            Status = SharedMemoryStatus.MutexCreateError;
            using var mutex = new Mutex(false, name + "_init");
            bool held = false;
            IntPtr mapping = IntPtr.Zero, view = IntPtr.Zero;
            try
            {
                Status = SharedMemoryStatus.MutexWaitError;
                try { held = mutex.WaitOne(TimeSpan.FromSeconds(120)); } catch (AbandonedMutexException) { held = true; }
                if (!held) throw new IOException("Timed out waiting for the shared NNUE initializer.");
                Status = SharedMemoryStatus.FileMappingError;
                // Open an existing large-page image without needing the initializer's privilege.
                // Distinct suffixes identify the view flags, never per-process random identities.
                mapping = OpenFileMapping(6, false, name + "_large");
                UsesLargePages = mapping != IntPtr.Zero;
                if (mapping == IntPtr.Zero) mapping = OpenFileMapping(6, false, name + "_ordinary");
                if (mapping == IntPtr.Zero)
                {
                    mapping = AlignedMemory.WithLargePagePrivileges(page =>
                    {
                        ulong size = AlignedMemory.AlignUp((ulong)length + 64, page);
                        return CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x88000004,
                            (uint)(size >> 32), (uint)size, name + "_large");
                    }, () => IntPtr.Zero);
                    UsesLargePages = mapping != IntPtr.Zero;
                }
                if (mapping == IntPtr.Zero)
                    mapping = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 4, 0, checked((uint)length + 64), name + "_ordinary");
                if (mapping == IntPtr.Zero) throw NativeError("CreateFileMapping");
                BackingFile = name + (UsesLargePages ? "_large" : "_ordinary");
                uint largeFlag = UsesLargePages ? 0x20000000u : 0;
                ulong viewBytes = UsesLargePages ? AlignedMemory.AlignUp((ulong)length + 64, AlignedMemory.WindowsLargePageMinimum) : (ulong)length + 64;
                Status = SharedMemoryStatus.MapViewError;
                view = MapViewOfFile(mapping, 6 | largeFlag, 0, 0, new UIntPtr(viewBytes));
                if (view == IntPtr.Zero) throw NativeError("MapViewOfFile");
                byte* bytes = (byte*)view;
                long* header = (long*)(bytes + length);
                if (header[0] != length || header[1] != 0x53464e4e554531L)
                {
                    header[1] = 0;
                    Status = SharedMemoryStatus.InitializationError;
                    var payload = new Span<byte>(bytes, length); payload.Clear(); initialize(payload);
                    header[0] = length;
                    // Mutex release/acquire publishes the complete immutable image to peer processes.
                    header[1] = 0x53464e4e554531L;
                }
                UnmapViewOfFile(view); view = IntPtr.Zero;
                Status = SharedMemoryStatus.MapViewError;
                view = MapViewOfFile(mapping, 4 | largeFlag, 0, 0, new UIntPtr(viewBytes));
                if (view == IntPtr.Zero) throw NativeError("MapViewOfFile(read-only)");
                IntPtr retainedView = view, retainedMapping = mapping;
                Owner = new BufferOwner(view, length, () => { UnmapViewOfFile(retainedView); CloseHandle(retainedMapping); }, true);
                view = mapping = IntPtr.Zero;
            }
            finally
            {
                if (view != IntPtr.Zero) UnmapViewOfFile(view);
                if (mapping != IntPtr.Zero) CloseHandle(mapping);
                if (held)
                {
                    try { mutex.ReleaseMutex(); }
                    catch { Status = SharedMemoryStatus.MutexReleaseError; throw; }
                }
            }
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr file, IntPtr attributes, uint protection, uint high, uint low, string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenFileMapping(uint access, bool inherit, string name);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr mapping, uint access, uint high, uint low, UIntPtr size);
        [DllImport("kernel32.dll")] private static extern bool UnmapViewOfFile(IntPtr view);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
        private static unsafe void InitializeView(MemoryMappedViewAccessor view, int length, BufferInitializer initialize)
        {
            byte* p = null;
            try { view.SafeMemoryMappedViewHandle.AcquirePointer(ref p); var bytes = new Span<byte>(p + view.PointerOffset, length); bytes.Clear(); initialize(bytes); view.Flush(); }
            finally { if (p != null) view.SafeMemoryMappedViewHandle.ReleasePointer(); }
        }
        private void OpenUnix(int length, BufferInitializer initialize, string? directory)
        {
            string root = directory ?? Path.Combine("/tmp", "stockfish-cs-shm-" + geteuid());
            if (mkdir(root, 0x1c0) != 0 && Marshal.GetLastWin32Error() != 17) throw NativeError("mkdir");
            if ((System.IO.File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) throw new IOException("Shared memory directory must not be a symbolic link.");
            ValidateRoot(root);
            string prefix = Path.Combine(root, Identity);
            string data = prefix + ".weights", ready = prefix + ".ready";
            BackingFile = data;
            FileStream? live = null;
            bool success = false;
            using (var gate = OpenLock(Path.Combine(root, ".registry")))
            {
                Lock(gate, 2);
                try
                {
                    ReclaimAbandoned(root, prefix);
                    live = OpenLock(prefix + ".live");
                    bool alone = TryLock(live, 2 | 4);
                    if (alone)
                    {
                        System.IO.File.Delete(ready); System.IO.File.Delete(data);
                        using (var stream = new FileStream(data, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                        {
                            stream.SetLength(length);
                            // ftruncate alone can succeed on a full filesystem and later cause SIGBUS
                            // when touching the mapping. Reserve real blocks with bounded streaming IO first.
                            ReserveFileSpace(stream, length);
                            using var map = MemoryMappedFile.CreateFromFile(stream, null, length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
                            using var view = map.CreateViewAccessor(0, length, MemoryMappedFileAccess.ReadWrite);
                            Status = SharedMemoryStatus.InitializationError;
                            InitializeView(view, length, initialize);
                        }
                        System.IO.File.WriteAllText(ready, length.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        if (!System.IO.File.Exists(ready) || System.IO.File.ReadAllText(ready) != length.ToString(System.Globalization.CultureInfo.InvariantCulture))
                            throw new IOException("Active shared mapping has invalid publication metadata.");
                    }
                    Lock(live, 1); // Keep a shared lifetime lock until the last local lease is released.
                    var input = new FileStream(data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    if (input.Length != length) { input.Dispose(); throw new IOException("Shared mapping has an invalid length."); }
                    MemoryMappedFile? readMap = null;
                    try
                    {
                        readMap = MemoryMappedFile.CreateFromFile(input, null, length, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
                        var retained = live;
                        Owner = View(readMap, length, MemoryMappedFileAccess.Read, () => Retire(prefix, retained));
                        readMap = null; live = null; success = true;
                    }
                    finally { readMap?.Dispose(); if (!success) input.Dispose(); }
                }
                finally
                {
                    live?.Dispose();
                    if (!success)
                    {
                        // A peer may own a valid image when joining failed: only reclaim unlocked entries.
                        ReclaimAbandoned(root, null);
                    }
                    // No publication survives a failed first initializer; the next opener retries from scratch.
                    Lock(gate, 8);
                }
            }
        }
        private static void Retire(string prefix, FileStream live)
        {
            try
            {
                using var gate = OpenLock(Path.Combine(Path.GetDirectoryName(prefix)!, ".registry")); Lock(gate, 2);
                try
                {
                    Lock(live, 8);
                    if (TryLock(live, 2 | 4))
                    {
                        System.IO.File.Delete(prefix + ".ready"); System.IO.File.Delete(prefix + ".weights");
                        // All opens/retirements hold the registry lock, so unlinking cannot split lock identity.
                        System.IO.File.Delete(prefix + ".live");
                    }
                }
                finally { Lock(gate, 8); }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is Win32Exception) { /* Kernel closes mappings/locks even if cache cleanup fails. */ }
            finally { live.Dispose(); }
        }
        private static void ReclaimAbandoned(string root, string? except)
        {
            foreach (string path in Directory.EnumerateFiles(root, "*.live"))
            {
                string prefix = path.Substring(0, path.Length - 5);
                if (prefix == except) continue;
                using var live = OpenLock(path);
                if (TryLock(live, 2 | 4))
                {
                    System.IO.File.Delete(prefix + ".ready"); System.IO.File.Delete(prefix + ".weights"); System.IO.File.Delete(path);
                }
            }
        }
        internal static void ReserveFileSpace(Stream stream, int length)
        {
            byte[] zeros = new byte[Math.Min(65536, length)];
            for (int remaining = length; remaining > 0;)
            {
                int count = Math.Min(zeros.Length, remaining);
                stream.Write(zeros, 0, count); remaining -= count;
            }
            if (stream is FileStream file) file.Flush(true);
            else stream.Flush();
        }
        private static FileStream OpenLock(string path)
        {
            // open(O_NOFOLLOW|O_CREAT) prevents lock/data substitution through symbolic links.
            // Linux flags differ from BSD; other Unix uses managed open plus a link check.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                int fd = open(path, 2 | 0x40 | 0x20000 | 0x80000, 0x180);
                if (fd < 0) throw NativeError("open lock");
                var handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(fd), true);
                try { return new FileStream(handle, FileAccess.ReadWrite); }
                catch { handle.Dispose(); throw; }
            }
            if (System.IO.File.Exists(path) && (System.IO.File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new IOException("Symbolic lock file.");
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
        }
        private static void ValidateRoot(string root)
        {
            // stat ABI is explicitly selected; unknown ABIs safely fall back to local storage.
            bool linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool mac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            bool arm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
            if (IntPtr.Size != 8 || (!linux && !mac) || (linux && !arm64 && RuntimeInformation.ProcessArchitecture != Architecture.X64)) throw new PlatformNotSupportedException("Unsupported Unix stat ABI; local NNUE fallback.");
            byte[] metadata = new byte[256];
            if (lstat(root, metadata) != 0) throw NativeError("lstat");
            uint mode = linux ? BitConverter.ToUInt32(metadata, arm64 ? 16 : 24) : BitConverter.ToUInt16(metadata, 4);
            uint uid = BitConverter.ToUInt32(metadata, linux ? (arm64 ? 24 : 28) : 16);
            if ((mode & 0xf000) != 0x4000 || (mode & 0xfff) != 0x1c0 || uid != geteuid())
                throw new IOException("Shared memory directory must be owned by the current user with mode 0700.");
        }
        private static bool TryLock(FileStream stream, int operation)
        {
            int result;
            do { result = flock(stream.SafeFileHandle.DangerousGetHandle().ToInt32(), operation); } while (result != 0 && Marshal.GetLastWin32Error() == 4);
            if (result == 0) return true;
            int error = Marshal.GetLastWin32Error();
            if ((operation & 4) != 0 && (error == 11 || error == 35)) return false;
            throw NativeError("flock");
        }
        private static void Lock(FileStream stream, int operation) { TryLock(stream, operation); }
        private static IOException NativeError(string operation) => new IOException(operation + ": " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
        public void Dispose() { Owner?.Dispose(); Owner = null; Status = SharedMemoryStatus.NotInitialized; }
        [DllImport("libc", SetLastError = true)] private static extern int mkdir(string path, uint mode);
        [DllImport("libc", SetLastError = true)] private static extern int open(string path, int flags, uint mode);
        [DllImport("libc", SetLastError = true)] private static extern int flock(int fd, int operation);
        [DllImport("libc", SetLastError = true)] private static extern int lstat(string path, [Out] byte[] metadata);
        [DllImport("libc")] private static extern uint geteuid();
    }
}
