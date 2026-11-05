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
    /// <summary>Valor inmutable de WDLScore, con las constantes del enum original.</summary>
    /// <remarks>Porteo de SF19-000625. Default representa WDLDraw.</remarks>
    public readonly struct WDLScore : IEquatable<WDLScore>, IComparable<WDLScore>
    {
        public static readonly WDLScore WDLLoss = -2;
        public static readonly WDLScore WDLBlessedLoss = -1;
        public static readonly WDLScore WDLDraw = 0;
        public static readonly WDLScore WDLCursedWin = 1;
        public static readonly WDLScore WDLWin = 2;

        private readonly int value;

        /// <summary>Representación int de 32 bits; el dominio admitido es -2..2.</summary>
        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los 5 valores nombrados del enum original.</summary>
        public WDLScore(int value)
        {
            if (value < -2 || value > 2)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre -2 y 2.");

            this.value = value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro estado.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WDLScore left, WDLScore right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WDLScore left, WDLScore right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(WDLScore left, WDLScore right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(WDLScore left, WDLScore right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(WDLScore left, WDLScore right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(WDLScore left, WDLScore right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WDLScore left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WDLScore left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(WDLScore left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(WDLScore left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(WDLScore left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(WDLScore left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, WDLScore right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, WDLScore right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, WDLScore right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, WDLScore right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, WDLScore right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, WDLScore right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WDLScore left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WDLScore left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(WDLScore left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(WDLScore left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(WDLScore left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(WDLScore left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, WDLScore right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, WDLScore right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, WDLScore right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, WDLScore right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, WDLScore right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, WDLScore right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre WDLScore.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de -2..2.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(WDLScore left, WDLScore right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(WDLScore left, WDLScore right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(WDLScore left, WDLScore right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(WDLScore left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(WDLScore left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(WDLScore left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, WDLScore right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, WDLScore right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, WDLScore right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(WDLScore left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(WDLScore left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(WDLScore left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, WDLScore right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, WDLScore right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, WDLScore right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(WDLScore state) => ~state.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(WDLScore state, int count) => state.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(WDLScore state, int count) => state.value >> count;

        /// <summary>Conserva la validación -2..2 de la entrada.</summary>
        public static implicit operator WDLScore(byte value) => new WDLScore(value);

        /// <summary>Conserva la validación -2..2 de la entrada.</summary>
        public static implicit operator WDLScore(int value) => new WDLScore(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(WDLScore state) => unchecked((byte)state.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(WDLScore state) => state.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(WDLScore other) => this == other;

        public override bool Equals(object? obj) => obj is WDLScore other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(WDLScore other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            -2 => nameof(WDLLoss),
            -1 => nameof(WDLBlessedLoss),
            0 => nameof(WDLDraw),
            1 => nameof(WDLCursedWin),
            _ => nameof(WDLWin)
        };

        /// <summary>Invierte el resultado desde el otro bando (SF19-000580).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WDLScore operator -(WDLScore score) => new WDLScore(-score.value);

        /// <summary>DTZ anterior a una jugada que reinicia el contador de 50 jugadas (SF19-000587).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DtzBeforeZeroing() => this == WDLWin ? 1
            : this == WDLCursedWin ? 101
            : this == WDLBlessedLoss ? -101
            : this == WDLLoss ? -1 : 0;
    }
}
