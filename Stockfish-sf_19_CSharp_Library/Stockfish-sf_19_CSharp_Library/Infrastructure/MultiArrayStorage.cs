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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    public delegate ref T MultiArrayAccess<T, TStorage>(ref TStorage storage, int index);

    /// <summary>Inline, fixed nonnegative length and a checked static ref accessor. No shared mutable arrays.</summary>
    public interface IMultiArrayStorage<T, TStorage> where TStorage : struct
    {
        int Length { get; }
        MultiArrayAccess<T, TStorage> Access { get; }
    }

    public struct MultiArrayStorage0<T> : IMultiArrayStorage<T, MultiArrayStorage0<T>>
    {
        private static readonly MultiArrayAccess<T, MultiArrayStorage0<T>> accessor = Get;
        public int Length => 0;
        public MultiArrayAccess<T, MultiArrayStorage0<T>> Access => accessor;
        private static ref T Get(ref MultiArrayStorage0<T> storage, int index)
            => throw new ArgumentOutOfRangeException(nameof(index));
    }

    public struct MultiArrayStorage1<T> : IMultiArrayStorage<T, MultiArrayStorage1<T>>
    {
        private T value;
        private static readonly MultiArrayAccess<T, MultiArrayStorage1<T>> accessor = Get;
        public int Length => 1;
        public MultiArrayAccess<T, MultiArrayStorage1<T>> Access => accessor;
        private static ref T Get(ref MultiArrayStorage1<T> storage, int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return ref storage.value;
        }
    }

    /// <summary>Compose arbitrary fixed lengths without allocating element buffers.</summary>
    public struct MultiArrayStoragePair<T, TLeft, TRight> : IMultiArrayStorage<T, MultiArrayStoragePair<T, TLeft, TRight>>
        where TLeft : struct, IMultiArrayStorage<T, TLeft>
        where TRight : struct, IMultiArrayStorage<T, TRight>
    {
        private TLeft left;
        private TRight right;
        private static readonly MultiArrayAccess<T, MultiArrayStoragePair<T, TLeft, TRight>> accessor = Get;
        public int Length => checked(left.Length + right.Length);
        public MultiArrayAccess<T, MultiArrayStoragePair<T, TLeft, TRight>> Access => accessor;
        private static ref T Get(ref MultiArrayStoragePair<T, TLeft, TRight> storage, int index)
        {
            if ((uint)index >= (uint)storage.Length) throw new ArgumentOutOfRangeException(nameof(index));
            int split = storage.left.Length;
            if (index < split) return ref storage.left.Access(ref storage.left, index);
            return ref storage.right.Access(ref storage.right, index - split);
        }
    }
}
