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
using System.Runtime.CompilerServices;

namespace Stockfish_sf_19_CSharp_Library.Types
{
    /// <summary>Codificación original de jugada de 16 bits, con semántica de valor.</summary>
    /// <remarks>SF19-000760. IsOk excluye únicamente none/null; no comprueba legalidad.</remarks>
    public readonly partial struct Move : IEquatable<Move>
    {
        public const int FromSqShift = 6;
        public const int ToSqShift = 0;

        private readonly ushort data;

        /// <summary>Conserva todos los patrones u16, sin filtrar los estados especiales.</summary>
        public Move(ushort data) => this.data = data;

        public Move(Square from, Square to) => data = unchecked((ushort)((from << 6) + to));

        public static Move Make(MoveType type, Square from, Square to) => Make(type, from, to, PieceType.KNIGHT);

        /// <summary>Equivale a Move::make&lt;T&gt;; no elimina bits de promoción para otros tipos.</summary>
        public static Move Make(MoveType type, Square from, Square to, PieceType pt)
        {
            // En C++17 un pt menor que KNIGHT desplazaría un entero negativo: no hay oráculo definido.
            if (pt < PieceType.KNIGHT)
                throw new ArgumentOutOfRangeException(nameof(pt), pt, "El desplazamiento de pt - KNIGHT debe ser no negativo.");
            return new Move(unchecked((ushort)(type + ((pt - PieceType.KNIGHT) << 12) + (from << 6) + to)));
        }

        public Square FromSq()
        {
            RequireOk();
            return (data >> 6) & 0x3f;
        }

        public Square ToSq()
        {
            RequireOk();
            return data & 0x3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveType TypeOf() => data & (3 << 14);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceType PromotionType() => ((data >> 12) & 3) + PieceType.KNIGHT;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOk() => data != None().data && data != Null().data;

        public static Move None() => new Move(0);
        public static Move Null() => new Move(65);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort Raw() => data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Move left, Move right) => left.data == right.data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Move left, Move right) => left.data != right.data;

        // Contexto condicional C++ explícito: if(move) y !move, sin asignación bool implícita.
        public static explicit operator bool(Move move) => move.data != 0;
        public static bool operator true(Move move) => move.data != 0;
        public static bool operator false(Move move) => move.data == 0;
        public static bool operator !(Move move) => move.data == 0;

        private void RequireOk()
        {
            if (!IsOk())
                throw new InvalidOperationException("none y null no admiten from_sq/to_sq.");
        }

        public bool Equals(Move other) => data == other.data;
        public override bool Equals(object? obj) => obj is Move other && Equals(other);
        public override int GetHashCode() => data.GetHashCode();
        // Representación numérica de depuración; la conversión UCI tiene su propio port.
        public override string ToString() => data.ToString();
    }
}
