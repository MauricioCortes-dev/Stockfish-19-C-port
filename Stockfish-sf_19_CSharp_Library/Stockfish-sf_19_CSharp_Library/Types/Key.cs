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
    /// <summary>Identidad de clave de 64 bits sin signo; toda la aritmética es modular.</summary>
    /// <remarks>SF19-000708. Conserva todo u64, incluido cero; no calcula una posición Zobrist.</remarks>
    public readonly struct Key : IEquatable<Key>, IComparable<Key>
    {
        private readonly ulong value;

        public ulong Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        public Key(ulong value) => this.value = value;
        public Key(long value) => this.value = unchecked((ulong)value);
        public Key(uint value) => this.value = value;
        public Key(int value) => this.value = unchecked((ulong)value);
        public Key(ushort value) => this.value = value;
        public Key(short value) => this.value = unchecked((ulong)value);
        public Key(byte value) => this.value = value;
        public Key(sbyte value) => this.value = unchecked((ulong)value);

        /// <summary>SF19-000758: mezcla congruencial original, módulo 2^64.</summary>
        public static Key MakeKey(ulong seed) => unchecked(seed * 6364136223846793005UL + 1442695040888963407UL);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, Key right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, Key right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, Key right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, Key right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, Key right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, Key right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, Key right) => unchecked(left.value + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, Key right) => unchecked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, Key right) => unchecked(left.value * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, Key right) => left.value / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, Key right) => left.value % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, Key right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, Key right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, Key right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, ulong right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, ulong right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, ulong right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, ulong right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, ulong right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, ulong right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, ulong right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, ulong right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, ulong right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, ulong right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, ulong right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, ulong right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, ulong right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, ulong right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ulong left, Key right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ulong left, Key right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ulong left, Key right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ulong left, Key right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ulong left, Key right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ulong left, Key right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(ulong left, Key right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(ulong left, Key right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(ulong left, Key right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(ulong left, Key right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(ulong left, Key right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(ulong left, Key right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(ulong left, Key right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(ulong left, Key right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, long right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, long right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, long right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, long right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, long right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, long right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, long right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, long right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, long right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, long right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, long right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, long right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, long right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, long right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(long left, Key right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(long left, Key right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(long left, Key right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(long left, Key right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(long left, Key right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(long left, Key right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(long left, Key right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(long left, Key right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(long left, Key right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(long left, Key right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(long left, Key right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(long left, Key right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(long left, Key right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(long left, Key right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, uint right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, uint right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, uint right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, uint right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, uint right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, uint right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, uint right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, uint right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, uint right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, uint right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, uint right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, uint right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, uint right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, uint right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint left, Key right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint left, Key right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(uint left, Key right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(uint left, Key right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(uint left, Key right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(uint left, Key right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(uint left, Key right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(uint left, Key right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(uint left, Key right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(uint left, Key right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(uint left, Key right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(uint left, Key right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(uint left, Key right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(uint left, Key right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, int right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, int right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, int right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, int right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, int right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, int right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, int right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, int right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, int right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, int right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, int right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, int right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, int right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, int right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Key right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Key right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Key right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Key right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Key right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Key right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(int left, Key right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(int left, Key right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(int left, Key right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(int left, Key right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(int left, Key right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(int left, Key right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(int left, Key right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(int left, Key right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, ushort right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, ushort right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, ushort right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, ushort right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, ushort right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, ushort right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, ushort right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, ushort right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, ushort right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, ushort right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, ushort right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, ushort right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, ushort right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, ushort right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ushort left, Key right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ushort left, Key right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ushort left, Key right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ushort left, Key right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ushort left, Key right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ushort left, Key right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(ushort left, Key right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(ushort left, Key right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(ushort left, Key right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(ushort left, Key right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(ushort left, Key right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(ushort left, Key right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(ushort left, Key right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(ushort left, Key right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, short right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, short right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, short right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, short right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, short right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, short right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, short right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, short right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, short right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, short right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, short right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, short right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, short right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, short right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(short left, Key right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(short left, Key right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(short left, Key right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(short left, Key right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(short left, Key right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(short left, Key right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(short left, Key right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(short left, Key right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(short left, Key right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(short left, Key right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(short left, Key right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(short left, Key right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(short left, Key right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(short left, Key right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, byte right) => unchecked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, byte right) => unchecked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, byte right) => unchecked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, byte right) => left.value / right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, byte right) => left.value % right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Key right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Key right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Key right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Key right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Key right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Key right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(byte left, Key right) => unchecked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(byte left, Key right) => unchecked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(byte left, Key right) => unchecked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(byte left, Key right) => left / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(byte left, Key right) => left % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(byte left, Key right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(byte left, Key right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(byte left, Key right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Key left, sbyte right) => left.value == SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Key left, sbyte right) => left.value != SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Key left, sbyte right) => left.value < SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Key left, sbyte right) => left.value > SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Key left, sbyte right) => left.value <= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Key left, sbyte right) => left.value >= SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, sbyte right) => unchecked(left.value + SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, sbyte right) => unchecked(left.value - SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, sbyte right) => unchecked(left.value * SignedBits(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(Key left, sbyte right) => left.value / SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(Key left, sbyte right) => left.value % SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(Key left, sbyte right) => left.value & SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(Key left, sbyte right) => left.value | SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(Key left, sbyte right) => left.value ^ SignedBits(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(sbyte left, Key right) => SignedBits(left) == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sbyte left, Key right) => SignedBits(left) != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sbyte left, Key right) => SignedBits(left) < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sbyte left, Key right) => SignedBits(left) > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sbyte left, Key right) => SignedBits(left) <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sbyte left, Key right) => SignedBits(left) >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(sbyte left, Key right) => unchecked(SignedBits(left) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(sbyte left, Key right) => unchecked(SignedBits(left) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(sbyte left, Key right) => unchecked(SignedBits(left) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator /(sbyte left, Key right) => SignedBits(left) / right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator %(sbyte left, Key right) => SignedBits(left) % right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator &(sbyte left, Key right) => SignedBits(left) & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator |(sbyte left, Key right) => SignedBits(left) | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(sbyte left, Key right) => SignedBits(left) ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key left, bool right) => unchecked(left.value + (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key left, bool right) => unchecked(left.value - (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(Key left, bool right) => unchecked(left.value * (right ? 1UL : 0UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(bool left, Key right) => unchecked((left ? 1UL : 0UL) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(bool left, Key right) => unchecked((left ? 1UL : 0UL) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator *(bool left, Key right) => unchecked((left ? 1UL : 0UL) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator +(Key key) => key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator -(Key key) => unchecked(0UL - key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ~(Key key) => ~key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator <<(Key key, int count) => key.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator >>(Key key, int count) => key.value >> count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Key operator ++(Key key) => new Key(unchecked(key.value + 1UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Key operator --(Key key) => new Key(unchecked(key.value - 1UL));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(ulong value) => new Key(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(long value) => new Key(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(uint value) => new Key(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(int value) => new Key(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(ushort value) => new Key(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(short value) => new Key(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(byte value) => new Key(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Key(sbyte value) => new Key(unchecked((ulong)value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(Key key) => key.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator long(Key key) => unchecked((long)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(Key key) => unchecked((uint)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Key key) => unchecked((int)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ushort(Key key) => unchecked((ushort)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator short(Key key) => unchecked((short)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator byte(Key key) => unchecked((byte)key.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator sbyte(Key key) => unchecked((sbyte)key.value);

        // La extensión de signo a 64 bits es intencional y precede a la reducción unsigned.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SignedBits(long value) => unchecked((ulong)value);

        public bool Equals(Key other) => value == other.value;
        public override bool Equals(object? obj) => obj is Key other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public int CompareTo(Key other) => value.CompareTo(other.value);
        public override string ToString() => value.ToString();
    }
}
