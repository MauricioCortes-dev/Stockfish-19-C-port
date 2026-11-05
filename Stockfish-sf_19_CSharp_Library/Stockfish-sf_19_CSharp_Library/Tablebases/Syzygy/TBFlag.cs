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
    /// <summary>Valor inmutable de TBFlag, con las constantes del enum original.</summary>
    /// <remarks>Porteo de SF19-000579. Default representa la máscara cero sin nombre.</remarks>
    internal readonly struct TBFlag : IEquatable<TBFlag>, IComparable<TBFlag>
    {
        public static readonly TBFlag STM = 1;
        public static readonly TBFlag Mapped = 2;
        public static readonly TBFlag WinPlies = 4;
        public static readonly TBFlag LossPlies = 8;
        public static readonly TBFlag Wide = 16;
        public static readonly TBFlag SingleValue = 128;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es 0..255.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite todo el rango definido del enum original: 0..255, incluidos bits sin nombre.</summary>
        public TBFlag(int value)
        {
            if (value < 0 || value > 255)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre 0 y 255.");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra categoría.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TBFlag left, TBFlag right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TBFlag left, TBFlag right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(TBFlag left, TBFlag right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(TBFlag left, TBFlag right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(TBFlag left, TBFlag right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(TBFlag left, TBFlag right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TBFlag left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TBFlag left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(TBFlag left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(TBFlag left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(TBFlag left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(TBFlag left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, TBFlag right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, TBFlag right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, TBFlag right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, TBFlag right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, TBFlag right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, TBFlag right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TBFlag left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TBFlag left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(TBFlag left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(TBFlag left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(TBFlag left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(TBFlag left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, TBFlag right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, TBFlag right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, TBFlag right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, TBFlag right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, TBFlag right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, TBFlag right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre TBFlag.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..255.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(TBFlag left, TBFlag right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(TBFlag left, TBFlag right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(TBFlag left, TBFlag right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(TBFlag left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(TBFlag left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(TBFlag left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, TBFlag right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, TBFlag right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, TBFlag right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(TBFlag left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(TBFlag left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(TBFlag left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, TBFlag right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, TBFlag right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, TBFlag right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(TBFlag kind) => ~kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(TBFlag kind, int count) => kind.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(TBFlag kind, int count) => kind.value >> count;

        /// <summary>Conserva la validación 0..255 de la entrada.</summary>
        public static implicit operator TBFlag(byte value) => new TBFlag(value);

        /// <summary>Conserva la validación 0..255 de la entrada.</summary>
        public static implicit operator TBFlag(int value) => new TBFlag(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(TBFlag kind) => unchecked((byte)kind.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(TBFlag kind) => kind.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TBFlag other) => this == other;

        public override bool Equals(object? obj) => obj is TBFlag other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TBFlag other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            1 => nameof(STM),
            2 => nameof(Mapped),
            4 => nameof(WinPlies),
            8 => nameof(LossPlies),
            16 => nameof(Wide),
            128 => nameof(SingleValue),
            _ => value.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
