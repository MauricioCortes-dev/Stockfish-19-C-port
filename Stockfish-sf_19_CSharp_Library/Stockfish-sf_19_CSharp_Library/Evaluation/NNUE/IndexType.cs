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

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Exact-width NNUE domain alias; primitive promotion and explicit value copies.</summary>
    public readonly struct IndexType : IEquatable<IndexType>
    {
        private readonly uint value;
        public IndexType(uint value) => this.value = value;
        public static implicit operator IndexType(uint value) => new IndexType(unchecked((uint)value));
        public static implicit operator uint (IndexType value) => value.value;
        public uint Raw() => value;
        public bool Equals(IndexType other) => value == other.value;
        public override bool Equals(object obj) => obj is IndexType other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public static bool operator ==(IndexType a, IndexType b) => a.value == b.value;
        public static bool operator !=(IndexType a, IndexType b) => a.value != b.value;
        public static implicit operator IndexType(int value) => new IndexType(unchecked((uint)value));
        public static uint operator +(IndexType a, IndexType b) => unchecked(a.value + b.value);
        public static uint operator -(IndexType a, IndexType b) => unchecked(a.value - b.value);
        public static uint operator *(IndexType a, IndexType b) => unchecked(a.value * b.value);
        public static uint operator /(IndexType a, IndexType b) => unchecked(a.value / b.value);
        public static uint operator %(IndexType a, IndexType b) => unchecked(a.value % b.value);
        public static uint operator &(IndexType a, IndexType b) => unchecked(a.value & b.value);
        public static uint operator |(IndexType a, IndexType b) => unchecked(a.value | b.value);
        public static uint operator ^(IndexType a, IndexType b) => unchecked(a.value ^ b.value);
        public static bool operator <(IndexType a, IndexType b) => a.value < b.value;
        public static bool operator <=(IndexType a, IndexType b) => a.value <= b.value;
        public static bool operator>(IndexType a, IndexType b) => a.value > b.value;
        public static bool operator >=(IndexType a, IndexType b) => a.value >= b.value;
        public static uint operator +(IndexType a, int b) => unchecked(a.value + unchecked((uint)b));
        public static uint operator -(IndexType a, int b) => unchecked(a.value - unchecked((uint)b));
        public static uint operator *(IndexType a, int b) => unchecked(a.value * unchecked((uint)b));
        public static uint operator /(IndexType a, int b) => unchecked(a.value / unchecked((uint)b));
        public static uint operator %(IndexType a, int b) => unchecked(a.value % unchecked((uint)b));
        public static uint operator &(IndexType a, int b) => unchecked(a.value & unchecked((uint)b));
        public static uint operator |(IndexType a, int b) => unchecked(a.value | unchecked((uint)b));
        public static uint operator ^(IndexType a, int b) => unchecked(a.value ^ unchecked((uint)b));
        public static bool operator <(IndexType a, int b) => a.value < unchecked((uint)b);
        public static bool operator <=(IndexType a, int b) => a.value <= unchecked((uint)b);
        public static bool operator>(IndexType a, int b) => a.value > unchecked((uint)b);
        public static bool operator >=(IndexType a, int b) => a.value >= unchecked((uint)b);
        public static bool operator ==(IndexType a, int b) => a.value == unchecked((uint)b);
        public static bool operator !=(IndexType a, int b) => a.value != unchecked((uint)b);
        public static uint operator +(int a, IndexType b) => unchecked(unchecked((uint)a) + b.value);
        public static uint operator -(int a, IndexType b) => unchecked(unchecked((uint)a) - b.value);
        public static uint operator *(int a, IndexType b) => unchecked(unchecked((uint)a) * b.value);
        public static uint operator /(int a, IndexType b) => unchecked(unchecked((uint)a) / b.value);
        public static uint operator %(int a, IndexType b) => unchecked(unchecked((uint)a) % b.value);
        public static uint operator &(int a, IndexType b) => unchecked(unchecked((uint)a) & b.value);
        public static uint operator |(int a, IndexType b) => unchecked(unchecked((uint)a) | b.value);
        public static uint operator ^(int a, IndexType b) => unchecked(unchecked((uint)a) ^ b.value);
        public static bool operator <(int a, IndexType b) => unchecked((uint)a) < b.value;
        public static bool operator <=(int a, IndexType b) => unchecked((uint)a) <= b.value;
        public static bool operator>(int a, IndexType b) => unchecked((uint)a) > b.value;
        public static bool operator >=(int a, IndexType b) => unchecked((uint)a) >= b.value;
        public static bool operator ==(int a, IndexType b) => unchecked((uint)a) == b.value;
        public static bool operator !=(int a, IndexType b) => unchecked((uint)a) != b.value;
        public static uint operator +(IndexType a, uint b) => unchecked(a.value + unchecked((uint)b));
        public static uint operator -(IndexType a, uint b) => unchecked(a.value - unchecked((uint)b));
        public static uint operator *(IndexType a, uint b) => unchecked(a.value * unchecked((uint)b));
        public static uint operator /(IndexType a, uint b) => unchecked(a.value / unchecked((uint)b));
        public static uint operator %(IndexType a, uint b) => unchecked(a.value % unchecked((uint)b));
        public static uint operator &(IndexType a, uint b) => unchecked(a.value & unchecked((uint)b));
        public static uint operator |(IndexType a, uint b) => unchecked(a.value | unchecked((uint)b));
        public static uint operator ^(IndexType a, uint b) => unchecked(a.value ^ unchecked((uint)b));
        public static bool operator <(IndexType a, uint b) => a.value < unchecked((uint)b);
        public static bool operator <=(IndexType a, uint b) => a.value <= unchecked((uint)b);
        public static bool operator>(IndexType a, uint b) => a.value > unchecked((uint)b);
        public static bool operator >=(IndexType a, uint b) => a.value >= unchecked((uint)b);
        public static bool operator ==(IndexType a, uint b) => a.value == unchecked((uint)b);
        public static bool operator !=(IndexType a, uint b) => a.value != unchecked((uint)b);
        public static uint operator +(uint a, IndexType b) => unchecked(unchecked((uint)a) + b.value);
        public static uint operator -(uint a, IndexType b) => unchecked(unchecked((uint)a) - b.value);
        public static uint operator *(uint a, IndexType b) => unchecked(unchecked((uint)a) * b.value);
        public static uint operator /(uint a, IndexType b) => unchecked(unchecked((uint)a) / b.value);
        public static uint operator %(uint a, IndexType b) => unchecked(unchecked((uint)a) % b.value);
        public static uint operator &(uint a, IndexType b) => unchecked(unchecked((uint)a) & b.value);
        public static uint operator |(uint a, IndexType b) => unchecked(unchecked((uint)a) | b.value);
        public static uint operator ^(uint a, IndexType b) => unchecked(unchecked((uint)a) ^ b.value);
        public static bool operator <(uint a, IndexType b) => unchecked((uint)a) < b.value;
        public static bool operator <=(uint a, IndexType b) => unchecked((uint)a) <= b.value;
        public static bool operator>(uint a, IndexType b) => unchecked((uint)a) > b.value;
        public static bool operator >=(uint a, IndexType b) => unchecked((uint)a) >= b.value;
        public static bool operator ==(uint a, IndexType b) => unchecked((uint)a) == b.value;
        public static bool operator !=(uint a, IndexType b) => unchecked((uint)a) != b.value;
        public static uint operator <<(IndexType v, int shift) => v.value << shift;
        public static uint operator >>(IndexType v, int shift) => v.value >> shift;
        public static uint operator ~(IndexType v) => ~v.value;
        public static uint operator -(IndexType v) => unchecked(0U - v.value);
        public static IndexType operator ++(IndexType v) => new IndexType(unchecked(v.value + 1));
        public static IndexType operator --(IndexType v) => new IndexType(unchecked(v.value - 1));
    }
}
