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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numerics
{
    /// <summary>A C++ promoted integer whose static type depends on the process ABI.
    /// Fixed-width operands return CLR primitives; only native-width mixtures need this value.</summary>
    public readonly struct IntegerResult : IEquatable<IntegerResult>
    {
        private readonly ulong bits;
        private readonly int width;
        private readonly bool signed;
        public int Width => width == 0 ? 32 : width;
        public bool IsSigned => width == 0 || signed;
        private IntegerResult(ulong bits, int width, bool signed)
        { this.bits = width == 32 ? (uint)bits : bits; this.width = width; this.signed = signed; }
        public static IntegerResult From(int value) => new IntegerResult(unchecked((ulong)value), 32, true);
        public static IntegerResult From(uint value) => new IntegerResult(value, 32, false);
        public static IntegerResult From(long value) => new IntegerResult(unchecked((ulong)value), 64, true);
        public static IntegerResult From(ulong value) => new IntegerResult(value, 64, false);
        public static IntegerResult Native(ulong bits, bool signed) => new IntegerResult(bits, IntPtr.Size * 8, signed);
        public static implicit operator IntegerResult(u8 value) => From((int)value.Value);
        public static implicit operator IntegerResult(i8 value) => From((int)value.Value);
        public static implicit operator IntegerResult(u16 value) => From((int)value.Value);
        public static implicit operator IntegerResult(i16 value) => From((int)value.Value);
        public static implicit operator IntegerResult(u32 value) => From((uint)value.Value);
        public static implicit operator IntegerResult(i32 value) => From((int)value.Value);
        public static implicit operator IntegerResult(u64 value) => From((ulong)value.Value);
        public static implicit operator IntegerResult(i64 value) => From((long)value.Value);
        public static implicit operator IntegerResult(byte value) => From((int)value);
        public static implicit operator IntegerResult(sbyte value) => From((int)value);
        public static implicit operator IntegerResult(ushort value) => From((int)value);
        public static implicit operator IntegerResult(short value) => From((int)value);
        public static implicit operator IntegerResult(int value) => From(value);
        public static implicit operator IntegerResult(uint value) => From(value);
        public static implicit operator IntegerResult(long value) => From(value);
        public static implicit operator IntegerResult(ulong value) => From(value);
        public static implicit operator IntegerResult(Numa.CpuIndex value) => value.Number;
        public static implicit operator IntegerResult(Numa.NumaIndex value) => value.Number;
        public static implicit operator IntegerResult(usize value) => value.Number;
        public static implicit operator IntegerResult(isize value) => value.Number;
        private long Signed => Width == 32 ? unchecked((int)bits) : unchecked((long)bits);
        private ulong Extended => IsSigned ? unchecked((ulong)Signed) : bits;
        public static explicit operator int(IntegerResult value) => unchecked((int)value.bits);
        public static explicit operator uint(IntegerResult value) => unchecked((uint)value.bits);
        public static explicit operator long(IntegerResult value) => unchecked((long)value.Extended);
        public static explicit operator ulong(IntegerResult value) => value.Extended;
        private static IntegerResult Common(IntegerResult a, IntegerResult b)
        {
            int width = Math.Max(a.Width, b.Width);
            bool signed = a.IsSigned == b.IsSigned ? a.IsSigned
                : (a.IsSigned ? a.Width > b.Width : b.Width > a.Width);
            return new IntegerResult(0, width, signed);
        }
        private static IntegerResult ConvertTo(IntegerResult a, IntegerResult common) => new IntegerResult(a.Extended, common.Width, common.IsSigned);
        private static IntegerResult Binary(IntegerResult a, IntegerResult b, char op)
        {
            var type = Common(a, b); a = ConvertTo(a, type); b = ConvertTo(b, type);
            ulong result;
            if (op == '&') result = a.bits & b.bits;
            else if (op == '|') result = a.bits | b.bits;
            else if (op == '^') result = a.bits ^ b.bits;
            else if (!type.IsSigned)
            {
                switch (op)
                {
                    case '+': result = unchecked(a.bits + b.bits); break;
                    case '-': result = unchecked(a.bits - b.bits); break;
                    case '*': result = unchecked(a.bits * b.bits); break;
                    case '/': result = a.bits / b.bits; break;
                    default: result = a.bits % b.bits; break;
                }
            }
            else
            {
                long x = a.Signed, y = b.Signed, z;
                if ((op == '/' || op == '%') && y == -1 && x == (type.Width == 32 ? int.MinValue : long.MinValue))
                    throw new OverflowException("Undefined C++ signed division.");
                switch (op)
                {
                    case '+': z = checked(x + y); break;
                    case '-': z = checked(x - y); break;
                    case '*': z = checked(x * y); break;
                    case '/': z = x / y; break;
                    default: z = x % y; break;
                }
                if (type.Width == 32) z = checked((int)z);
                result = unchecked((ulong)z);
            }
            return new IntegerResult(result, type.Width, type.IsSigned);
        }
        public IntegerResult Plus() => this;
        public IntegerResult Negate() => Binary(new IntegerResult(0, Width, IsSigned), this, '-');
        public IntegerResult Complement() => new IntegerResult(~bits, Width, IsSigned);
        public static IntegerResult Left(IntegerResult a, int count)
        {
            RequireShift(a, count);
            if (a.IsSigned && (a.Signed < 0 || a.bits > (a.Width == 32 ? uint.MaxValue : ulong.MaxValue) >> count))
                throw new OverflowException("Undefined C++17 signed left shift.");
            return new IntegerResult(a.bits << count, a.Width, a.IsSigned);
        }
        public static IntegerResult Right(IntegerResult a, int count)
        {
            RequireShift(a, count);
            return new IntegerResult(a.IsSigned ? unchecked((ulong)(a.Signed >> count)) : a.bits >> count, a.Width, a.IsSigned);
        }
        private static void RequireShift(IntegerResult a, int count)
        { if (count < 0 || count >= a.Width) throw new ArgumentOutOfRangeException(nameof(count), "C++ shift count must be below the promoted width."); }
        private static int Compare(IntegerResult a, IntegerResult b)
        {
            var type = Common(a,b); a = ConvertTo(a,type); b = ConvertTo(b,type);
            return type.IsSigned ? a.Signed.CompareTo(b.Signed) : a.bits.CompareTo(b.bits);
        }
        // .NET equality includes the promoted type; C++ operator== applies usual conversions.
        public bool Equals(IntegerResult other) => bits == other.bits && Width == other.Width && IsSigned == other.IsSigned;
        public override bool Equals(object? obj) => obj is IntegerResult other && Equals(other);
        public override int GetHashCode() => bits.GetHashCode() ^ Width ^ IsSigned.GetHashCode();
        public override string ToString() => IsSigned ? Signed.ToString(CultureInfo.InvariantCulture) : bits.ToString(CultureInfo.InvariantCulture);
        public static IntegerResult operator +(IntegerResult a, usize b) => a + b.Number;
        public static IntegerResult operator -(IntegerResult a, usize b) => a - b.Number;
        public static IntegerResult operator *(IntegerResult a, usize b) => a * b.Number;
        public static IntegerResult operator /(IntegerResult a, usize b) => a / b.Number;
        public static IntegerResult operator %(IntegerResult a, usize b) => a % b.Number;
        public static IntegerResult operator &(IntegerResult a, usize b) => a & b.Number;
        public static IntegerResult operator |(IntegerResult a, usize b) => a | b.Number;
        public static IntegerResult operator ^(IntegerResult a, usize b) => a ^ b.Number;
        public static bool operator ==(IntegerResult a, usize b) => a == b.Number;
        public static bool operator !=(IntegerResult a, usize b) => a != b.Number;
        public static bool operator <(IntegerResult a, usize b) => a < b.Number;
        public static bool operator <=(IntegerResult a, usize b) => a <= b.Number;
        public static bool operator >(IntegerResult a, usize b) => a > b.Number;
        public static bool operator >=(IntegerResult a, usize b) => a >= b.Number;
        public static IntegerResult operator +(usize a, IntegerResult b) => a.Number + b;
        public static IntegerResult operator -(usize a, IntegerResult b) => a.Number - b;
        public static IntegerResult operator *(usize a, IntegerResult b) => a.Number * b;
        public static IntegerResult operator /(usize a, IntegerResult b) => a.Number / b;
        public static IntegerResult operator %(usize a, IntegerResult b) => a.Number % b;
        public static IntegerResult operator &(usize a, IntegerResult b) => a.Number & b;
        public static IntegerResult operator |(usize a, IntegerResult b) => a.Number | b;
        public static IntegerResult operator ^(usize a, IntegerResult b) => a.Number ^ b;
        public static bool operator ==(usize a, IntegerResult b) => a.Number == b;
        public static bool operator !=(usize a, IntegerResult b) => a.Number != b;
        public static bool operator <(usize a, IntegerResult b) => a.Number < b;
        public static bool operator <=(usize a, IntegerResult b) => a.Number <= b;
        public static bool operator >(usize a, IntegerResult b) => a.Number > b;
        public static bool operator >=(usize a, IntegerResult b) => a.Number >= b;
        public static IntegerResult operator +(IntegerResult a, isize b) => a + b.Number;
        public static IntegerResult operator -(IntegerResult a, isize b) => a - b.Number;
        public static IntegerResult operator *(IntegerResult a, isize b) => a * b.Number;
        public static IntegerResult operator /(IntegerResult a, isize b) => a / b.Number;
        public static IntegerResult operator %(IntegerResult a, isize b) => a % b.Number;
        public static IntegerResult operator &(IntegerResult a, isize b) => a & b.Number;
        public static IntegerResult operator |(IntegerResult a, isize b) => a | b.Number;
        public static IntegerResult operator ^(IntegerResult a, isize b) => a ^ b.Number;
        public static bool operator ==(IntegerResult a, isize b) => a == b.Number;
        public static bool operator !=(IntegerResult a, isize b) => a != b.Number;
        public static bool operator <(IntegerResult a, isize b) => a < b.Number;
        public static bool operator <=(IntegerResult a, isize b) => a <= b.Number;
        public static bool operator >(IntegerResult a, isize b) => a > b.Number;
        public static bool operator >=(IntegerResult a, isize b) => a >= b.Number;
        public static IntegerResult operator +(isize a, IntegerResult b) => a.Number + b;
        public static IntegerResult operator -(isize a, IntegerResult b) => a.Number - b;
        public static IntegerResult operator *(isize a, IntegerResult b) => a.Number * b;
        public static IntegerResult operator /(isize a, IntegerResult b) => a.Number / b;
        public static IntegerResult operator %(isize a, IntegerResult b) => a.Number % b;
        public static IntegerResult operator &(isize a, IntegerResult b) => a.Number & b;
        public static IntegerResult operator |(isize a, IntegerResult b) => a.Number | b;
        public static IntegerResult operator ^(isize a, IntegerResult b) => a.Number ^ b;
        public static bool operator ==(isize a, IntegerResult b) => a.Number == b;
        public static bool operator !=(isize a, IntegerResult b) => a.Number != b;
        public static bool operator <(isize a, IntegerResult b) => a.Number < b;
        public static bool operator <=(isize a, IntegerResult b) => a.Number <= b;
        public static bool operator >(isize a, IntegerResult b) => a.Number > b;
        public static bool operator >=(isize a, IntegerResult b) => a.Number >= b;
        public static IntegerResult operator +(IntegerResult a, IntegerResult b) => Binary(a,b,'+');
        public static IntegerResult operator -(IntegerResult a, IntegerResult b) => Binary(a,b,'-');
        public static IntegerResult operator *(IntegerResult a, IntegerResult b) => Binary(a,b,'*');
        public static IntegerResult operator /(IntegerResult a, IntegerResult b) => Binary(a,b,'/');
        public static IntegerResult operator %(IntegerResult a, IntegerResult b) => Binary(a,b,'%');
        public static IntegerResult operator &(IntegerResult a, IntegerResult b) => Binary(a,b,'&');
        public static IntegerResult operator |(IntegerResult a, IntegerResult b) => Binary(a,b,'|');
        public static IntegerResult operator ^(IntegerResult a, IntegerResult b) => Binary(a,b,'^');
        public static bool operator ==(IntegerResult a, IntegerResult b) => Compare(a,b) == 0;
        public static bool operator !=(IntegerResult a, IntegerResult b) => Compare(a,b) != 0;
        public static bool operator <(IntegerResult a, IntegerResult b) => Compare(a,b) < 0;
        public static bool operator <=(IntegerResult a, IntegerResult b) => Compare(a,b) <= 0;
        public static bool operator >(IntegerResult a, IntegerResult b) => Compare(a,b) > 0;
        public static bool operator >=(IntegerResult a, IntegerResult b) => Compare(a,b) >= 0;
    }
}
