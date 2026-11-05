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


#pragma warning disable CS0675 // Sign extension is required by the C++ usual arithmetic conversions.
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numerics
{
    /// <summary>C++ i64; exact storage and C++ integral promotions. See Numerics.md.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct i64 : IEquatable<i64>
    {
        public long Value { get; }
        public i64(long value) => Value = value;
        public static implicit operator long(i64 value) => value.Value;
        public static implicit operator i64(byte value) => new i64(unchecked((long)value));
        public static implicit operator i64(sbyte value) => new i64(unchecked((long)value));
        public static implicit operator i64(ushort value) => new i64(unchecked((long)value));
        public static implicit operator i64(short value) => new i64(unchecked((long)value));
        public static implicit operator i64(uint value) => new i64(unchecked((long)value));
        public static implicit operator i64(int value) => new i64(unchecked((long)value));
        public static implicit operator i64(ulong value) => new i64(unchecked((long)value));
        public static implicit operator i64(long value) => new i64(unchecked((long)value));
        public bool Equals(i64 other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is i64 other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public static long operator +(i64 a) => checked(+(long)a.Value);
        public static long operator -(i64 a) => checked(-(long)a.Value);
        public static long operator ~(i64 a) => unchecked(~(long)a.Value);
        public static i64 operator ++(i64 a) => new i64(checked((long)(a.Value + 1)));
        public static i64 operator --(i64 a) => new i64(checked((long)(a.Value - 1)));
        public static long operator <<(i64 a, int count) => (long)IntegerResult.Left(IntegerResult.From((long)a.Value), count);
        public static long operator >>(i64 a, int count) => (long)IntegerResult.Right(IntegerResult.From((long)a.Value), count);
        public static long operator +(i64 a, i64 b) => checked(unchecked((long)a.Value) + unchecked((long)b.Value));
        public static long operator -(i64 a, i64 b) => checked(unchecked((long)a.Value) - unchecked((long)b.Value));
        public static long operator *(i64 a, i64 b) => checked(unchecked((long)a.Value) * unchecked((long)b.Value));
        public static long operator /(i64 a, i64 b) => checked(unchecked((long)a.Value) / unchecked((long)b.Value));
        public static long operator %(i64 a, i64 b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b.Value));
        public static long operator &(i64 a, i64 b) => unchecked(unchecked((long)a.Value) & unchecked((long)b.Value));
        public static long operator |(i64 a, i64 b) => unchecked(unchecked((long)a.Value) | unchecked((long)b.Value));
        public static long operator ^(i64 a, i64 b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b.Value));
        public static bool operator ==(i64 a, i64 b) => unchecked((long)a.Value) == unchecked((long)b.Value);
        public static bool operator !=(i64 a, i64 b) => unchecked((long)a.Value) != unchecked((long)b.Value);
        public static bool operator <(i64 a, i64 b) => unchecked((long)a.Value) < unchecked((long)b.Value);
        public static bool operator <=(i64 a, i64 b) => unchecked((long)a.Value) <= unchecked((long)b.Value);
        public static bool operator >(i64 a, i64 b) => unchecked((long)a.Value) > unchecked((long)b.Value);
        public static bool operator >=(i64 a, i64 b) => unchecked((long)a.Value) >= unchecked((long)b.Value);
        public static long operator +(i64 a, byte b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, byte b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, byte b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, byte b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, byte b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, byte b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, byte b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, byte b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, byte b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, byte b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, byte b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, byte b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, byte b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, byte b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(byte a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(byte a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(byte a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(byte a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(byte a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(byte a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(byte a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(byte a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(byte a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(byte a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(byte a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(byte a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(byte a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(byte a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static long operator +(i64 a, sbyte b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, sbyte b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, sbyte b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, sbyte b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, sbyte b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, sbyte b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, sbyte b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, sbyte b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, sbyte b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, sbyte b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, sbyte b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, sbyte b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, sbyte b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, sbyte b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(sbyte a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(sbyte a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(sbyte a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(sbyte a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(sbyte a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(sbyte a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(sbyte a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(sbyte a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(sbyte a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(sbyte a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(sbyte a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(sbyte a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(sbyte a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(sbyte a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static long operator +(i64 a, ushort b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, ushort b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, ushort b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, ushort b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, ushort b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, ushort b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, ushort b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, ushort b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, ushort b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, ushort b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, ushort b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, ushort b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, ushort b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, ushort b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(ushort a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(ushort a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(ushort a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(ushort a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(ushort a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(ushort a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(ushort a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(ushort a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(ushort a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(ushort a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(ushort a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(ushort a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(ushort a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(ushort a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static long operator +(i64 a, short b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, short b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, short b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, short b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, short b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, short b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, short b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, short b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, short b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, short b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, short b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, short b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, short b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, short b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(short a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(short a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(short a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(short a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(short a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(short a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(short a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(short a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(short a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(short a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(short a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(short a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(short a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(short a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static long operator +(i64 a, uint b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, uint b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, uint b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, uint b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, uint b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, uint b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, uint b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, uint b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, uint b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, uint b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, uint b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, uint b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, uint b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, uint b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(uint a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(uint a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(uint a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(uint a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(uint a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(uint a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(uint a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(uint a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(uint a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(uint a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(uint a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(uint a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(uint a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(uint a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static long operator +(i64 a, int b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, int b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, int b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, int b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, int b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, int b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, int b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, int b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, int b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, int b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, int b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, int b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, int b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, int b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(int a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(int a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(int a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(int a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(int a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(int a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(int a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(int a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(int a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(int a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(int a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(int a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(int a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(int a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
        public static ulong operator +(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) + unchecked((ulong)b));
        public static ulong operator -(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) - unchecked((ulong)b));
        public static ulong operator *(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) * unchecked((ulong)b));
        public static ulong operator /(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) / unchecked((ulong)b));
        public static ulong operator %(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) % unchecked((ulong)b));
        public static ulong operator &(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) & unchecked((ulong)b));
        public static ulong operator |(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) | unchecked((ulong)b));
        public static ulong operator ^(i64 a, ulong b) => unchecked(unchecked((ulong)a.Value) ^ unchecked((ulong)b));
        public static bool operator ==(i64 a, ulong b) => unchecked((ulong)a.Value) == unchecked((ulong)b);
        public static bool operator !=(i64 a, ulong b) => unchecked((ulong)a.Value) != unchecked((ulong)b);
        public static bool operator <(i64 a, ulong b) => unchecked((ulong)a.Value) < unchecked((ulong)b);
        public static bool operator <=(i64 a, ulong b) => unchecked((ulong)a.Value) <= unchecked((ulong)b);
        public static bool operator >(i64 a, ulong b) => unchecked((ulong)a.Value) > unchecked((ulong)b);
        public static bool operator >=(i64 a, ulong b) => unchecked((ulong)a.Value) >= unchecked((ulong)b);
        public static ulong operator +(ulong a, i64 b) => unchecked(unchecked((ulong)a) + unchecked((ulong)b.Value));
        public static ulong operator -(ulong a, i64 b) => unchecked(unchecked((ulong)a) - unchecked((ulong)b.Value));
        public static ulong operator *(ulong a, i64 b) => unchecked(unchecked((ulong)a) * unchecked((ulong)b.Value));
        public static ulong operator /(ulong a, i64 b) => unchecked(unchecked((ulong)a) / unchecked((ulong)b.Value));
        public static ulong operator %(ulong a, i64 b) => unchecked(unchecked((ulong)a) % unchecked((ulong)b.Value));
        public static ulong operator &(ulong a, i64 b) => unchecked(unchecked((ulong)a) & unchecked((ulong)b.Value));
        public static ulong operator |(ulong a, i64 b) => unchecked(unchecked((ulong)a) | unchecked((ulong)b.Value));
        public static ulong operator ^(ulong a, i64 b) => unchecked(unchecked((ulong)a) ^ unchecked((ulong)b.Value));
        public static bool operator ==(ulong a, i64 b) => unchecked((ulong)a) == unchecked((ulong)b.Value);
        public static bool operator !=(ulong a, i64 b) => unchecked((ulong)a) != unchecked((ulong)b.Value);
        public static bool operator <(ulong a, i64 b) => unchecked((ulong)a) < unchecked((ulong)b.Value);
        public static bool operator <=(ulong a, i64 b) => unchecked((ulong)a) <= unchecked((ulong)b.Value);
        public static bool operator >(ulong a, i64 b) => unchecked((ulong)a) > unchecked((ulong)b.Value);
        public static bool operator >=(ulong a, i64 b) => unchecked((ulong)a) >= unchecked((ulong)b.Value);
        public static long operator +(i64 a, long b) => checked(unchecked((long)a.Value) + unchecked((long)b));
        public static long operator -(i64 a, long b) => checked(unchecked((long)a.Value) - unchecked((long)b));
        public static long operator *(i64 a, long b) => checked(unchecked((long)a.Value) * unchecked((long)b));
        public static long operator /(i64 a, long b) => checked(unchecked((long)a.Value) / unchecked((long)b));
        public static long operator %(i64 a, long b) => (unchecked((long)a.Value) == long.MinValue && unchecked((long)b) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a.Value) % unchecked((long)b));
        public static long operator &(i64 a, long b) => unchecked(unchecked((long)a.Value) & unchecked((long)b));
        public static long operator |(i64 a, long b) => unchecked(unchecked((long)a.Value) | unchecked((long)b));
        public static long operator ^(i64 a, long b) => unchecked(unchecked((long)a.Value) ^ unchecked((long)b));
        public static bool operator ==(i64 a, long b) => unchecked((long)a.Value) == unchecked((long)b);
        public static bool operator !=(i64 a, long b) => unchecked((long)a.Value) != unchecked((long)b);
        public static bool operator <(i64 a, long b) => unchecked((long)a.Value) < unchecked((long)b);
        public static bool operator <=(i64 a, long b) => unchecked((long)a.Value) <= unchecked((long)b);
        public static bool operator >(i64 a, long b) => unchecked((long)a.Value) > unchecked((long)b);
        public static bool operator >=(i64 a, long b) => unchecked((long)a.Value) >= unchecked((long)b);
        public static long operator +(long a, i64 b) => checked(unchecked((long)a) + unchecked((long)b.Value));
        public static long operator -(long a, i64 b) => checked(unchecked((long)a) - unchecked((long)b.Value));
        public static long operator *(long a, i64 b) => checked(unchecked((long)a) * unchecked((long)b.Value));
        public static long operator /(long a, i64 b) => checked(unchecked((long)a) / unchecked((long)b.Value));
        public static long operator %(long a, i64 b) => (unchecked((long)a) == long.MinValue && unchecked((long)b.Value) == -1) ? throw new OverflowException("Undefined C++ signed remainder.") : checked(unchecked((long)a) % unchecked((long)b.Value));
        public static long operator &(long a, i64 b) => unchecked(unchecked((long)a) & unchecked((long)b.Value));
        public static long operator |(long a, i64 b) => unchecked(unchecked((long)a) | unchecked((long)b.Value));
        public static long operator ^(long a, i64 b) => unchecked(unchecked((long)a) ^ unchecked((long)b.Value));
        public static bool operator ==(long a, i64 b) => unchecked((long)a) == unchecked((long)b.Value);
        public static bool operator !=(long a, i64 b) => unchecked((long)a) != unchecked((long)b.Value);
        public static bool operator <(long a, i64 b) => unchecked((long)a) < unchecked((long)b.Value);
        public static bool operator <=(long a, i64 b) => unchecked((long)a) <= unchecked((long)b.Value);
        public static bool operator >(long a, i64 b) => unchecked((long)a) > unchecked((long)b.Value);
        public static bool operator >=(long a, i64 b) => unchecked((long)a) >= unchecked((long)b.Value);
    }
}
