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
    public readonly struct WeightType : IEquatable<WeightType>
    {
        private readonly short value;
        public WeightType(short value) => this.value = value;
        public static implicit operator WeightType(int value) => new WeightType(unchecked((short)value));
        public static implicit operator int (WeightType value) => value.value;
        public short Raw() => value;
        public bool Equals(WeightType other) => value == other.value;
        public override bool Equals(object obj) => obj is WeightType other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public static bool operator ==(WeightType a, WeightType b) => a.value == b.value;
        public static bool operator !=(WeightType a, WeightType b) => a.value != b.value;
        public static bool operator ==(WeightType a, int b) => (int)a.value == b;
        public static bool operator ==(int a, WeightType b) => a == (int)b.value;
        public static bool operator !=(WeightType a, int b) => (int)a.value != b;
        public static bool operator !=(int a, WeightType b) => a != (int)b.value;
        public static WeightType operator ++(WeightType v) => new WeightType(unchecked((short)(v.value + 1)));
        public static WeightType operator --(WeightType v) => new WeightType(unchecked((short)(v.value - 1)));
        public static bool operator ==(WeightType a, ThreatWeightType b) => (int)a == (int)b;
        public static bool operator ==(ThreatWeightType a, WeightType b) => (int)a == (int)b;
        public static bool operator !=(WeightType a, ThreatWeightType b) => (int)a != (int)b;
        public static bool operator !=(ThreatWeightType a, WeightType b) => (int)a != (int)b;
        public static bool operator ==(WeightType a, PSQTWeightType b) => (int)a == (int)b;
        public static bool operator ==(PSQTWeightType a, WeightType b) => (int)a == (int)b;
        public static bool operator !=(WeightType a, PSQTWeightType b) => (int)a != (int)b;
        public static bool operator !=(PSQTWeightType a, WeightType b) => (int)a != (int)b;
        public static bool operator ==(WeightType a, TransformedFeatureType b) => (int)a == (int)b;
        public static bool operator ==(TransformedFeatureType a, WeightType b) => (int)a == (int)b;
        public static bool operator !=(WeightType a, TransformedFeatureType b) => (int)a != (int)b;
        public static bool operator !=(TransformedFeatureType a, WeightType b) => (int)a != (int)b;
    }
}
