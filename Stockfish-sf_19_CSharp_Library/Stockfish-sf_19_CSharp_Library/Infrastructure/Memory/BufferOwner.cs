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
using System.Threading;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>A deterministic reference-counted lease on stable native or mapped bytes.</summary>
    /// <remarks>Spans are borrowed until this lease is disposed. Retain a separate lease across
    /// owner retirement. Neither Dispose nor mutation may race a borrow on the same lease.</remarks>
    public sealed unsafe class BufferOwner : IDisposable
    {
        private sealed class Resource
        {
            internal readonly IntPtr Pointer;
            internal readonly int Length;
            internal readonly Action Release;
            internal readonly bool ReadOnly;
            internal int References = 1;
            internal Resource(IntPtr pointer, int length, Action release, bool readOnly)
            { Pointer = pointer; Length = length; Release = release; ReadOnly = readOnly; }
        }
        private Resource? resource;
        private readonly bool readOnlyLease;
        internal BufferOwner(IntPtr pointer, int length, Action release, bool readOnly = false)
        { resource = new Resource(pointer, length, release, readOnly); }
        private BufferOwner(Resource value, bool readOnly) { resource = value; readOnlyLease = readOnly; }
        public int Length => Get().Length;
        public IntPtr Address => Get().Pointer;
        private Resource Get() => Volatile.Read(ref resource) ?? throw new ObjectDisposedException(nameof(BufferOwner));
        public ReadOnlySpan<byte> ReadOnlyBytes { get { var r = Get(); return new ReadOnlySpan<byte>((void*)r.Pointer, r.Length); } }
        public Span<byte> Bytes
        {
            get
            {
                var r = Get();
                if (r.ReadOnly || readOnlyLease) throw new InvalidOperationException("This buffer lease is read-only.");
                return new Span<byte>((void*)r.Pointer, r.Length);
            }
        }
        public BufferOwner Retain() => Retain(readOnlyLease);
        public BufferOwner RetainReadOnly() => Retain(true);
        private BufferOwner Retain(bool readOnly)
        {
            var r = Get();
            // A lease is retained only while its current owner is still held by the caller.
            int count = Volatile.Read(ref r.References);
            while (count > 0)
            {
                int observed = Interlocked.CompareExchange(ref r.References, checked(count + 1), count);
                if (observed == count)
                {
                    try { return new BufferOwner(r, readOnly); }
                    catch { if (Interlocked.Decrement(ref r.References) == 0) r.Release(); throw; }
                }
                count = observed;
            }
            throw new ObjectDisposedException(nameof(BufferOwner));
        }
        public void Dispose() { Release(); GC.SuppressFinalize(this); }
        private void Release()
        {
            var r = Interlocked.Exchange(ref resource, null);
            if (r != null && Interlocked.Decrement(ref r.References) == 0) r.Release();
        }
        ~BufferOwner() { Release(); }
    }
}
