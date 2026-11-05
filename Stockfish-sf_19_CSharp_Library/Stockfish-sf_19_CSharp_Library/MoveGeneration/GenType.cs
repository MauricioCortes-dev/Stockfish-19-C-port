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

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>Categoría inmutable de generación de jugadas del enum original GenType.</summary>
    /// <remarks>Porteo de SF19-000210. Default representa CAPTURES.</remarks>
    public readonly struct GenType : IEquatable<GenType>, IComparable<GenType>
    {
        public static readonly GenType CAPTURES = 0;
        public static readonly GenType QUIETS = 1;
        public static readonly GenType EVASIONS = 2;
        public static readonly GenType NON_EVASIONS = 3;
        public static readonly GenType LEGAL = 4;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es 0..4.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los cinco valores nombrados del enum original.</summary>
        public GenType(int value)
        {
            if (value < 0 || value > 4)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre CAPTURES (0) y LEGAL (4).");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra categoría.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(GenType left, GenType right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(GenType left, GenType right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(GenType left, GenType right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(GenType left, GenType right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(GenType left, GenType right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(GenType left, GenType right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(GenType left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(GenType left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(GenType left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(GenType left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(GenType left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(GenType left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, GenType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, GenType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, GenType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, GenType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, GenType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, GenType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(GenType left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(GenType left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(GenType left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(GenType left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(GenType left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(GenType left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, GenType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, GenType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, GenType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, GenType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, GenType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, GenType right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre GenType.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..4.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(GenType left, GenType right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(GenType left, GenType right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(GenType left, GenType right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(GenType left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(GenType left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(GenType left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, GenType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, GenType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, GenType right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(GenType left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(GenType left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(GenType left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, GenType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, GenType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, GenType right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(GenType kind) => ~kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(GenType kind, int count) => kind.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(GenType kind, int count) => kind.value >> count;

        /// <summary>Conserva la validación 0..4 de la entrada.</summary>
        public static implicit operator GenType(byte value) => new GenType(value);

        /// <summary>Conserva la validación 0..4 de la entrada.</summary>
        public static implicit operator GenType(int value) => new GenType(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(GenType kind) => (byte)kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(GenType kind) => kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(GenType other) => this == other;

        public override bool Equals(object? obj) => obj is GenType other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(GenType other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(CAPTURES),
            1 => nameof(QUIETS),
            2 => nameof(EVASIONS),
            3 => nameof(NON_EVASIONS),
            _ => nameof(LEGAL)
        };
    }
}
