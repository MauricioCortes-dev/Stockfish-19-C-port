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
    /// <summary>Fila inmutable del tablero, incluido el centinela RANK_NB.</summary>
    /// <remarks>Porteo de SF19-000729. Default representa RANK_1.</remarks>
    public readonly struct Rank : IEquatable<Rank>, IComparable<Rank>
    {
        public static readonly Rank RANK_1 = 0;
        public static readonly Rank RANK_2 = 1;
        public static readonly Rank RANK_3 = 2;
        public static readonly Rank RANK_4 = 3;
        public static readonly Rank RANK_5 = 4;
        public static readonly Rank RANK_6 = 5;
        public static readonly Rank RANK_7 = 6;
        public static readonly Rank RANK_8 = 7;
        public static readonly Rank RANK_NB = 8;

        private readonly byte value;

        /// <summary>Representación original: índices 0..7 y centinela 8.</summary>
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => value;
        }

        /// <summary>Admite los ocho índices y el centinela de conteo (8).</summary>
        public Rank(int value)
        {
            if (value < 0 || value > 8)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "El valor debe estar entre RANK_1 (0) y RANK_NB (8).");

            this.value = (byte)value;
        }

        // Comparar con int/byte conserva la promoción numérica sin construir otro índice.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, Rank right) => left.value == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, Rank right) => left.value != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Rank left, Rank right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Rank left, Rank right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Rank left, Rank right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Rank left, Rank right) => left.value >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, int right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, int right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Rank left, int right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Rank left, int right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Rank left, int right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Rank left, int right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int left, Rank right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int left, Rank right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(int left, Rank right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(int left, Rank right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(int left, Rank right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(int left, Rank right) => left >= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rank left, byte right) => left.value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rank left, byte right) => left.value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Rank left, byte right) => left.value < right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Rank left, byte right) => left.value > right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Rank left, byte right) => left.value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Rank left, byte right) => left.value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte left, Rank right) => left == right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte left, Rank right) => left != right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(byte left, Rank right) => left < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(byte left, Rank right) => left > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(byte left, Rank right) => left <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(byte left, Rank right) => left >= right.value;

        // Las enumeraciones C++ se promocionan a int, incluso al operar entre Rank.
        // Las sobrecargas mixtas evitan convertir operandos externos fuera de 0..8.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Rank left, Rank right) => left.value & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Rank left, Rank right) => left.value | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Rank left, Rank right) => left.value ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Rank left, int right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Rank left, int right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Rank left, int right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(int left, Rank right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(int left, Rank right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(int left, Rank right) => left ^ right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(Rank left, byte right) => left.value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(Rank left, byte right) => left.value | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(Rank left, byte right) => left.value ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator &(byte left, Rank right) => left & right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(byte left, Rank right) => left | right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ^(byte left, Rank right) => left ^ right.value;

        /// <summary>Complemento numérico de int; no trunca el resultado al ancho de almacenamiento.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator ~(Rank rank) => ~rank.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Rank rank, int count) => rank.value << count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Rank rank, int count) => rank.value >> count;

        // C# usa estos operadores tanto para prefijo como para postfijo.
        // El constructor rechaza atravesar los límites del dominio admitido.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator ++(Rank index) => new Rank(index.value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank operator --(Rank index) => new Rank(index.value - 1);

        /// <summary>Orientación original r ^ (c * 7); valida únicamente el resultado.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank RelativeRank(Color color, Rank rank) => rank ^ (color * 7);

        /// <summary>SF19-000753: extrae la fila; SQ_NONE produce RANK_NB.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank RankOf(Square square) => square >> 3;

        /// <summary>SF19-000756: orientación a partir de la fila de una casilla.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank RelativeRank(Color color, Square square) => RelativeRank(color, RankOf(square));

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator Rank(byte value) => new Rank(value);

        /// <summary>Conserva la validación 0..8 de la entrada.</summary>
        public static implicit operator Rank(int value) => new Rank(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(Rank rank) => rank.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Rank rank) => rank.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rank other) => this == other;

        public override bool Equals(object? obj) => obj is Rank other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Rank other) => value.CompareTo(other.value);

        public override string ToString() => value switch
        {
            0 => nameof(RANK_1),
            1 => nameof(RANK_2),
            2 => nameof(RANK_3),
            3 => nameof(RANK_4),
            4 => nameof(RANK_5),
            5 => nameof(RANK_6),
            6 => nameof(RANK_7),
            7 => nameof(RANK_8),
            _ => nameof(RANK_NB)
        };
    }
}
