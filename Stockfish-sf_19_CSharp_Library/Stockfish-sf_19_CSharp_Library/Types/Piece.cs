/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS piece)

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
    /// <summary>Pieza inmutable con color codificado en el bit 3 y centinela PIECE_NB.</summary>
    /// <remarks>Porteo de SF19-000724. Default representa NO_PIECE.</remarks>
    public readonly struct Piece : IEquatable<Piece>, IComparable<Piece>
    {
        public static readonly Piece NO_PIECE = 0;
        public static readonly Piece W_PAWN = 1;
        public static readonly Piece W_KNIGHT = 2;
        public static readonly Piece W_BISHOP = 3;
        public static readonly Piece W_ROOK = 4;
        public static readonly Piece W_QUEEN = 5;
        public static readonly Piece W_KING = 6;
        public static readonly Piece B_PAWN = 9;
        public static readonly Piece B_KNIGHT = 10;
        public static readonly Piece B_BISHOP = 11;
        public static readonly Piece B_ROOK = 12;
        public static readonly Piece B_QUEEN = 13;
        public static readonly Piece B_KING = 14;
        public static readonly Piece PIECE_NB = 16;

        private readonly byte value;

        /// <summary>Representación original de un byte; admite 0..16, incluidos 7, 8 y 15 sin nombre.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite las representaciones 0..16 y conserva los valores sin nombre del original.</summary>
        public Piece(int value)
        {
            if (value < 0 || value > 16)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre NO_PIECE (0) y PIECE_NB (16).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otra pieza.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, Piece right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, Piece right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Piece left, Piece right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Piece left, Piece right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Piece left, Piece right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Piece left, Piece right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Piece left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Piece left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Piece left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Piece left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Piece right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Piece right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Piece right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Piece right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Piece right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Piece right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Piece left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Piece left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Piece left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Piece left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Piece right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Piece right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Piece right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Piece right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Piece right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Piece right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre Piece.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..16.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Piece left, Piece right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Piece left, Piece right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Piece left, Piece right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Piece left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Piece left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Piece left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Piece right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Piece right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Piece right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Piece left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Piece left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Piece left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Piece right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Piece right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Piece right) => left ^ right.value;

        /// <summary>SF19-000745: invierte el bit de color mediante XOR con 8, como el original.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator ~(Piece piece) => new Piece(piece.value ^ 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Piece piece, int count) => piece.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Piece piece, int count) => piece.value >> count;

        /// <summary>SF19-000748: conserva (color &lt;&lt; 3) + pieceType, validando el resultado.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece MakePiece(Color color, PieceType pieceType) => (color << 3) + pieceType;

        /// <summary>Conserva la validación 0..16 de la entrada.</summary>
        public static implicit operator Piece(byte value) => new Piece(value);

        /// <summary>Conserva la validación 0..16 de la entrada.</summary>
        public static implicit operator Piece(int value) => new Piece(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(Piece piece) => piece.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Piece piece) => piece.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Piece other) => this == other;

        public override bool Equals(object? obj) => obj is Piece other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Piece other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(NO_PIECE),
            1 => nameof(W_PAWN),
            2 => nameof(W_KNIGHT),
            3 => nameof(W_BISHOP),
            4 => nameof(W_ROOK),
            5 => nameof(W_QUEEN),
            6 => nameof(W_KING),
            7 => "7",
            8 => "8",
            9 => nameof(B_PAWN),
            10 => nameof(B_KNIGHT),
            11 => nameof(B_BISHOP),
            12 => nameof(B_ROOK),
            13 => nameof(B_QUEEN),
            14 => nameof(B_KING),
            15 => "15",
            _ => nameof(PIECE_NB)
        };
    }
}
