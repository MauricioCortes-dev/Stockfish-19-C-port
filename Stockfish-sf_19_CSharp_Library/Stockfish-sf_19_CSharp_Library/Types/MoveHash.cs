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
using System.Collections.Generic;

namespace Stockfish_sf_19_CSharp_Library.Types
{
    public readonly partial struct Move
    {
        /// <summary>SF19-000761. Functor nativo y adaptador de igualdad para colecciones .NET.</summary>
        public readonly struct MoveHash : IEqualityComparer<Move>
        {
            /// <summary>Conserva el usize original, después de calcular make_key a 64 bits.</summary>
            public UIntPtr Invoke(Move move) => new UIntPtr(AtWidth(Key.MakeKey(move.Raw()), UIntPtr.Size * 8));

            // Separado para verificar ambas conversiones unsigned, incluso en un host de 64 bits.
            internal static ulong AtWidth(ulong value, int bits)
            {
                if (bits == 32) return unchecked((uint)value);
                if (bits == 64) return value;
                throw new ArgumentOutOfRangeException(nameof(bits), bits, "El ancho nativo debe ser 32 o 64.");
            }

            public bool Equals(Move left, Move right) => left == right;

            // Hash int del CLR; Invoke mantiene aparte el resultado usize completo.
            public int GetHashCode(Move move) => Invoke(move).GetHashCode();
        }
    }
}
