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

using System.Collections;
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000900: nominal alias adapter, with recursive fixed dimensions and live ref access.</summary>
    public struct AtomicStats<T, TScalar, TLimit, TChild, TStorage> : IReadOnlyList<TChild>,
        IAtomicStatsNode<T, TScalar, TLimit, AtomicStats<T, TScalar, TLimit, TChild, TStorage>>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T>
        where TLimit : struct, IStatsLimit
        where TChild : struct, IAtomicStatsNode<T, TScalar, TLimit, TChild>
        where TStorage : struct, IMultiArrayStorage<TChild, TStorage>
    {
        private MultiArray<TChild, TStorage> values;
        private static readonly MultiArrayAssignment<TChild, T> assign = AssignChild;
        private static void AssignChild(ref TChild child, in T value) => child.Fill(in value);
        public int Count => values.Count;
        public int Size() => Count;
        public int MaxSize() => Count;
        public bool Empty() => Count == 0;
        public TChild this[int index]
        {
            get { var snapshot = default(TChild); snapshot.CopyFrom(ref Element(ref this, index)); return snapshot; }
            set => Element(ref this, index).CopyFrom(ref value);
        }
        public TChild Front { get => this[0]; set => this[0] = value; }
        public TChild Back { get => this[Count - 1]; set => this[Count - 1] = value; }
        public void Fill(in T value) => values.Fill(in value, assign);
        public AtomicStats(ref AtomicStats<T, TScalar, TLimit, TChild, TStorage> source) { values = default; CopyFrom(ref source); }
        public void CopyFrom(ref AtomicStats<T, TScalar, TLimit, TChild, TStorage> source)
        {
            for (int i = 0; i < Count; ++i)
                Element(ref this, i).CopyFrom(ref Element(ref source, i));
        }
        // std::array::swap visits entries; for shared leaves these are individual Load/Stores.
        public void Swap(ref AtomicStats<T, TScalar, TLimit, TChild, TStorage> other)
        {
            for (int i = 0; i < Count; ++i)
                Element(ref this, i).Swap(ref Element(ref other, i));
        }
        internal static ref TChild Element(ref AtomicStats<T, TScalar, TLimit, TChild, TStorage> source, int index)
            => ref MultiArray.At(ref source.values, index);
        public MultiArray<TChild, TStorage>.Enumerator GetEnumerator()
        {
            var snapshot = default(AtomicStats<T, TScalar, TLimit, TChild, TStorage>); snapshot.CopyFrom(ref this);
            return snapshot.values.GetEnumerator();
        }
        public MultiArray<TChild, TStorage>.Enumerator GetReverseEnumerator()
        {
            var snapshot = default(AtomicStats<T, TScalar, TLimit, TChild, TStorage>); snapshot.CopyFrom(ref this);
            return snapshot.values.GetReverseEnumerator();
        }
        IEnumerator<TChild> IEnumerable<TChild>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static class AtomicStats
    {
        public static ref TChild At<T, TScalar, TLimit, TChild, TStorage>(ref AtomicStats<T, TScalar, TLimit, TChild, TStorage> source, int index)
            where T : struct where TScalar : struct, ISignedAtomicStorage<T>
            where TLimit : struct, IStatsLimit
            where TChild : struct, IAtomicStatsNode<T, TScalar, TLimit, TChild>
            where TStorage : struct, IMultiArrayStorage<TChild, TStorage>
            => ref AtomicStats<T, TScalar, TLimit, TChild, TStorage>.Element(ref source, index);
    }
}
