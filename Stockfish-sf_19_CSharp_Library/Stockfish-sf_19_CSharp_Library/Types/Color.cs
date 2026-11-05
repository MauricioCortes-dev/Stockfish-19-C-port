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
    /// <summary>
    /// Valor inmutable que representa un bando o el centinela COLOR_NB.
    /// </summary>
    /// <remarks>
    /// Porteo de SF19-000710. El valor predeterminado de la estructura es WHITE.
    /// Una variable puede recibir otro Color; el valor almacenado no se modifica.
    /// </remarks>
    public readonly struct Color : IEquatable<Color>, IComparable<Color>
    {
        /// <summary>Número de bandos, utilizable como tamaño de un arreglo.</summary>
        public const int Count = 2;

        public static readonly Color WHITE = 0;
        public static readonly Color BLACK = 1;

        /// <summary>Centinela del original; no representa un bando jugable.</summary>
        public static readonly Color COLOR_NB = 2;

        private readonly byte value;

        /// <summary>Representación numérica original: blanco 0, negro 1, centinela 2.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Indica si el valor representa WHITE o BLACK.</summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value < Count;
        }

        /// <summary>Devuelve el otro bando; equivale al operador ~.</summary>
        public Color Opposite => ~this;

        /// <summary>SF19-000750: extrae el color y exige la precondición original piece != NO_PIECE.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color ColorOf(Piece piece)
        {
            if (piece == Piece.NO_PIECE)
                throw new ArgumentOutOfRangeException(nameof(piece), (int)piece, "NO_PIECE no tiene color.");
            return piece >> 3;
        }

        /// <summary>Crea un color a partir de 0, 1 o del centinela 2.</summary>
        public Color(int value)
        {
            if (value < 0 || value > Count)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe ser WHITE (0), BLACK (1) o COLOR_NB (2).");

            this.value = (byte)value;
        }

        /// <summary>Invierte WHITE y BLACK, como el operador C++ de SF19-000742.</summary>
        public static Color operator ~(Color color)
        {
            if (!color.IsValid)
                throw new InvalidOperationException("COLOR_NB no representa un bando y no tiene opuesto.");

            return (Color)(color ^ BLACK);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color left, Color right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color left, Color right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Color left, Color right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Color left, Color right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Color left, Color right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Color left, Color right) => left.value >= right.value;

        // Las comparaciones con enteros conservan la promoción numérica del original,
        // sin construir un Color para valores como -1 o 3.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Color left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Color left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Color left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Color left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Color right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Color right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Color right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Color right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Color right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Color right) => left >= right.value;

        // Los operandos byte necesitan sobrecargas propias para evitar ambigüedad
        // entre la conversión a Color y la promoción a int.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Color left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Color left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Color left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Color left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Color right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Color right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Color right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Color right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Color right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Color right) => left >= right.value;


        /// <summary>Convierte un byte conservando la validación del constructor.</summary>
        public static implicit operator Color(byte value) => new Color(value);

        /// <summary>Convierte un entero conservando la validación del constructor.</summary>
        public static implicit operator Color(int value) => new Color(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(Color color) => color.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Color color) => color.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Color other) => this == other;

        public override bool Equals(object? obj) => obj is Color other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Color other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(WHITE),
            1 => nameof(BLACK),
            _ => nameof(COLOR_NB)
        };
    }
}
