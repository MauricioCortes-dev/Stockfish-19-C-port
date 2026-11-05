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
    /// <summary>Máscara inmutable de derechos de enroque y centinela CASTLING_RIGHT_NB.</summary>
    /// <remarks>Porteo de SF19-000711 y SF19-000746. Default representa NO_CASTLING.</remarks>
    public readonly struct CastlingRights : IEquatable<CastlingRights>, IComparable<CastlingRights>
    {
        public static readonly CastlingRights NO_CASTLING = 0;
        public static readonly CastlingRights WHITE_OO = 1;
        public static readonly CastlingRights WHITE_OOO = WHITE_OO << 1;
        public static readonly CastlingRights BLACK_OO = WHITE_OO << 2;
        public static readonly CastlingRights BLACK_OOO = WHITE_OO << 3;
        public static readonly CastlingRights KING_SIDE = WHITE_OO | BLACK_OO;
        public static readonly CastlingRights QUEEN_SIDE = WHITE_OOO | BLACK_OOO;
        public static readonly CastlingRights WHITE_CASTLING = WHITE_OO | WHITE_OOO;
        public static readonly CastlingRights BLACK_CASTLING = BLACK_OO | BLACK_OOO;
        public static readonly CastlingRights ANY_CASTLING = WHITE_CASTLING | BLACK_CASTLING;

        /// <summary>Número de combinaciones de derechos; no representa una máscara.</summary>
        public static readonly CastlingRights CASTLING_RIGHT_NB = 16;

        private readonly byte value;

        /// <summary>Representación original: máscaras 0..15 o centinela 16.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Indica si el valor es una máscara de derechos, incluido NO_CASTLING.</summary>
        public bool IsMask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value < CASTLING_RIGHT_NB;
        }

        /// <summary>Indica si contiene algún derecho de enroque por el flanco de rey.</summary>
        public bool HasKingSide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this & KING_SIDE) != 0;
        }

        /// <summary>Indica si contiene algún derecho de enroque por el flanco de dama.</summary>
        public bool HasQueenSide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this & QUEEN_SIDE) != 0;
        }

        /// <summary>Admite las 16 combinaciones de cuatro bits y el centinela 16.</summary>
        public CastlingRights(int value)
        {
            if (value < 0 || value > 16)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe ser una máscara 0..15 o CASTLING_RIGHT_NB (16).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra máscara.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CastlingRights left, CastlingRights right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CastlingRights left, CastlingRights right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CastlingRights left, CastlingRights right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CastlingRights left, CastlingRights right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CastlingRights left, CastlingRights right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CastlingRights left, CastlingRights right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CastlingRights left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CastlingRights left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CastlingRights left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CastlingRights left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CastlingRights left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CastlingRights left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, CastlingRights right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, CastlingRights right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, CastlingRights right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, CastlingRights right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, CastlingRights right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, CastlingRights right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CastlingRights left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CastlingRights left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CastlingRights left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CastlingRights left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CastlingRights left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CastlingRights left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, CastlingRights right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, CastlingRights right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, CastlingRights right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, CastlingRights right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, CastlingRights right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, CastlingRights right) => left >= right.value;

        // Las máscaras C++ se promocionan a int, incluso al operar entre CastlingRights.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..16.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CastlingRights left, CastlingRights right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CastlingRights left, CastlingRights right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CastlingRights left, CastlingRights right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CastlingRights left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CastlingRights left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CastlingRights left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, CastlingRights right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, CastlingRights right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, CastlingRights right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CastlingRights left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CastlingRights left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CastlingRights left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, CastlingRights right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, CastlingRights right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, CastlingRights right) => left ^ right.value;

        /// <summary>Selecciona los derechos de color, como el operador C++ de SF19-000746.</summary>
        /// <remarks>El original selecciona BLACK_CASTLING para todo color distinto de WHITE,
        /// incluido COLOR_NB. El resultado es CastlingRights, no int.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CastlingRights operator &(Color color, CastlingRights rights) =>
            (color == Color.WHITE ? WHITE_CASTLING : BLACK_CASTLING) & rights;

        /// <summary>Complemento numérico de int; no restringe el resultado a cuatro bits.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(CastlingRights rights) => ~rights.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(CastlingRights rights, int count) => rights.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(CastlingRights rights, int count) => rights.value >> count;

        /// <summary>Conserva la validación 0..16 de la entrada.</summary>
        public static implicit operator CastlingRights(byte value) => new CastlingRights(value);

        /// <summary>Conserva la validación 0..16 de la entrada.</summary>
        public static implicit operator CastlingRights(int value) => new CastlingRights(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(CastlingRights rights) => rights.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(CastlingRights rights) => rights.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(CastlingRights other) => this == other;

        public override bool Equals(object? obj) => obj is CastlingRights other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(CastlingRights other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(NO_CASTLING),
            1 => nameof(WHITE_OO),
            2 => nameof(WHITE_OOO),
            3 => nameof(WHITE_CASTLING),
            4 => nameof(BLACK_OO),
            5 => nameof(KING_SIDE),
            8 => nameof(BLACK_OOO),
            10 => nameof(QUEEN_SIDE),
            12 => nameof(BLACK_CASTLING),
            15 => nameof(ANY_CASTLING),
            16 => nameof(CASTLING_RIGHT_NB),
            _ => value.ToString()
        };
    }
}
