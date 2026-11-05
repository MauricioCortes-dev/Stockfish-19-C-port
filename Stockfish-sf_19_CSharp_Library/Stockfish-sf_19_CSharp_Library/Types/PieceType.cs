/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS pieceType)

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
    /// <summary>Tipo de pieza inmutable, incluido el valor intermedio 7 y PIECE_TYPE_NB.</summary>
    /// <remarks>Porteo de SF19-000723. Default representa NO_PIECE_TYPE.</remarks>
    public readonly struct PieceType : IEquatable<PieceType>, IComparable<PieceType>
    {
        public static readonly PieceType NO_PIECE_TYPE = 0;
        public static readonly PieceType PAWN = 1;
        public static readonly PieceType KNIGHT = 2;
        public static readonly PieceType BISHOP = 3;
        public static readonly PieceType ROOK = 4;
        public static readonly PieceType QUEEN = 5;
        public static readonly PieceType KING = 6;
        public static readonly PieceType ALL_PIECES = 0;
        public static readonly PieceType PIECE_TYPE_NB = 8;

        private readonly byte value;

        /// <summary>Representación byte: 0..6 nombrados, intermedio 7 y centinela 8.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite 0..8, incluido 7 para conservar la terminación de los bucles C++.</summary>
        public PieceType(int value)
        {
            if (value < 0 || value > 8)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre NO_PIECE_TYPE (0) y PIECE_TYPE_NB (8).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro tipo de pieza.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PieceType left, PieceType right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PieceType left, PieceType right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(PieceType left, PieceType right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(PieceType left, PieceType right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(PieceType left, PieceType right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(PieceType left, PieceType right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PieceType left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PieceType left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(PieceType left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(PieceType left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(PieceType left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(PieceType left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, PieceType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, PieceType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, PieceType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, PieceType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, PieceType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, PieceType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PieceType left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PieceType left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(PieceType left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(PieceType left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(PieceType left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(PieceType left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, PieceType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, PieceType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, PieceType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, PieceType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, PieceType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, PieceType right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre PieceType.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..8.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(PieceType left, PieceType right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(PieceType left, PieceType right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(PieceType left, PieceType right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(PieceType left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(PieceType left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(PieceType left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, PieceType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, PieceType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, PieceType right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(PieceType left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(PieceType left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(PieceType left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, PieceType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, PieceType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, PieceType right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(PieceType pieceType) => ~pieceType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(PieceType pieceType, int count) => pieceType.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(PieceType pieceType, int count) => pieceType.value >> count;

        // C# usa estos operadores tanto para prefijo como para postfijo.
        // El constructor rechaza atravesar los límites del dominio admitido.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType operator ++(PieceType pieceType) => new PieceType(pieceType.value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType operator --(PieceType pieceType) => new PieceType(pieceType.value - 1);

        /// <summary>SF19-000749: extrae los tres bits del tipo, incluidos valores sin nombre.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PieceType TypeOf(Piece piece) => piece & 7;

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator PieceType(byte value) => new PieceType(value);

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator PieceType(int value) => new PieceType(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(PieceType pieceType) => pieceType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(PieceType pieceType) => pieceType.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PieceType other) => this == other;

        public override bool Equals(object? obj) => obj is PieceType other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(PieceType other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(NO_PIECE_TYPE),
            1 => nameof(PAWN),
            2 => nameof(KNIGHT),
            3 => nameof(BISHOP),
            4 => nameof(ROOK),
            5 => nameof(QUEEN),
            6 => nameof(KING),
            7 => "7",
            _ => nameof(PIECE_TYPE_NB)
        };
    }
}
