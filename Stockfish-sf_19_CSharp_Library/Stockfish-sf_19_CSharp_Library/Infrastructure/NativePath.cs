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
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Native filesystem path: arbitrary Unix bytes or a decoded Windows path.
    /// Owns its spelling, not an open handle. Opening transfers a fresh descriptor to FileStream.</summary>
    public sealed class NativePath
    {
        private readonly byte[] bytes;
        private readonly string? windows;
        private NativePath(ReadOnlySpan<byte> source)
        {
            if (source.IndexOf((byte)0) >= 0) throw new ArgumentException("A native path cannot contain NUL.", nameof(source));
            bytes = source.ToArray();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) windows = MiscUtilities.PathFromUtf8(source);
        }
        public static NativePath FromUtf8(ReadOnlySpan<byte> source) => new NativePath(source);
        public byte[] GetNativeBytes() => (byte[])bytes.Clone();
        /// <summary>A strict Unicode projection; use OpenRead for Unix byte paths that are not UTF-8.</summary>
        public string UnicodePath => windows ?? new UTF8Encoding(false, true).GetString(bytes);
        public FileStream OpenRead()
        {
            if (windows != null) return new FileStream(windows, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (!(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                || RuntimeInformation.IsOSPlatform(OSPlatform.Create("FREEBSD"))))
                return new FileStream(UnicodePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            // open's variadic mode argument is unused for O_RDONLY; O_CLOEXEC is atomic with open.
            int cloexec = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 0x80000
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 0x1000000 : 0x100000;
            byte[] terminated = new byte[checked(bytes.Length + 1)]; bytes.CopyTo(terminated, 0);
            int fd;
            do { fd = Open(terminated, cloexec); } while (fd == -1 && Marshal.GetLastWin32Error() == 4); // EINTR
            if (fd < 0) throw new IOException("Cannot open native path (errno " + Marshal.GetLastWin32Error() + ").");
            var owner = new SafeFileHandle(new IntPtr(fd), true);
            try { return new FileStream(owner, FileAccess.Read, 4096, false); }
            catch { owner.Dispose(); throw; }
        }
        public override string ToString()
        {
            try { return UnicodePath; }
            catch (DecoderFallbackException) { return "Unix bytes: " + BitConverter.ToString(bytes); }
        }
        [DllImport("libc", EntryPoint="open", SetLastError=true)]
        private static extern int Open(byte[] path, int flags);
    }
}
