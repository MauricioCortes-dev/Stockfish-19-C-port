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
    /// <summary>Portable unsigned arithmetic; exact primitive storage with explicit technical-alias entry points.</summary>
    public static class WideMath
    {
        public static Numerics.usize MulHi64(Numerics.u64 a, Numerics.usize b) => MulHi64(a.Value, b.Value);
        /// <summary>SF19-000191: high 64 bits of a * b, with native-width b and result.</summary>
        public static UIntPtr MulHi64(ulong a, UIntPtr b)
        {
            ulong nativeB = b.ToUInt64();
            ulong aL = unchecked((uint)a), aH = a >> 32;
            ulong bL = unchecked((uint)nativeB), bH = nativeB >> 32;
            ulong c1 = (aL * bL) >> 32;
            ulong c2 = aH * bL + c1;
            ulong c3 = aL * bH + unchecked((uint)c2);
            return new UIntPtr(aH * bH + (c2 >> 32) + (c3 >> 32));
        }
    }
}
