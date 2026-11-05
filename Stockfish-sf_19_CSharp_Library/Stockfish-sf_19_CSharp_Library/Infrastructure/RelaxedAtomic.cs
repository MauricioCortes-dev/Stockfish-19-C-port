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
    /// <summary>Individually non-tearing loads/stores. Arithmetic is deliberately NOT an atomic RMW.</summary>
    /// <remarks>SF19-000189. Access a shared instance in place; never copy its storage concurrently.
    /// Use the ref constructor/CopyFrom for a synchronized value snapshot. No publication of other data is promised.</remarks>
    public struct RelaxedAtomic<T, TStorage>
        where T : struct
        where TStorage : struct, IRelaxedAtomicStorage<T>
    {
        private TStorage inner;
        public RelaxedAtomic(T value) { inner = default; inner.Store(value); }
        public RelaxedAtomic(ref RelaxedAtomic<T, TStorage> source) : this(source.Load()) { }
        public T Load() => inner.Load();
        public void Store(T value) => inner.Store(value);
        public T Assign(T value) { Store(value); return value; }
        public void CopyFrom(ref RelaxedAtomic<T, TStorage> source) => Store(source.Load());
        public void Add(T value) { T current = Load(); Store(inner.Add(current, value)); }
        public void Subtract(T value) { T current = Load(); Store(inner.Subtract(current, value)); }
        public void Increment() => Add(inner.One);
        public void Decrement() => Subtract(inner.One);
        public T PostIncrement() { T current = Load(); Store(inner.Add(current, inner.One)); return current; }
        public T PostDecrement() { T current = Load(); Store(inner.Subtract(current, inner.One)); return current; }
    }
}
