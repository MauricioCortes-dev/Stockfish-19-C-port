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
    /// <summary>Máscara inmutable del tipo de límite de una puntuación de búsqueda.</summary>
    /// <remarks>Porteo de SF19-000712. Default representa BOUND_NONE.</remarks>
    public readonly struct Bound : IEquatable<Bound>, IComparable<Bound>
    {
        public static readonly Bound BOUND_NONE = 0;
        public static readonly Bound BOUND_UPPER = 1;
        public static readonly Bound BOUND_LOWER = 2;
        public static readonly Bound BOUND_EXACT = BOUND_UPPER | BOUND_LOWER;

        private readonly byte value;

        /// <summary>Representación original de los dos bits: 0, 1, 2 o 3.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Indica si la máscara contiene un límite superior.</summary>
        public bool HasUpperBound
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this & BOUND_UPPER) != 0;
        }

        /// <summary>Indica si la máscara contiene un límite inferior.</summary>
        public bool HasLowerBound
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this & BOUND_LOWER) != 0;
        }

        /// <summary>Admite las cuatro combinaciones de los dos bits del original.</summary>
        public Bound(int value)
        {
            if (value < 0 || value > 3)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre BOUND_NONE (0) y BOUND_EXACT (3).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra máscara.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bound left, Bound right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bound left, Bound right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bound left, Bound right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bound left, Bound right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bound left, Bound right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bound left, Bound right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bound left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bound left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bound left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bound left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bound left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bound left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Bound right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Bound right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Bound right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Bound right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Bound right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Bound right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bound left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bound left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bound left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bound left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bound left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bound left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Bound right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Bound right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Bound right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Bound right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Bound right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Bound right) => left >= right.value;

        // Las máscaras C++ se promocionan a int, incluso al operar entre Bound.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..3.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Bound left, Bound right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Bound left, Bound right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Bound left, Bound right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Bound left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Bound left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Bound left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Bound right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Bound right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Bound right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Bound left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Bound left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Bound left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Bound right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Bound right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Bound right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no restringe el resultado a dos bits.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Bound bound) => ~bound.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Bound bound, int count) => bound.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Bound bound, int count) => bound.value >> count;

        /// <summary>Conserva la validación 0..3 de la entrada.</summary>
        public static implicit operator Bound(byte value) => new Bound(value);

        /// <summary>Conserva la validación 0..3 de la entrada.</summary>
        public static implicit operator Bound(int value) => new Bound(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(Bound bound) => bound.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Bound bound) => bound.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bound other) => this == other;

        public override bool Equals(object? obj) => obj is Bound other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Bound other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(BOUND_NONE),
            1 => nameof(BOUND_UPPER),
            2 => nameof(BOUND_LOWER),
            _ => nameof(BOUND_EXACT)
        };
    }
}
