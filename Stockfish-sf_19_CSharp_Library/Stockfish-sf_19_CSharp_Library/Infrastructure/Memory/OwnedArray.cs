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
using System.Threading;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Memory
{
    /// <summary>Managed adaptation of placement construction/destruction: failure rollback and reverse disposal.</summary>
    public sealed class OwnedArray<T> : IDisposable where T : class, IDisposable
    {
        private T?[]? values;
        public OwnedArray(int length, Func<int, T> construct)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (construct == null) throw new ArgumentNullException(nameof(construct));
            values = new T?[length];
            try { for (int i = 0; i < length; ++i) values[i] = construct(i) ?? throw new InvalidOperationException("Constructor returned null."); }
            catch { Dispose(); throw; }
        }
        public T this[int index] => (values ?? throw new ObjectDisposedException(nameof(OwnedArray<T>)))[index]!;
        public void Dispose()
        {
            var retired = Interlocked.Exchange(ref values, null);
            if (retired == null) return;
            // Dispose implementations must not throw (the C++ destructor contract).
            for (int i = retired.Length; i-- > 0;) retired[i]?.Dispose();
        }
    }
}
