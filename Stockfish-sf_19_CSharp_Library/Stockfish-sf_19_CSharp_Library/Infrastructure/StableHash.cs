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
using System.Text;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>misc.h/misc.cpp hashing, including native-width combine and byte-exact unmanaged images.</summary>
    public static class StableHash
    {
        public static ulong Bytes(ReadOnlySpan<byte> data)
        {
            unchecked
            {
                const ulong m = 0xc6a4a7935bd1e995UL;
                ulong h = (ulong)data.Length * m; int i = 0;
                for (; i + 8 <= data.Length; i += 8)
                { ulong k = MemoryMarshal.Read<ulong>(data.Slice(i)); k *= m; k ^= k >> 47; k *= m; h ^= k; h *= m; }
                if (i != data.Length)
                { ulong k = 0; for (int j = data.Length - 1; j >= i; --j) k = (k << 8) | data[j]; h ^= k; h *= m; }
                h ^= h >> 47; h *= m; h ^= h >> 47; return h;
            }
        }
        public static UIntPtr RawData<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            Type t = typeof(T);
            if ((!t.IsPrimitive && !t.IsEnum) || t == typeof(float) || t == typeof(double))
                throw new ArgumentException("Raw hashing requires integral values without padding. Hash an explicitly defined byte layout for aggregates.");
            return new UIntPtr(Native(Bytes(MemoryMarshal.AsBytes(data))));
        }
        public static ulong String(string value) => Bytes(Encoding.UTF8.GetBytes(value));
        private static ulong Native(ulong value) => UIntPtr.Size == 4 ? (uint)value : value;
        public static UIntPtr Combine(UIntPtr seed, UIntPtr value)
        {
            ulong s = seed.ToUInt64();
            return new UIntPtr(Native(unchecked(s ^ (value.ToUInt64() + 0x9e3779b9UL + Native(s << 6) + (s >> 2)))));
        }
    }
}
