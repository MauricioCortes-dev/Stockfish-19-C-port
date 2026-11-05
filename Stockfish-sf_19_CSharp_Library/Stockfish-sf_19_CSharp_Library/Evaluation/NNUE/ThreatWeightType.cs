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
    public readonly struct ThreatWeightType : IEquatable<ThreatWeightType>
    {
        private readonly sbyte value;
        public ThreatWeightType(sbyte value) => this.value = value;
        public static implicit operator ThreatWeightType(int value) => new ThreatWeightType(unchecked((sbyte)value));
        public static implicit operator int (ThreatWeightType value) => value.value;
        public sbyte Raw() => value;
        public bool Equals(ThreatWeightType other) => value == other.value;
        public override bool Equals(object obj) => obj is ThreatWeightType other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public static bool operator ==(ThreatWeightType a, ThreatWeightType b) => a.value == b.value;
        public static bool operator !=(ThreatWeightType a, ThreatWeightType b) => a.value != b.value;
        public static bool operator ==(ThreatWeightType a, int b) => (int)a.value == b;
        public static bool operator ==(int a, ThreatWeightType b) => a == (int)b.value;
        public static bool operator !=(ThreatWeightType a, int b) => (int)a.value != b;
        public static bool operator !=(int a, ThreatWeightType b) => a != (int)b.value;
        public static ThreatWeightType operator ++(ThreatWeightType v) => new ThreatWeightType(unchecked((sbyte)(v.value + 1)));
        public static ThreatWeightType operator --(ThreatWeightType v) => new ThreatWeightType(unchecked((sbyte)(v.value - 1)));
        public static bool operator ==(ThreatWeightType a, PSQTWeightType b) => (int)a == (int)b;
        public static bool operator ==(PSQTWeightType a, ThreatWeightType b) => (int)a == (int)b;
        public static bool operator !=(ThreatWeightType a, PSQTWeightType b) => (int)a != (int)b;
        public static bool operator !=(PSQTWeightType a, ThreatWeightType b) => (int)a != (int)b;
        public static bool operator ==(ThreatWeightType a, TransformedFeatureType b) => (int)a == (int)b;
        public static bool operator ==(TransformedFeatureType a, ThreatWeightType b) => (int)a == (int)b;
        public static bool operator !=(ThreatWeightType a, TransformedFeatureType b) => (int)a != (int)b;
        public static bool operator !=(TransformedFeatureType a, ThreatWeightType b) => (int)a != (int)b;
    }
}
