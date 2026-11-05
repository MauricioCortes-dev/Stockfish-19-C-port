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

namespace Stockfish_sf_19_CSharp_Library.Types
{
    public readonly struct TimePoint : IEquatable<TimePoint>, IComparable<TimePoint>
    {
        private readonly long value;
        public long Value => value;

        public TimePoint(long value)
        {
            this.value = value;
        }

        public static implicit operator TimePoint(long v) => new TimePoint(v);
        public static implicit operator long (TimePoint v) => v.value;
        public static bool operator ==(TimePoint a, TimePoint b) => a.value == b.value;
        public static bool operator ==(TimePoint a, long b) => a.value == b;
        public static bool operator ==(long a, TimePoint b) => a == b.value;
        public static bool operator !=(TimePoint a, TimePoint b) => a.value != b.value;
        public static bool operator !=(TimePoint a, long b) => a.value != b;
        public static bool operator !=(long a, TimePoint b) => a != b.value;
        public static bool operator <(TimePoint a, TimePoint b) => a.value < b.value;
        public static bool operator <(TimePoint a, long b) => a.value < b;
        public static bool operator <(long a, TimePoint b) => a < b.value;
        public static bool operator>(TimePoint a, TimePoint b) => a.value > b.value;
        public static bool operator>(TimePoint a, long b) => a.value > b;
        public static bool operator>(long a, TimePoint b) => a > b.value;
        public static bool operator <=(TimePoint a, TimePoint b) => a.value <= b.value;
        public static bool operator <=(TimePoint a, long b) => a.value <= b;
        public static bool operator <=(long a, TimePoint b) => a <= b.value;
        public static bool operator >=(TimePoint a, TimePoint b) => a.value >= b.value;
        public static bool operator >=(TimePoint a, long b) => a.value >= b;
        public static bool operator >=(long a, TimePoint b) => a >= b.value;
        public static long operator +(TimePoint a, TimePoint b) => checked(a.value + b.value);
        public static long operator +(TimePoint a, long b) => checked(a.value + b);
        public static long operator +(long a, TimePoint b) => checked(a + b.value);
        public static long operator -(TimePoint a, TimePoint b) => checked(a.value - b.value);
        public static long operator -(TimePoint a, long b) => checked(a.value - b);
        public static long operator -(long a, TimePoint b) => checked(a - b.value);
        public static long operator *(TimePoint a, TimePoint b) => checked(a.value * b.value);
        public static long operator *(TimePoint a, long b) => checked(a.value * b);
        public static long operator *(long a, TimePoint b) => checked(a * b.value);
        public static long operator /(TimePoint a, TimePoint b) => checked(a.value / b.value);
        public static long operator /(TimePoint a, long b) => checked(a.value / b);
        public static long operator /(long a, TimePoint b) => checked(a / b.value);
        public static long operator %(TimePoint a, TimePoint b) => checked(a.value % b.value);
        public static long operator %(TimePoint a, long b) => checked(a.value % b);
        public static long operator %(long a, TimePoint b) => checked(a % b.value);
        public static bool operator ==(TimePoint a, int b) => a.value == b;
        public static bool operator ==(int a, TimePoint b) => a == b.value;
        public static bool operator !=(TimePoint a, int b) => a.value != b;
        public static bool operator !=(int a, TimePoint b) => a != b.value;
        public static bool operator <(TimePoint a, int b) => a.value < b;
        public static bool operator <(int a, TimePoint b) => a < b.value;
        public static bool operator>(TimePoint a, int b) => a.value > b;
        public static bool operator>(int a, TimePoint b) => a > b.value;
        public static bool operator <=(TimePoint a, int b) => a.value <= b;
        public static bool operator <=(int a, TimePoint b) => a <= b.value;
        public static bool operator >=(TimePoint a, int b) => a.value >= b;
        public static bool operator >=(int a, TimePoint b) => a >= b.value;
        public static long operator +(TimePoint a, int b) => checked(a.value + b);
        public static long operator +(int a, TimePoint b) => checked(a + b.value);
        public static long operator -(TimePoint a, int b) => checked(a.value - b);
        public static long operator -(int a, TimePoint b) => checked(a - b.value);
        public static long operator *(TimePoint a, int b) => checked(a.value * b);
        public static long operator *(int a, TimePoint b) => checked(a * b.value);
        public static long operator /(TimePoint a, int b) => checked(a.value / b);
        public static long operator /(int a, TimePoint b) => checked(a / b.value);
        public static long operator %(TimePoint a, int b) => checked(a.value % b);
        public static long operator %(int a, TimePoint b) => checked(a % b.value);
        public bool Equals(TimePoint b) => value == b.value;
        public override bool Equals(object? b) => b is TimePoint t && Equals(t);
        public override int GetHashCode() => value.GetHashCode();
        public int CompareTo(TimePoint b) => value.CompareTo(b.value);
        public override string ToString() => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
