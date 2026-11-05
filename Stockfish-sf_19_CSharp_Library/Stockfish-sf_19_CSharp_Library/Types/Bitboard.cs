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
    /// <summary>Conjunto de casillas de 64 bits sin signo; toda la aritmética es modular.</summary>
    /// <remarks>SF19-000709. Conserva todo u64, incluido cero; cada bit representa una casilla.</remarks>
    public readonly partial struct Bitboard : IEquatable<Bitboard>, IComparable<Bitboard>
    {
        private readonly ulong value;

        public ulong Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        public Bitboard(ulong value) => this.value = value;
        public Bitboard(long value) => this.value = unchecked((ulong)value);
        public Bitboard(uint value) => this.value = value;
        public Bitboard(int value) => this.value = unchecked((ulong)value);
        public Bitboard(ushort value) => this.value = value;
        public Bitboard(short value) => this.value = unchecked((ulong)value);
        public Bitboard(byte value) => this.value = value;
        public Bitboard(sbyte value) => this.value = unchecked((ulong)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, Bitboard right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, Bitboard right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, Bitboard right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, Bitboard right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, Bitboard right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, Bitboard right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, Bitboard right) => unchecked(left.value + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, Bitboard right) => unchecked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, Bitboard right) => unchecked(left.value * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, Bitboard right) => left.value / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, Bitboard right) => left.value % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, Bitboard right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, Bitboard right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, Bitboard right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, ulong right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, ulong right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, ulong right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, ulong right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, ulong right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, ulong right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, ulong right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, ulong right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, ulong right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, ulong right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, ulong right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, ulong right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, ulong right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, ulong right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ulong left, Bitboard right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ulong left, Bitboard right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ulong left, Bitboard right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ulong left, Bitboard right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ulong left, Bitboard right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ulong left, Bitboard right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(ulong left, Bitboard right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(ulong left, Bitboard right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(ulong left, Bitboard right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(ulong left, Bitboard right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(ulong left, Bitboard right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(ulong left, Bitboard right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(ulong left, Bitboard right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(ulong left, Bitboard right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, long right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, long right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, long right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, long right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, long right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, long right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, long right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, long right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, long right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, long right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, long right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, long right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, long right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, long right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(long left, Bitboard right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(long left, Bitboard right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(long left, Bitboard right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(long left, Bitboard right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(long left, Bitboard right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(long left, Bitboard right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(long left, Bitboard right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(long left, Bitboard right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(long left, Bitboard right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(long left, Bitboard right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(long left, Bitboard right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(long left, Bitboard right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(long left, Bitboard right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(long left, Bitboard right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, uint right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, uint right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, uint right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, uint right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, uint right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, uint right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, uint right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, uint right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, uint right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, uint right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, uint right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, uint right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, uint right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, uint right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint left, Bitboard right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint left, Bitboard right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(uint left, Bitboard right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(uint left, Bitboard right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(uint left, Bitboard right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(uint left, Bitboard right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(uint left, Bitboard right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(uint left, Bitboard right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(uint left, Bitboard right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(uint left, Bitboard right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(uint left, Bitboard right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(uint left, Bitboard right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(uint left, Bitboard right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(uint left, Bitboard right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, int right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, int right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, int right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, int right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, int right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, int right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, int right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, int right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, int right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, int right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, int right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, int right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, int right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, int right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Bitboard right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Bitboard right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Bitboard right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Bitboard right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Bitboard right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Bitboard right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(int left, Bitboard right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(int left, Bitboard right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(int left, Bitboard right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(int left, Bitboard right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(int left, Bitboard right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(int left, Bitboard right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(int left, Bitboard right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(int left, Bitboard right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, ushort right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, ushort right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, ushort right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, ushort right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, ushort right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, ushort right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, ushort right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, ushort right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, ushort right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, ushort right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, ushort right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, ushort right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, ushort right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, ushort right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ushort left, Bitboard right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ushort left, Bitboard right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ushort left, Bitboard right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ushort left, Bitboard right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ushort left, Bitboard right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ushort left, Bitboard right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(ushort left, Bitboard right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(ushort left, Bitboard right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(ushort left, Bitboard right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(ushort left, Bitboard right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(ushort left, Bitboard right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(ushort left, Bitboard right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(ushort left, Bitboard right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(ushort left, Bitboard right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, short right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, short right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, short right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, short right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, short right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, short right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, short right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, short right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, short right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, short right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, short right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, short right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, short right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, short right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(short left, Bitboard right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(short left, Bitboard right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(short left, Bitboard right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(short left, Bitboard right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(short left, Bitboard right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(short left, Bitboard right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(short left, Bitboard right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(short left, Bitboard right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(short left, Bitboard right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(short left, Bitboard right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(short left, Bitboard right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(short left, Bitboard right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(short left, Bitboard right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(short left, Bitboard right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, byte right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, byte right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, byte right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, byte right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, byte right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Bitboard right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Bitboard right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Bitboard right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Bitboard right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Bitboard right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Bitboard right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(byte left, Bitboard right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(byte left, Bitboard right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(byte left, Bitboard right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(byte left, Bitboard right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(byte left, Bitboard right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(byte left, Bitboard right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(byte left, Bitboard right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(byte left, Bitboard right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Bitboard left, sbyte right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Bitboard left, sbyte right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Bitboard left, sbyte right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Bitboard left, sbyte right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Bitboard left, sbyte right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Bitboard left, sbyte right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, sbyte right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, sbyte right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, sbyte right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Bitboard left, sbyte right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Bitboard left, sbyte right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Bitboard left, sbyte right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Bitboard left, sbyte right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Bitboard left, sbyte right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(sbyte left, Bitboard right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sbyte left, Bitboard right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sbyte left, Bitboard right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sbyte left, Bitboard right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sbyte left, Bitboard right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sbyte left, Bitboard right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(sbyte left, Bitboard right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(sbyte left, Bitboard right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(sbyte left, Bitboard right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(sbyte left, Bitboard right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(sbyte left, Bitboard right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(sbyte left, Bitboard right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(sbyte left, Bitboard right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(sbyte left, Bitboard right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard left, bool right) => unchecked(left.value + (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard left, bool right) => unchecked(left.value - (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Bitboard left, bool right) => unchecked(left.value * (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(bool left, Bitboard right) => unchecked((left ? 1UL : 0UL) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(bool left, Bitboard right) => unchecked((left ? 1UL : 0UL) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(bool left, Bitboard right) => unchecked((left ? 1UL : 0UL) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Bitboard key) => key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Bitboard key) => unchecked(0UL - key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ~(Bitboard key) => ~key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator <<(Bitboard key, int count) => key.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator >>(Bitboard key, int count) => key.value >> count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator ++(Bitboard key) => new Bitboard(unchecked(key.value + 1UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator --(Bitboard key) => new Bitboard(unchecked(key.value - 1UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(ulong value) => new Bitboard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(long value) => new Bitboard(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(uint value) => new Bitboard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(int value) => new Bitboard(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(ushort value) => new Bitboard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(short value) => new Bitboard(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(byte value) => new Bitboard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bitboard(sbyte value) => new Bitboard(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(Bitboard key) => key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator long(Bitboard key) => unchecked((long)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(Bitboard key) => unchecked((uint)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Bitboard key) => unchecked((int)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ushort(Bitboard key) => unchecked((ushort)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator short(Bitboard key) => unchecked((short)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator byte(Bitboard key) => unchecked((byte)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator sbyte(Bitboard key) => unchecked((sbyte)key.value);

        public static explicit operator bool(Bitboard board) => board.value != 0;
        public static bool operator true(Bitboard board) => board.value != 0;
        public static bool operator false(Bitboard board) => board.value == 0;
        public static bool operator !(Bitboard board) => board.value == 0;

        // La extensión de signo a 64 bits es intencional y precede a la reducción unsigned.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SignedBits(long value) => unchecked((ulong)value);

        public bool Equals(Bitboard other) => value == other.value;
        public override bool Equals(object? obj) => obj is Bitboard other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public int CompareTo(Bitboard other) => value.CompareTo(other.value);
        public override string ToString() => value.ToString();
    }
}
