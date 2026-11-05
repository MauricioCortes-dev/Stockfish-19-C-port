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
    /// <summary>SF19-000299..305: RVV logical registers with runtime-selected lengths.
    /// The portable implementation chooses min(AVL,VLMAX), a permitted vsetvl strategy.</summary>
    public readonly struct VariableTiling
    {
        public int VectorBits { get; }
        public int NumRegs => 1;
        public int NumPsqtRegs => 1;
        public VariableTiling(int vectorBits)
        {
            if (vectorBits < 128 || vectorBits > 65536 || (vectorBits & (vectorBits - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(vectorBits));
            VectorBits = vectorBits;
        }
        public int Length(int remaining, int elementBits, int multiplier = 1, int divisor = 1)
        {
            if (VectorBits == 0 || remaining < 0 || (elementBits != 8 && elementBits != 16 && elementBits != 32 && elementBits != 64)
                || (multiplier != 1 && multiplier != 2 && multiplier != 4 && multiplier != 8)
                || (divisor != 1 && divisor != 2 && divisor != 4 && divisor != 8)) throw new ArgumentOutOfRangeException();
            int maximum = VectorBits * multiplier / elementBits / divisor;
            if (maximum == 0) throw new ArgumentOutOfRangeException();
            return Math.Min(remaining, maximum);
        }
        public int LoadTile(int index, ReadOnlySpan<short> data, Span<short> register)
        {
            int count = Length(data.Length - index, 16, 8);
            data.Slice(index, count).CopyTo(register); return count;
        }
        public void StoreTile(int index, Span<short> destination, ReadOnlySpan<short> register)
        {
            int count = Length(destination.Length - index, 16, 8);
            register.Slice(0, count).CopyTo(destination.Slice(index, count));
        }
        public int LoadPsqt(int index, ReadOnlySpan<int> data, Span<int> register)
        {
            int count = Length(data.Length - index, 32);
            data.Slice(index, count).CopyTo(register); return count;
        }
        public void StorePsqt(int index, Span<int> destination, ReadOnlySpan<int> register)
        {
            int count = Length(destination.Length - index, 32);
            register.Slice(0, count).CopyTo(destination.Slice(index, count));
        }
        public void IncrementIndex(ref int index, int dimensions) => index += Length(dimensions - index, 16, 8);
        public void IncrementPsqtIndex(ref int index, int buckets) => index += Length(buckets - index, 32);
    }
}
