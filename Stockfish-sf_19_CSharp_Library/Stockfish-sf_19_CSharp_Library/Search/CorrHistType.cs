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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Valor inmutable de CorrHistType, con las constantes del enum original.</summary>
    /// <remarks>Porteo de SF19-000106. Default representa PieceTo.</remarks>
    public readonly struct CorrHistType : IEquatable<CorrHistType>, IComparable<CorrHistType>
    {
        public static readonly CorrHistType PieceTo = 0;
        public static readonly CorrHistType Continuation = 1;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es 0..1.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los 2 valores nombrados del enum original.</summary>
        public CorrHistType(int value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre 0 y 1.");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra categoría.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CorrHistType left, CorrHistType right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CorrHistType left, CorrHistType right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CorrHistType left, CorrHistType right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CorrHistType left, CorrHistType right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CorrHistType left, CorrHistType right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CorrHistType left, CorrHistType right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CorrHistType left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CorrHistType left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CorrHistType left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CorrHistType left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CorrHistType left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CorrHistType left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, CorrHistType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, CorrHistType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, CorrHistType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, CorrHistType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, CorrHistType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, CorrHistType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CorrHistType left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CorrHistType left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(CorrHistType left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(CorrHistType left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(CorrHistType left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(CorrHistType left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, CorrHistType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, CorrHistType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, CorrHistType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, CorrHistType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, CorrHistType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, CorrHistType right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre CorrHistType.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..1.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CorrHistType left, CorrHistType right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CorrHistType left, CorrHistType right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CorrHistType left, CorrHistType right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CorrHistType left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CorrHistType left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CorrHistType left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, CorrHistType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, CorrHistType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, CorrHistType right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(CorrHistType left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(CorrHistType left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(CorrHistType left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, CorrHistType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, CorrHistType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, CorrHistType right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(CorrHistType kind) => ~kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(CorrHistType kind, int count) => kind.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(CorrHistType kind, int count) => kind.value >> count;

        /// <summary>Conserva la validación 0..1 de la entrada.</summary>
        public static implicit operator CorrHistType(byte value) => new CorrHistType(value);

        /// <summary>Conserva la validación 0..1 de la entrada.</summary>
        public static implicit operator CorrHistType(int value) => new CorrHistType(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(CorrHistType kind) => unchecked((byte)kind.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(CorrHistType kind) => kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(CorrHistType other) => this == other;

        public override bool Equals(object? obj) => obj is CorrHistType other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(CorrHistType other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(PieceTo),
            _ => nameof(Continuation)
        };
    }
}


