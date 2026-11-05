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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>Typed, immutable view factory replaces C++ placement-copy of inline trivially-copyable T.</summary>
    /// <remarks>The factory takes ownership of its buffer lease. The payload must contain no CLR references.</remarks>
    public sealed class SystemWideSharedConstant<T> : IDisposable where T : class, IDisposable
    {
        private SharedMemoryBackend? backend;
        private BufferOwner? local;
        private T? value;
        public T Value => value ?? throw new ObjectDisposedException(nameof(SystemWideSharedConstant<T>));
        public SystemWideSharedConstantAllocationStatus Status => value == null ? SystemWideSharedConstantAllocationStatus.NoAllocation : backend != null ? SystemWideSharedConstantAllocationStatus.SharedMemory : SystemWideSharedConstantAllocationStatus.LocalMemory;
        public string? Error { get; private set; }
        public string Identity { get; } = "";
        public string? BackingFile => backend?.BackingFile;
        public SystemWideSharedConstant() { }
        public SystemWideSharedConstant(string contentIdentity, string layoutIdentity, string discriminator, int length,
            BufferInitializer initialize, Func<BufferOwner, T> view, string? unixDirectory = null, bool enableSharing = true)
        {
            string application = SharedMemoryBackend.ApplicationIdentity;
            if (application.Length == 0) { enableSharing = false; Error = "Stable application identity unavailable. Local allocation fallback."; }
            string key = contentIdentity + "$" + application + "$" + layoutIdentity + "$" + discriminator + "$" + length;
            Identity = SharedMemoryBackend.StableHash(key);
            try
            {
                if (enableSharing)
                {
                    backend = new SharedMemoryBackend(key, length, initialize, unixDirectory);
                    if (!backend.IsValid) { Error = backend.Status + ": " + backend.Error; backend.Dispose(); backend = null; }
                }
                else Error = Error ?? "Shared memory disabled by host. Local allocation fallback.";
                if (backend == null)
                {
                    local = AlignedMemory.Allocate(length, 64, true); initialize(local.Bytes);
                    Error = Error ?? "Shared memory unavailable. Local allocation fallback.";
                }
                BufferOwner lease = (backend?.Owner ?? local)!.RetainReadOnly();
                try { value = view(lease); } catch { lease.Dispose(); throw; }
            }
            catch { Dispose(); throw; }
        }
        public void Dispose() { value?.Dispose(); value = null; backend?.Dispose(); backend = null; local?.Dispose(); local = null; }
    }
}
