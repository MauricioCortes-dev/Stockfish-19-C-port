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
    // Self constraints bind every nested child to the same signed type, policy and D.
    public interface ILocalStatsNode<T, TScalar, TLimit, TSelf>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T>
        where TLimit : struct, IStatsLimit where TSelf : struct
    {
        void Fill(in T value);
        void CopyFrom(ref TSelf source);
        void Swap(ref TSelf other);
    }
    public interface IAtomicStatsNode<T, TScalar, TLimit, TSelf>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T>
        where TLimit : struct, IStatsLimit where TSelf : struct
    {
        void Fill(in T value);
        void CopyFrom(ref TSelf source);
        void Swap(ref TSelf other);
    }

    /// <summary>Leaf adapter for Stats. It keeps exactly one ordinary StatsEntry.</summary>
    public struct StatsCell<T, TScalar, TLimit> : ILocalStatsNode<T, TScalar, TLimit, StatsCell<T, TScalar, TLimit>>,
        IHistoryCell<T, StatsCell<T, TScalar, TLimit>>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T> where TLimit : struct, IStatsLimit
    {
        private StatsEntry<T, LocalStatsStorage<T, TScalar>, TLimit> entry;
        public StatsCell(T value) { entry = new StatsEntry<T, LocalStatsStorage<T, TScalar>, TLimit>(value); }
        public T Load() => entry.Load();
        public void Assign(T value) => entry.Assign(value);
        public void Update(int bonus) => entry.Update(bonus);
        public void Fill(in T value) => Assign(value);
        public void CopyFrom(ref StatsCell<T, TScalar, TLimit> source) => entry.CopyFrom(ref source.entry);
        public void Swap(ref StatsCell<T, TScalar, TLimit> other)
        { T previous = Load(); Assign(other.Load()); other.Assign(previous); }
        public static implicit operator T(StatsCell<T, TScalar, TLimit> value) => value.Load();
        public static implicit operator StatsCell<T, TScalar, TLimit>(T value) => new StatsCell<T, TScalar, TLimit>(value);
    }

    /// <summary>Leaf adapter for AtomicStats. Never copy this shared struct while writers are active.</summary>
    public struct AtomicStatsCell<T, TScalar, TLimit> : IAtomicStatsNode<T, TScalar, TLimit, AtomicStatsCell<T, TScalar, TLimit>>,
        IHistoryCell<T, AtomicStatsCell<T, TScalar, TLimit>>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T> where TLimit : struct, IStatsLimit
    {
        private StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit> entry;
        public AtomicStatsCell(T value) { entry = new StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit>(value); }
        public T Load() => entry.Load();
        public void Assign(T value) => entry.Assign(value);
        public void Update(int bonus) => entry.Update(bonus);
        public void Fill(in T value) => Assign(value);
        public void CopyFrom(ref AtomicStatsCell<T, TScalar, TLimit> source) => entry.CopyFrom(ref source.entry);
        public void Swap(ref AtomicStatsCell<T, TScalar, TLimit> other)
        { T previous = Load(); Assign(other.Load()); other.Assign(previous); }
    }
}
