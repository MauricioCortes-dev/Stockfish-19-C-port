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
    /// <summary>Valor interno de búsqueda con el dominio int completo del alias original.</summary>
    /// <remarks>SF19-000713. Default representa VALUE_ZERO/VALUE_DRAW. La aritmética detecta desbordamiento.</remarks>
    public readonly struct Value : IEquatable<Value>, IComparable<Value>
    {
        public static readonly Value VALUE_ZERO = 0;
        public static readonly Value VALUE_DRAW = 0;
        public static readonly Value VALUE_NONE = 32002;
        public static readonly Value VALUE_INFINITE = 32001;
        public static readonly Value VALUE_MATE = 32000;
        public static readonly Value VALUE_MATE_IN_MAX_PLY = VALUE_MATE - ChessConstants.MAX_PLY;
        public static readonly Value VALUE_MATED_IN_MAX_PLY = -VALUE_MATE_IN_MAX_PLY;
        public static readonly Value VALUE_TB = VALUE_MATE_IN_MAX_PLY - 1;
        public static readonly Value VALUE_TB_WIN_IN_MAX_PLY = VALUE_TB - ChessConstants.MAX_PLY;
        public static readonly Value VALUE_TB_LOSS_IN_MAX_PLY = -VALUE_TB_WIN_IN_MAX_PLY;

        private readonly int value;

        public int RawValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Conserva cualquier int, sin imponer las restricciones locales de búsqueda o TT.</summary>
        public Value(int value) => this.value = value;

        /// <summary>Equivale a is_valid: únicamente excluye VALUE_NONE, sin comprobar un intervalo.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid() => value != VALUE_NONE.value;

        public bool IsWin()
        {
            RequireValid();
            return value >= VALUE_TB_WIN_IN_MAX_PLY.value;
        }

        public bool IsLoss()
        {
            RequireValid();
            return value <= VALUE_TB_LOSS_IN_MAX_PLY.value;
        }

        public bool IsDecisive() => IsWin() || IsLoss();

        public bool IsMate()
        {
            RequireValid();
            return value >= VALUE_MATE_IN_MAX_PLY.value;
        }

        public bool IsMated()
        {
            RequireValid();
            return value <= VALUE_MATED_IN_MAX_PLY.value;
        }

        public bool IsMateOrMated() => IsMate() || IsMated();

        /// <summary>VALUE_MATE - ply; no impone límites de profundidad ajenos al original.</summary>
        public static Value MateIn(int ply) => checked(VALUE_MATE.value - ply);

        /// <summary>-VALUE_MATE + ply; no impone límites de profundidad ajenos al original.</summary>
        public static Value MatedIn(int ply) => checked(-VALUE_MATE.value + ply);

        private void RequireValid()
        {
            if (!IsValid())
                throw new InvalidOperationException("VALUE_NONE no admite clasificación de resultados.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Value left, Value right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Value left, Value right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Value left, Value right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Value left, Value right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Value left, Value right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Value left, Value right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Value left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Value left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Value left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Value left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Value left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Value left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Value right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Value right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Value right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Value right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Value right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Value right) => left >= right.value;

        // El alias C++ es int: resultados intermedios int, sin reconstruir Value.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Value left, Value right) => checked(left.value + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Value left, Value right) => checked(left.value - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Value left, Value right) => checked(left.value * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Value left, Value right) => checked(left.value / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Value left, Value right) => Remainder(left.value, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Value left, Value right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Value left, Value right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Value left, Value right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Value left, int right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Value left, int right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Value left, int right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Value left, int right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Value left, int right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Value left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Value left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Value left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(int left, Value right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(int left, Value right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(int left, Value right) => checked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(int left, Value right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(int left, Value right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Value right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Value right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Value right) => left ^ right.value;

        // search.cpp multiplica valores por Depth; ambos eran alias int en C++.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Value left, Depth right) => checked(left.value * right.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Depth left, Value right) => checked(left.Value * right.value);

        // byte necesita sobrecargas exactas para evitar ambigüedad de conversiones.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Value left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Value left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Value left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Value left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Value left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Value left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Value left, byte right) => checked(left.value + right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Value left, byte right) => checked(left.value - right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Value left, byte right) => checked(left.value * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(Value left, byte right) => checked(left.value / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(Value left, byte right) => Remainder(left.value, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Value left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Value left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Value left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Value right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Value right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Value right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Value right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Value right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Value right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(byte left, Value right) => checked(left + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(byte left, Value right) => checked(left - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(byte left, Value right) => checked(left * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator /(byte left, Value right) => checked(left / right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator %(byte left, Value right) => Remainder(left, right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Value right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Value right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Value right) => left ^ right.value;

        // C++ promociona bool a int; estas sobrecargas conservan las expresiones con Value.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Value left, bool right) => checked(left.value + (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Value left, bool right) => checked(left.value - (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Value left, bool right) => checked(left.value * (right ? 1 : 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(bool left, Value right) => checked((left ? 1 : 0) + right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(bool left, Value right) => checked((left ? 1 : 0) - right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(bool left, Value right) => checked((left ? 1 : 0) * right.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator +(Value score) => score.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator -(Value score) => checked(-score.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Value score) => ~score.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Value score, int count) => score.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Value score, int count) => score.value >> count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Value operator ++(Value score) => new Value(checked(score.value + 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Value operator --(Value score) => new Value(checked(score.value - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Value(int value) => new Value(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Value score) => score.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Value other) => value == other.value;

        public override bool Equals(object? obj) => obj is Value other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Value other) => value.CompareTo(other.value);

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
