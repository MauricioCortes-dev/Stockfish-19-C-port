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
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Adapts the integer template argument and T's value initialization/fill(int).</summary>
    public interface IDynStatsPolicy<T> where T : struct
    {
        int SizeMultiplier { get; }
        void Initialize(ref T item);
        void Fill(ref T item, int value);
    }
    /// <summary>SF19-000105: identity owner for a noncopyable native unique allocation.</summary>
    public sealed class DynStats<T, TPolicy>
        where T : struct where TPolicy : struct, IDynStatsPolicy<T>
    {
        private readonly T[] data;
        public DynStats(UIntPtr size)
        {
            TPolicy policy = default;
            ulong count = unchecked(size.ToUInt64() * (ulong)(long)policy.SizeMultiplier);
            if (IntPtr.Size == 4) count = unchecked((uint)count);
            if (count > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(size), "Allocation exceeds managed array index space.");
            data = new T[(int)count];
            for (int i = 0; i < data.Length; ++i) policy.Initialize(ref data[i]);
        }
        public DynStats(int size) : this(ToNative(size)) { }
        private static UIntPtr ToNative(int size)
        { if (size < 0) throw new ArgumentOutOfRangeException(nameof(size)); return new UIntPtr((uint)size); }
        public int Count => data.Length;
        public UIntPtr GetSize() => new UIntPtr((uint)data.Length);
        public ref T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)data.Length) throw new ArgumentOutOfRangeException(nameof(index));
                return ref data[index];
            }
        }
        public ref T this[UIntPtr index]
        {
            get
            {
                ulong value = index.ToUInt64();
                if (value >= (ulong)data.Length) throw new ArgumentOutOfRangeException(nameof(index));
                return ref data[(int)value];
            }
        }
        // A true const reference. Shared mutable T must use a type-specific live read view
        // rather than invoking non-readonly struct methods on this receiver.
        public ref readonly T ReadOnlyAt(UIntPtr index) => ref this[index];
        public void ClearRange(int value, UIntPtr threadIdx, UIntPtr numaTotal)
        {
            ulong thread = threadIdx.ToUInt64(), total = numaTotal.ToUInt64(), size = (ulong)data.Length;
            if (total == 0) throw new ArgumentOutOfRangeException(nameof(numaTotal));
            ulong start = unchecked(thread * size) / total;
            if (start >= size) throw new ArgumentOutOfRangeException(nameof(threadIdx));
            ulong next = unchecked(thread + 1);
            if (IntPtr.Size == 4) next = unchecked((uint)next);
            ulong end = next == total ? size : unchecked(next * size) / total;
            if (end > size) throw new ArgumentOutOfRangeException(nameof(numaTotal));
            TPolicy policy = default;
            while (start < end) policy.Fill(ref data[(int)start++], value);
        }
    }
}
