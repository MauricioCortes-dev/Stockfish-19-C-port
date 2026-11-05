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
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Stockfish_sf_19_CSharp_Library.Types
{
    /// <summary>Desplazamiento inmutable de un byte con signo, no limitado a ocho constantes.</summary>
    /// <remarks>SF19-000727/736/737/757. Default representa el desplazamiento cero.</remarks>
    public readonly struct Direction : IEquatable<Direction>, IComparable<Direction>
    {
        public static readonly Direction NORTH = 8;
        public static readonly Direction EAST = 1;
        public static readonly Direction SOUTH = -NORTH;
        public static readonly Direction WEST = -EAST;
        public static readonly Direction NORTH_EAST = NORTH + EAST;
        public static readonly Direction SOUTH_EAST = SOUTH + EAST;
        public static readonly Direction SOUTH_WEST = SOUTH + WEST;
        public static readonly Direction NORTH_WEST = NORTH + WEST;

        private readonly sbyte value;
        public sbyte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite todo i8, incluidos cero y desplazamientos compuestos.</summary>
        public Direction(int value)
        {
            if (value < sbyte.MinValue || value > sbyte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "El desplazamiento debe estar entre -128 y 127.");
            this.value = (sbyte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Direction left, Direction right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Direction left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Direction right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Direction left, sbyte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(sbyte left, Direction right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Direction left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Direction right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Direction left, Direction right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Direction left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Direction right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Direction left, sbyte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sbyte left, Direction right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Direction left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Direction right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Direction left, Direction right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Direction left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Direction right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Direction left, sbyte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sbyte left, Direction right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Direction left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Direction right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Direction left, Direction right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Direction left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Direction right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Direction left, sbyte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sbyte left, Direction right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Direction left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Direction right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Direction left, Direction right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Direction left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Direction right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Direction left, sbyte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sbyte left, Direction right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Direction left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Direction right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Direction left, Direction right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Direction left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Direction right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Direction left, sbyte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sbyte left, Direction right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Direction left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Direction right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator +(Direction left, Direction right) => new Direction(checked(left.value + right.value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Direction left, int right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(int left, Direction right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Direction left, sbyte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(sbyte left, Direction right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Direction left, byte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(byte left, Direction right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction left, Direction right) => checked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction left, int right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(int left, Direction right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction left, sbyte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(sbyte left, Direction right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction left, byte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(byte left, Direction right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(Direction left, Direction right) => new Direction(checked(left.value * right.value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Direction left, int right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(int left, Direction right) => new Direction(checked(left * right.value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Direction left, sbyte right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(sbyte left, Direction right) => new Direction(checked(left * right.value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Direction left, byte right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(byte left, Direction right) => new Direction(checked(left * right.value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Direction left, Direction right) => checked(left.value / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Direction left, int right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(int left, Direction right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Direction left, sbyte right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(sbyte left, Direction right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Direction left, byte right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(byte left, Direction right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Direction left, Direction right) => Remainder(left.value, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Direction left, int right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(int left, Direction right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Direction left, sbyte right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(sbyte left, Direction right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Direction left, byte right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(byte left, Direction right) => Remainder(left, right.value);

        // La extensión de signo a int es deliberada y conserva la promoción C++.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Direction left, Direction right) => (int)left.value & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Direction left, int right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Direction right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Direction left, sbyte right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(sbyte left, Direction right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Direction left, byte right) => (int)left.value & (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Direction right) => (int)left & (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Direction left, Direction right) => (int)left.value | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Direction left, int right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Direction right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Direction left, sbyte right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(sbyte left, Direction right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Direction left, byte right) => (int)left.value | (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Direction right) => (int)left | (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Direction left, Direction right) => (int)left.value ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Direction left, int right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Direction right) => (int)left ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Direction left, sbyte right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(sbyte left, Direction right) => (int)left ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Direction left, byte right) => (int)left.value ^ (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Direction right) => (int)left ^ (int)right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Direction direction) => +direction.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Direction direction) => -direction.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Direction direction) => ~direction.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Direction direction, int count) => direction.value << count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Direction direction, int count) => direction.value >> count;

        // C++17 no define INT_MIN % -1. Rechazarlo explícitamente, como Depth.
        private static int Remainder(int left, int right)
        {
            if (left == int.MinValue && right == -1) throw new OverflowException();
            return left % right;
        }

        /// <summary>Conserva c == WHITE ? NORTH : SOUTH, incluido COLOR_NB.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnPush(Color color) => color == Color.WHITE ? NORTH : SOUTH;

        public static implicit operator Direction(int value) => new Direction(value);
        public static implicit operator Direction(sbyte value) => new Direction(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Direction direction) => direction.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator sbyte(Direction direction) => direction.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Direction other) => this == other;
        public override bool Equals(object? obj) => obj is Direction other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Direction other) => value.CompareTo(other.value);
        public override string ToString() => value switch
        {
            8 => nameof(NORTH), 1 => nameof(EAST), -8 => nameof(SOUTH), -1 => nameof(WEST),
            9 => nameof(NORTH_EAST), -7 => nameof(SOUTH_EAST),
            -9 => nameof(SOUTH_WEST), 7 => nameof(NORTH_WEST),
            _ => value.ToString(CultureInfo.InvariantCulture)
        };
    }
}
