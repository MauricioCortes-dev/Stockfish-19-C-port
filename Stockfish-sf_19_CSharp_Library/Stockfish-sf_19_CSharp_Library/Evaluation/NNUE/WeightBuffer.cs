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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Typed view on a weight image. Mutable spans are available only on local writable networks.</summary>
    public sealed class WeightBuffer<T> : IReadOnlyList<T> where T : unmanaged
    {
        private readonly WeightStorage storage;
        private readonly int offset;
        public int Length { get; }
        public int Count => Length;
        internal WeightBuffer(WeightStorage storage, int offset, int length) { this.storage = storage; this.offset = offset; Length = length; }
        public T this[int index] { get => AsReadOnlySpan()[index]; set => AsSpan()[index] = value; }
        public unsafe ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.Cast<byte, T>(storage.Bytes.Slice(offset, checked(Length * sizeof(T))));
        public unsafe Span<T> AsSpan()
        {
            if (storage.IsReadOnly) throw new InvalidOperationException("Shared NNUE weights are immutable. Make a private copy to edit them.");
            return MemoryMarshal.Cast<byte, T>(storage.WritableBytes.Slice(offset, checked(Length * sizeof(T))));
        }
        public Span<T> AsSpan(int start) => AsSpan().Slice(start);
        public Span<T> AsSpan(int start, int count) => AsSpan().Slice(start, count);
        public void CopyTo(WeightBuffer<T> target, int index) => AsReadOnlySpan().CopyTo(target.AsSpan().Slice(index));
        public static implicit operator ReadOnlySpan<T>(WeightBuffer<T> value) => value.AsReadOnlySpan();
        public static implicit operator Span<T>(WeightBuffer<T> value) => value.AsSpan();
        public IEnumerator<T> GetEnumerator() { for (int i = 0; i < Length; ++i) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
