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

using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Types;
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Geometría interna de Syzygy; conserva los auxiliares locales de tbprobe.cpp.</summary>
    internal static class SyzygyGeometry
    {
        /// <summary>XOR local que devuelve Square (SF19-000581).</summary>
        /// <remarks>Conserva primero la conversión modular u8; Square valida después 0..64.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Square Xor(Square square, int mask) => new Square(unchecked((byte)((int)square ^ mask)));

        /// <summary>Fila menos columna respecto a A1-H8, incluido SQ_NONE -> 8 (SF19-000583).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int OffA1H8(Square square) => (int)Rank.RankOf(square) - File.FileOf(square);
    }
}
