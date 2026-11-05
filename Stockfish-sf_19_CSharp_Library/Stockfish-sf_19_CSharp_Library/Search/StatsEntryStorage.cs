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
    public interface IStatsEntryStorage<T> where T : struct
    {
        T Load();
        void Store(T value);
        long Maximum { get; }
        int ArithmeticBits { get; }
        long ToWide(T value);
        T Narrow(long value);
    }

    /// <summary>Shared=false: ordinary reads/writes and independent inline copies.</summary>
    public struct LocalStatsStorage<T, TScalar> : IStatsEntryStorage<T>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T>
    {
        private TScalar entry;
        public LocalStatsStorage(T value) { entry = default; entry.StoreUnshared(value); }
        public T Load() => entry.LoadUnshared();
        public void Store(T value) => entry.StoreUnshared(value);
        public long Maximum => entry.Maximum;
        public int ArithmeticBits => entry.ArithmeticBits;
        public long ToWide(T value) => entry.ToWide(value);
        public T Narrow(long value) => entry.Narrow(value);
    }

    /// <summary>Shared=true: the same RelaxedAtomic load/store pair as the original.</summary>
    public struct SharedStatsStorage<T, TScalar> : IStatsEntryStorage<T>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T>
    {
        private RelaxedAtomic<T, TScalar> entry;
        public T Load() => entry.Load();
        public void Store(T value) => entry.Store(value);
        public long Maximum => default(TScalar).Maximum;
        public int ArithmeticBits => default(TScalar).ArithmeticBits;
        public long ToWide(T value) => default(TScalar).ToWide(value);
        public T Narrow(long value) => default(TScalar).Narrow(value);
    }
}
