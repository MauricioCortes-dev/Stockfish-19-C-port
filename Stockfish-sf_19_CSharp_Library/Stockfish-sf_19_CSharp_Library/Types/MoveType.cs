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
    /// <summary>Tipo inmutable de jugada, codificado en los bits 14 y 15 del original.</summary>
    /// <remarks>Porteo de SF19-000759. Default representa NORMAL.</remarks>
    public readonly struct MoveType : IEquatable<MoveType>, IComparable<MoveType>
    {
        public static readonly MoveType NORMAL = 0;
        public static readonly MoveType PROMOTION = 1 << 14;
        public static readonly MoveType EN_PASSANT = 2 << 14;
        public static readonly MoveType CASTLING = 3 << 14;

        private readonly ushort value;

        /// <summary>Representación original de 16 bits; los únicos bits activos son 14 y 15.</summary>
        public ushort Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite únicamente los cuatro tipos de jugada nombrados en el original.</summary>
        public MoveType(int value)
        {
            if (value != 0 && value != (1 << 14) && value != (2 << 14) && value != (3 << 14))
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe ser NORMAL (0), PROMOTION (16384), EN_PASSANT (32768) o CASTLING (49152).");

            this.value = (ushort)value;
        }

        // Comparar con int/ushort conserva la promoción numérica sin construir otro tipo de jugada.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveType left, MoveType right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveType left, MoveType right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(MoveType left, MoveType right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(MoveType left, MoveType right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(MoveType left, MoveType right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(MoveType left, MoveType right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveType left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveType left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(MoveType left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(MoveType left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(MoveType left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(MoveType left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, MoveType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, MoveType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, MoveType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, MoveType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, MoveType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, MoveType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveType left, ushort right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveType left, ushort right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(MoveType left, ushort right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(MoveType left, ushort right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(MoveType left, ushort right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(MoveType left, ushort right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ushort left, MoveType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ushort left, MoveType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ushort left, MoveType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ushort left, MoveType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ushort left, MoveType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ushort left, MoveType right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre MoveType.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de los cuatro valores nombrados.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(MoveType left, MoveType right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(MoveType left, MoveType right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(MoveType left, MoveType right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(MoveType left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(MoveType left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(MoveType left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, MoveType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, MoveType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, MoveType right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(MoveType left, ushort right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(MoveType left, ushort right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(MoveType left, ushort right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(ushort left, MoveType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(ushort left, MoveType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(ushort left, MoveType right) => left ^ right.value;

        // byte requiere coincidencia exacta: las conversiones hacia ushort y MoveType
        // podrían competir en la resolución de sobrecargas. El resultado sigue siendo numérico.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveType left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, MoveType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveType left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, MoveType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(MoveType left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, MoveType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(MoveType left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, MoveType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(MoveType left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, MoveType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(MoveType left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, MoveType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(MoveType left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, MoveType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(MoveType left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, MoveType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(MoveType left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, MoveType right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no restringe el resultado a dos bits.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(MoveType moveType) => ~moveType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(MoveType moveType, int count) => moveType.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(MoveType moveType, int count) => moveType.value >> count;

        /// <summary>Conserva la validación de los cuatro valores nombrados.</summary>
        public static implicit operator MoveType(ushort value) => new MoveType(value);

        /// <summary>Conserva la validación de los cuatro valores nombrados.</summary>
        public static implicit operator MoveType(int value) => new MoveType(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ushort(MoveType moveType) => moveType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(MoveType moveType) => moveType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MoveType other) => this == other;

        public override bool Equals(object? obj) => obj is MoveType other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(MoveType other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(NORMAL),
            1 << 14 => nameof(PROMOTION),
            2 << 14 => nameof(EN_PASSANT),
            _ => nameof(CASTLING)
        };
    }
}
