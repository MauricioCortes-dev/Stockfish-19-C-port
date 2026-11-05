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

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Valor inmutable de ProbeState, con las constantes del enum original.</summary>
    /// <remarks>Porteo de SF19-000626. Default representa FAIL.</remarks>
    public readonly struct ProbeState : IEquatable<ProbeState>, IComparable<ProbeState>
    {
        public static readonly ProbeState FAIL = 0;
        public static readonly ProbeState OK = 1;
        public static readonly ProbeState CHANGE_STM = -1;
        public static readonly ProbeState ZEROING_BEST_MOVE = 2;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es -1..2.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los 4 valores nombrados del enum original.</summary>
        public ProbeState(int value)
        {
            if (value < -1 || value > 2)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre -1 y 2.");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro estado.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ProbeState left, ProbeState right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ProbeState left, ProbeState right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ProbeState left, ProbeState right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ProbeState left, ProbeState right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ProbeState left, ProbeState right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ProbeState left, ProbeState right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ProbeState left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ProbeState left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ProbeState left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ProbeState left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ProbeState left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ProbeState left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, ProbeState right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, ProbeState right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, ProbeState right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, ProbeState right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, ProbeState right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, ProbeState right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ProbeState left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ProbeState left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ProbeState left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ProbeState left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ProbeState left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ProbeState left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, ProbeState right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, ProbeState right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, ProbeState right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, ProbeState right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, ProbeState right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, ProbeState right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre ProbeState.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de -1..2.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(ProbeState left, ProbeState right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(ProbeState left, ProbeState right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(ProbeState left, ProbeState right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(ProbeState left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(ProbeState left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(ProbeState left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, ProbeState right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, ProbeState right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, ProbeState right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(ProbeState left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(ProbeState left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(ProbeState left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, ProbeState right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, ProbeState right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, ProbeState right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(ProbeState state) => ~state.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(ProbeState state, int count) => state.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(ProbeState state, int count) => state.value >> count;

        /// <summary>Conserva la validación -1..2 de la entrada.</summary>
        public static implicit operator ProbeState(byte value) => new ProbeState(value);

        /// <summary>Conserva la validación -1..2 de la entrada.</summary>
        public static implicit operator ProbeState(int value) => new ProbeState(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(ProbeState state) => unchecked((byte)state.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(ProbeState state) => state.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ProbeState other) => this == other;

        public override bool Equals(object? obj) => obj is ProbeState other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ProbeState other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(FAIL),
            1 => nameof(OK),
            -1 => nameof(CHANGE_STM),
            _ => nameof(ZEROING_BEST_MOVE)
        };
    }
}


