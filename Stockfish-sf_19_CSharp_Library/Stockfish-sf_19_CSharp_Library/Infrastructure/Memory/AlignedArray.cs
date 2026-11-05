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
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>Value-initialized unmanaged elements with aligned/large-page allocation and deterministic release.</summary>
    public sealed unsafe class AlignedArray<T> : IDisposable where T : unmanaged
    {
        private readonly BufferOwner owner;
        public int Length { get; }
        public AlignedArray(int length, int alignment = 64, bool largePages = false)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length; owner = AlignedMemory.Allocate(checked(length * sizeof(T)), alignment, largePages);
        }
        public Span<T> Span => MemoryMarshal.Cast<byte, T>(owner.Bytes);
        public ref T this[int index] => ref Span[index];
        public IntPtr Address => owner.Address;
        public void Dispose() => owner.Dispose();
    }
}
