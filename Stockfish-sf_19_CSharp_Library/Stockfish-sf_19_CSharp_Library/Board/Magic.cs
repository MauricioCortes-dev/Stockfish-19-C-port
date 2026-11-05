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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Immutable view of one original magic and its private shared attack table.</summary>
    public readonly struct Magic
    {
        private readonly ulong[] attacks;
        public Bitboard Mask { get; }
        public Bitboard Multiplier { get; }
        public int Shift { get; }
        public int Offset { get; }
        public bool Is64Bit { get; }

        internal Magic(ulong[] table, ulong mask, ulong multiplier, int shift, int offset, bool is64Bit)
        {
            attacks = table;
            Mask = mask;
            Multiplier = multiplier;
            Shift = shift;
            Offset = offset;
            Is64Bit = is64Bit;
        }

        public uint Index(Bitboard occupied)
        {
            if (attacks == null)
                throw new InvalidOperationException("A default Magic has no table.");
            unchecked
            {
                ulong mask = Mask, magic = Multiplier, o = occupied;
                return Is64Bit ? (uint)(((o & mask) * magic) >> Shift) : (((uint)o & (uint)mask) * (uint)magic ^ ((uint)(o >> 32) & (uint)(mask >> 32)) * (uint)(magic >> 32)) >> Shift;
            }
        }

        public Bitboard AttacksBB(Bitboard occupied) => attacks[Offset + (int)Index(occupied)];
    }
}
