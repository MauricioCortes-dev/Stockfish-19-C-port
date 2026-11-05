/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS node)

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

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Categoría inmutable de nodo de búsqueda: NonPV, PV o Root.</summary>
    /// <remarks>Porteo de SF19-000532. Default representa NonPV.</remarks>
    public readonly struct NodeType : IEquatable<NodeType>, IComparable<NodeType>
    {
        public static readonly NodeType NonPV = 0;
        public static readonly NodeType PV = 1;
        public static readonly NodeType Root = 2;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es 0..2.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los tres valores nombrados del enum original.</summary>
        public NodeType(int value)
        {
            if (value < 0 || value > 2)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre NonPV (0) y Root (2).");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro nodo.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NodeType left, NodeType right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NodeType left, NodeType right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(NodeType left, NodeType right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(NodeType left, NodeType right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(NodeType left, NodeType right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(NodeType left, NodeType right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NodeType left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NodeType left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(NodeType left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(NodeType left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(NodeType left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(NodeType left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, NodeType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, NodeType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, NodeType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, NodeType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, NodeType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, NodeType right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NodeType left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NodeType left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(NodeType left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(NodeType left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(NodeType left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(NodeType left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, NodeType right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, NodeType right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, NodeType right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, NodeType right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, NodeType right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, NodeType right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre NodeType.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..2.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(NodeType left, NodeType right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(NodeType left, NodeType right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(NodeType left, NodeType right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(NodeType left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(NodeType left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(NodeType left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, NodeType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, NodeType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, NodeType right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(NodeType left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(NodeType left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(NodeType left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, NodeType right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, NodeType right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, NodeType right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(NodeType node) => ~node.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(NodeType node, int count) => node.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(NodeType node, int count) => node.value >> count;

        /// <summary>Conserva la validación 0..2 de la entrada.</summary>
        public static implicit operator NodeType(byte value) => new NodeType(value);

        /// <summary>Conserva la validación 0..2 de la entrada.</summary>
        public static implicit operator NodeType(int value) => new NodeType(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(NodeType node) => (byte)node.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(NodeType node) => node.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NodeType other) => this == other;

        public override bool Equals(object? obj) => obj is NodeType other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(NodeType other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(NonPV),
            1 => nameof(PV),
            _ => nameof(Root)
        };
    }
}
