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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Memory;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>One stable weight image; the layout contains values and padding, never CLR references.</summary>
    public sealed class WeightStorage : IDisposable
    {
        private BufferOwner? owner;
        private readonly object gate = new object();
        private bool disposed;
        public int Length { get; private set; }
        public bool IsReadOnly { get; }
        public WeightStorage() { }
        internal WeightStorage(BufferOwner owner, bool readOnly) { this.owner = owner; IsReadOnly = readOnly; }
        internal unsafe WeightBuffer<T> Add<T>(int count) where T : unmanaged
        {
            if (disposed) throw new ObjectDisposedException(nameof(WeightStorage));
            int offset = checked((int)AlignedMemory.AlignUp((ulong)Length, 64));
            Length = checked(offset + checked(count * sizeof(T)));
            if (owner != null && Length > owner.Length) throw new ArgumentException("Weight image is too short.");
            return new WeightBuffer<T>(this, offset, count);
        }
        private BufferOwner Owner
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(nameof(WeightStorage));
                if (Volatile.Read(ref owner) == null) lock (gate) { if (owner == null) Volatile.Write(ref owner, AlignedMemory.Allocate(Length, 64, true)); }
                return owner!;
            }
        }
        internal ReadOnlySpan<byte> Bytes => Owner.ReadOnlyBytes;
        internal Span<byte> WritableBytes => IsReadOnly ? throw new InvalidOperationException("Shared weights are immutable.") : Owner.Bytes;
        internal BufferOwner Retain() => Owner.Retain();
        public void Dispose() { if (disposed) return; disposed = true; owner?.Dispose(); owner = null; }
    }
}
