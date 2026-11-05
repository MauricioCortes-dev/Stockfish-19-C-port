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

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Valor inmutable de SymColour, con las constantes del enum original.</summary>
    /// <remarks>Porteo de SF19-000608. Default representa White.</remarks>
    internal readonly struct SymColour : IEquatable<SymColour>, IComparable<SymColour>
    {
        public static readonly SymColour White = 0;
        public static readonly SymColour Grey = 1;
        public static readonly SymColour Black = 2;

        private readonly byte value;

        /// <summary>Representación u8 de un byte; el dominio admitido es 0..2.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los tres estados nombrados del enum original.</summary>
        public SymColour(int value)
        {
            if (value < 0 || value > 2)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre 0 y 2.");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra categoría.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SymColour left, SymColour right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SymColour left, SymColour right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(SymColour left, SymColour right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(SymColour left, SymColour right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(SymColour left, SymColour right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(SymColour left, SymColour right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SymColour left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SymColour left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(SymColour left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(SymColour left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(SymColour left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(SymColour left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, SymColour right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, SymColour right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, SymColour right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, SymColour right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, SymColour right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, SymColour right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SymColour left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SymColour left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(SymColour left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(SymColour left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(SymColour left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(SymColour left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, SymColour right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, SymColour right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, SymColour right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, SymColour right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, SymColour right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, SymColour right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre SymColour.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..2.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(SymColour left, SymColour right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(SymColour left, SymColour right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(SymColour left, SymColour right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(SymColour left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(SymColour left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(SymColour left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, SymColour right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, SymColour right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, SymColour right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(SymColour left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(SymColour left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(SymColour left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, SymColour right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, SymColour right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, SymColour right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(SymColour kind) => ~kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(SymColour kind, int count) => kind.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(SymColour kind, int count) => kind.value >> count;

        /// <summary>Conserva la validación 0..2 de la entrada.</summary>
        public static implicit operator SymColour(byte value) => new SymColour(value);

        /// <summary>Conserva la validación 0..2 de la entrada.</summary>
        public static implicit operator SymColour(int value) => new SymColour(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(SymColour kind) => unchecked((byte)kind.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(SymColour kind) => kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SymColour other) => this == other;

        public override bool Equals(object? obj) => obj is SymColour other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SymColour other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(White),
            1 => nameof(Grey),
            _ => nameof(Black)
        };
    }
}
