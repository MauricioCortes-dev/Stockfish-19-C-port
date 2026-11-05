/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS square)

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
    /// <summary>Casilla inmutable del tablero, incluido el centinela SQ_NONE.</summary>
    /// <remarks>Porteo de SF19-000726. Default representa SQ_A1.</remarks>
    public readonly struct Square : IEquatable<Square>, IComparable<Square>
    {
        public static readonly Square SQ_A1 = 0;
        public static readonly Square SQ_B1 = 1;
        public static readonly Square SQ_C1 = 2;
        public static readonly Square SQ_D1 = 3;
        public static readonly Square SQ_E1 = 4;
        public static readonly Square SQ_F1 = 5;
        public static readonly Square SQ_G1 = 6;
        public static readonly Square SQ_H1 = 7;
        public static readonly Square SQ_A2 = 8;
        public static readonly Square SQ_B2 = 9;
        public static readonly Square SQ_C2 = 10;
        public static readonly Square SQ_D2 = 11;
        public static readonly Square SQ_E2 = 12;
        public static readonly Square SQ_F2 = 13;
        public static readonly Square SQ_G2 = 14;
        public static readonly Square SQ_H2 = 15;
        public static readonly Square SQ_A3 = 16;
        public static readonly Square SQ_B3 = 17;
        public static readonly Square SQ_C3 = 18;
        public static readonly Square SQ_D3 = 19;
        public static readonly Square SQ_E3 = 20;
        public static readonly Square SQ_F3 = 21;
        public static readonly Square SQ_G3 = 22;
        public static readonly Square SQ_H3 = 23;
        public static readonly Square SQ_A4 = 24;
        public static readonly Square SQ_B4 = 25;
        public static readonly Square SQ_C4 = 26;
        public static readonly Square SQ_D4 = 27;
        public static readonly Square SQ_E4 = 28;
        public static readonly Square SQ_F4 = 29;
        public static readonly Square SQ_G4 = 30;
        public static readonly Square SQ_H4 = 31;
        public static readonly Square SQ_A5 = 32;
        public static readonly Square SQ_B5 = 33;
        public static readonly Square SQ_C5 = 34;
        public static readonly Square SQ_D5 = 35;
        public static readonly Square SQ_E5 = 36;
        public static readonly Square SQ_F5 = 37;
        public static readonly Square SQ_G5 = 38;
        public static readonly Square SQ_H5 = 39;
        public static readonly Square SQ_A6 = 40;
        public static readonly Square SQ_B6 = 41;
        public static readonly Square SQ_C6 = 42;
        public static readonly Square SQ_D6 = 43;
        public static readonly Square SQ_E6 = 44;
        public static readonly Square SQ_F6 = 45;
        public static readonly Square SQ_G6 = 46;
        public static readonly Square SQ_H6 = 47;
        public static readonly Square SQ_A7 = 48;
        public static readonly Square SQ_B7 = 49;
        public static readonly Square SQ_C7 = 50;
        public static readonly Square SQ_D7 = 51;
        public static readonly Square SQ_E7 = 52;
        public static readonly Square SQ_F7 = 53;
        public static readonly Square SQ_G7 = 54;
        public static readonly Square SQ_H7 = 55;
        public static readonly Square SQ_A8 = 56;
        public static readonly Square SQ_B8 = 57;
        public static readonly Square SQ_C8 = 58;
        public static readonly Square SQ_D8 = 59;
        public static readonly Square SQ_E8 = 60;
        public static readonly Square SQ_F8 = 61;
        public static readonly Square SQ_G8 = 62;
        public static readonly Square SQ_H8 = 63;
        public static readonly Square SQ_NONE = 64;
        public static readonly Square SQUARE_ZERO = 0;
        public static readonly Square SQUARE_NB = 64;

        private readonly byte value;

        /// <summary>Representación original: casillas 0..63 y centinela 64.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite las 64 casillas y el centinela SQ_NONE (64).</summary>
        public Square(int value)
        {
            if (value < 0 || value > 64)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre SQ_A1 (0) y SQ_NONE (64).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro índice.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, Square right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, Square right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, Square right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, Square right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Square left, Square right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Square left, Square right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Square left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Square left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Square right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Square right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Square right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Square right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Square right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Square right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Square left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Square left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Square right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Square right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Square right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Square right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Square right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Square right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre Square.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..64.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Square left, Square right) => (int)left.value & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // bitboard.h: dos casillas forman una máscara, no el OR de sus índices.
        public static Bitboard operator |(Square left, Square right) =>
            Board.Bitboards.SquareBB(left) | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Square left, Square right) => (int)left.value ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Square left, int right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Square left, int right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Square left, int right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Square right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Square right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Square right) => (int)left ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Square left, byte right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Square left, byte right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Square left, byte right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Square right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Square right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Square right) => (int)left ^ (int)right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Square square) => ~square.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Square square, int count) => square.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Square square, int count) => square.value >> count;

        // C# usa estos operadores tanto para prefijo como para postfijo.
        // El constructor rechaza atravesar los límites del dominio admitido.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator ++(Square index) => new Square(index.value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator --(Square index) => new Square(index.value - 1);

        /// <summary>SF19-000747: conserva (rank &lt;&lt; 3) + file y valida el resultado.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square MakeSquare(File file, Rank rank) => (rank << 3) + file;

        /// <summary>SF19-000751: únicamente 0..63 son casillas del tablero.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // u8/byte no contiene negativos; el límite inferior original es redundante.
        public static bool IsOk(Square square) => square <= SQ_H8;

        /// <summary>SF19-000743: intercambia A1 y A8 mediante XOR.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square FlipRank(Square square) => square ^ SQ_A8;

        /// <summary>SF19-000744: intercambia A1 y H1 mediante XOR.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square FlipFile(Square square) => square ^ SQ_H1;

        /// <summary>SF19-000754: conserva square ^ (color * 56).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square RelativeSquare(Color color, Square square) => square ^ (color * 56);

        // SF19-000738/739. C# genera +=/-= con estos resultados por valor
        // (SF19-000740/741), mientras C++ devuelve una referencia al operando.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator +(Square square, Direction direction) => new Square(square.value + (int)direction);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square operator -(Square square, Direction direction) => new Square(square.value - (int)direction);

        // Los órdenes inversos conservan int y evitan competir con los operadores de Direction.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Direction direction, Square square) => (int)direction + square.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction direction, Square square) => (int)direction - square.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Square left, Square right) => checked(left.value + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Square left, Square right) => checked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Square left, int right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Square left, int right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(int left, Square right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(int left, Square right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Square left, byte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Square left, byte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(byte left, Square right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(byte left, Square right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Square left, sbyte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Square left, sbyte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Square left, sbyte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Square left, sbyte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Square left, sbyte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Square left, sbyte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Square left, sbyte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Square left, sbyte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Square left, sbyte right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Square left, sbyte right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Square left, sbyte right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(sbyte left, Square right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(sbyte left, Square right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(sbyte left, Square right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sbyte left, Square right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sbyte left, Square right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sbyte left, Square right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sbyte left, Square right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sbyte left, Square right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(sbyte left, Square right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(sbyte left, Square right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(sbyte left, Square right) => (int)left ^ (int)right.value;

        /// <summary>Conserva la validación 0..64 de la entrada.</summary>
        public static implicit operator Square(byte value) => new Square(value);

        /// <summary>Conserva la validación 0..64 de la entrada.</summary>
        public static implicit operator Square(int value) => new Square(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(Square square) => square.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Square square) => square.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Square other) => this == other;

        public override bool Equals(object? obj) => obj is Square other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Square other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(SQ_A1),
            1 => nameof(SQ_B1),
            2 => nameof(SQ_C1),
            3 => nameof(SQ_D1),
            4 => nameof(SQ_E1),
            5 => nameof(SQ_F1),
            6 => nameof(SQ_G1),
            7 => nameof(SQ_H1),
            8 => nameof(SQ_A2),
            9 => nameof(SQ_B2),
            10 => nameof(SQ_C2),
            11 => nameof(SQ_D2),
            12 => nameof(SQ_E2),
            13 => nameof(SQ_F2),
            14 => nameof(SQ_G2),
            15 => nameof(SQ_H2),
            16 => nameof(SQ_A3),
            17 => nameof(SQ_B3),
            18 => nameof(SQ_C3),
            19 => nameof(SQ_D3),
            20 => nameof(SQ_E3),
            21 => nameof(SQ_F3),
            22 => nameof(SQ_G3),
            23 => nameof(SQ_H3),
            24 => nameof(SQ_A4),
            25 => nameof(SQ_B4),
            26 => nameof(SQ_C4),
            27 => nameof(SQ_D4),
            28 => nameof(SQ_E4),
            29 => nameof(SQ_F4),
            30 => nameof(SQ_G4),
            31 => nameof(SQ_H4),
            32 => nameof(SQ_A5),
            33 => nameof(SQ_B5),
            34 => nameof(SQ_C5),
            35 => nameof(SQ_D5),
            36 => nameof(SQ_E5),
            37 => nameof(SQ_F5),
            38 => nameof(SQ_G5),
            39 => nameof(SQ_H5),
            40 => nameof(SQ_A6),
            41 => nameof(SQ_B6),
            42 => nameof(SQ_C6),
            43 => nameof(SQ_D6),
            44 => nameof(SQ_E6),
            45 => nameof(SQ_F6),
            46 => nameof(SQ_G6),
            47 => nameof(SQ_H6),
            48 => nameof(SQ_A7),
            49 => nameof(SQ_B7),
            50 => nameof(SQ_C7),
            51 => nameof(SQ_D7),
            52 => nameof(SQ_E7),
            53 => nameof(SQ_F7),
            54 => nameof(SQ_G7),
            55 => nameof(SQ_H7),
            56 => nameof(SQ_A8),
            57 => nameof(SQ_B8),
            58 => nameof(SQ_C8),
            59 => nameof(SQ_D8),
            60 => nameof(SQ_E8),
            61 => nameof(SQ_F8),
            62 => nameof(SQ_G8),
            63 => nameof(SQ_H8),
            _ => nameof(SQ_NONE)
        };
    }
}
