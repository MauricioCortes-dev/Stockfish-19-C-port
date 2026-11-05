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
    /// <summary>Columna inmutable del tablero, incluido el centinela FILE_NB.</summary>
    /// <remarks>Porteo de SF19-000728. Default representa FILE_A.</remarks>
    public readonly struct File : IEquatable<File>, IComparable<File>
    {
        public static readonly File FILE_A = 0;
        public static readonly File FILE_B = 1;
        public static readonly File FILE_C = 2;
        public static readonly File FILE_D = 3;
        public static readonly File FILE_E = 4;
        public static readonly File FILE_F = 5;
        public static readonly File FILE_G = 6;
        public static readonly File FILE_H = 7;
        public static readonly File FILE_NB = 8;

        private readonly byte value;

        /// <summary>Representación original: índices 0..7 y centinela 8.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los ocho índices y el centinela de conteo (8).</summary>
        public File(int value)
        {
            if (value < 0 || value > 8)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre FILE_A (0) y FILE_NB (8).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro índice.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, File right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, File right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(File left, File right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(File left, File right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(File left, File right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(File left, File right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(File left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(File left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(File left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(File left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, File right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, File right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, File right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, File right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, File right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, File right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(File left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(File left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(File left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(File left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, File right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, File right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, File right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, File right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, File right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, File right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre File.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..8.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(File left, File right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(File left, File right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(File left, File right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(File left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(File left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(File left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, File right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, File right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, File right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(File left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(File left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(File left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, File right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, File right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, File right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(File file) => ~file.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(File file, int count) => file.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(File file, int count) => file.value >> count;

        // C# usa estos operadores tanto para prefijo como para postfijo.
        // El constructor rechaza atravesar los límites del dominio admitido.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator ++(File index) => new File(index.value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator --(File index) => new File(index.value - 1);

        /// <summary>SF19-000054: distancia a un borde de columna, incluido FILE_NB.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EdgeDistance(File file)
        {
            // El cast u8 original produce 255 para FILE_NB; no construir un File fuera de 0..8.
            int mirrored = unchecked((byte)(FILE_H - file));
            return Math.Min((int)file, mirrored);
        }

        /// <summary>SF19-000752: extrae la columna, incluido el resultado de SQ_NONE.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File FileOf(Square square) => square & 7;

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator File(byte value) => new File(value);

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator File(int value) => new File(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(File file) => file.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(File file) => file.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(File other) => this == other;

        public override bool Equals(object? obj) => obj is File other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(File other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(FILE_A),
            1 => nameof(FILE_B),
            2 => nameof(FILE_C),
            3 => nameof(FILE_D),
            4 => nameof(FILE_E),
            5 => nameof(FILE_F),
            6 => nameof(FILE_G),
            7 => nameof(FILE_H),
            _ => nameof(FILE_NB)
        };
    }
}
