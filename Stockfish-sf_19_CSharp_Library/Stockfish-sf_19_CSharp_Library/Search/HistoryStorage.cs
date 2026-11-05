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

using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage2<T> : IMultiArrayStorage<T, HistoryStorage2<T>>
    {
        private MultiArrayStoragePair<T, MultiArrayStorage1<T>, MultiArrayStorage1<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage2<T>> access = Get;
        public int Length => 2;
        public MultiArrayAccess<T, HistoryStorage2<T>> Access => access;
        private static ref T Get(ref HistoryStorage2<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage4<T> : IMultiArrayStorage<T, HistoryStorage4<T>>
    {
        private MultiArrayStoragePair<T, HistoryStorage2<T>, HistoryStorage2<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage4<T>> access = Get;
        public int Length => 4;
        public MultiArrayAccess<T, HistoryStorage4<T>> Access => access;
        private static ref T Get(ref HistoryStorage4<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage8<T> : IMultiArrayStorage<T, HistoryStorage8<T>>
    {
        private MultiArrayStoragePair<T, HistoryStorage4<T>, HistoryStorage4<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage8<T>> access = Get;
        public int Length => 8;
        public MultiArrayAccess<T, HistoryStorage8<T>> Access => access;
        private static ref T Get(ref HistoryStorage8<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage16<T> : IMultiArrayStorage<T, HistoryStorage16<T>>
    {
        private MultiArrayStoragePair<T, HistoryStorage8<T>, HistoryStorage8<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage16<T>> access = Get;
        public int Length => 16;
        public MultiArrayAccess<T, HistoryStorage16<T>> Access => access;
        private static ref T Get(ref HistoryStorage16<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage32<T> : IMultiArrayStorage<T, HistoryStorage32<T>>
    {
        private MultiArrayStoragePair<T, HistoryStorage16<T>, HistoryStorage16<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage32<T>> access = Get;
        public int Length => 32;
        public MultiArrayAccess<T, HistoryStorage32<T>> Access => access;
        private static ref T Get(ref HistoryStorage32<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
    /// <summary>Inline shape adapter; not an additional engine alias.</summary>
    public struct HistoryStorage64<T> : IMultiArrayStorage<T, HistoryStorage64<T>>
    {
        private MultiArrayStoragePair<T, HistoryStorage32<T>, HistoryStorage32<T>> values;
        private static readonly MultiArrayAccess<T, HistoryStorage64<T>> access = Get;
        public int Length => 64;
        public MultiArrayAccess<T, HistoryStorage64<T>> Access => access;
        private static ref T Get(ref HistoryStorage64<T> source, int index)
            => ref source.values.Access(ref source.values, index);
    }
}
