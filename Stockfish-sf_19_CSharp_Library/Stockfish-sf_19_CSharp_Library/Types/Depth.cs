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
    /// <summary>Profundidad inmutable con el dominio int completo del alias original.</summary>
    /// <remarks>SF19-000725. Default representa DEPTH_QS. La aritmética detecta desbordamiento.</remarks>
    public readonly struct Depth : IEquatable<Depth>, IComparable<Depth>
    {
        public static readonly Depth DEPTH_QS = 0;
        public static readonly Depth DEPTH_UNSEARCHED = -2;
        public static readonly Depth DEPTH_NONE = -3;

        private readonly int value;

        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Conserva cualquier int, sin imponer las restricciones locales de búsqueda o TT.</summary>
        public Depth(int value) => this.value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Depth left, Depth right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Depth left, Depth right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Depth left, Depth right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Depth left, Depth right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Depth left, Depth right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Depth left, Depth right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Depth left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Depth left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Depth left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Depth left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Depth left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Depth left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Depth right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Depth right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Depth right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Depth right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Depth right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Depth right) => left >= right.value;

        // El alias C++ es int: resultados intermedios int, sin reconstruir Depth.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Depth left, Depth right) => checked(left.value + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Depth left, Depth right) => checked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Depth left, Depth right) => checked(left.value * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Depth left, Depth right) => checked(left.value / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Depth left, Depth right) => Remainder(left.value, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Depth left, Depth right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Depth left, Depth right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Depth left, Depth right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Depth left, int right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Depth left, int right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Depth left, int right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Depth left, int right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Depth left, int right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Depth left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Depth left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Depth left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(int left, Depth right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(int left, Depth right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(int left, Depth right) => checked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(int left, Depth right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(int left, Depth right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Depth right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Depth right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Depth right) => left ^ right.value;

        // byte necesita sobrecargas exactas para evitar ambigüedad de conversiones.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Depth left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Depth left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Depth left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Depth left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Depth left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Depth left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Depth left, byte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Depth left, byte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Depth left, byte right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Depth left, byte right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Depth left, byte right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Depth left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Depth left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Depth left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Depth right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Depth right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Depth right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Depth right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Depth right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Depth right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(byte left, Depth right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(byte left, Depth right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(byte left, Depth right) => checked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(byte left, Depth right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(byte left, Depth right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Depth right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Depth right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Depth right) => left ^ right.value;

        // C++ promociona bool a int; estas sobrecargas conservan las expresiones con Depth.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Depth left, bool right) => checked(left.value + (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Depth left, bool right) => checked(left.value - (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Depth left, bool right) => checked(left.value * (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(bool left, Depth right) => checked((left ? 1 : 0) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(bool left, Depth right) => checked((left ? 1 : 0) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(bool left, Depth right) => checked((left ? 1 : 0) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Depth depth) => depth.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Depth depth) => checked(-depth.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Depth depth) => ~depth.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Depth depth, int count) => depth.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Depth depth, int count) => depth.value >> count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Depth operator ++(Depth depth) => new Depth(checked(depth.value + 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Depth operator --(Depth depth) => new Depth(checked(depth.value - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Depth(int value) => new Depth(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Depth depth) => depth.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Depth other) => value == other.value;

        public override bool Equals(object? obj) => obj is Depth other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Depth other) => value.CompareTo(other.value);

        // C# runtimes can differ at MinValue % -1; make the chosen policy explicit.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Remainder(int left, int right)
        {
            if (left == int.MinValue && right == -1)
                throw new OverflowException("El cociente int.MinValue / -1 no es representable.");
            return left % right;
        }

        public override string ToString() => value.ToString();
    }
}
