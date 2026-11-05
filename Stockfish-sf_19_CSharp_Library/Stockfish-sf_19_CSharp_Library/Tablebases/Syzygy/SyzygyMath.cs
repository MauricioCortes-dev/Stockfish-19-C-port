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

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Auxiliares numéricos internos de Syzygy.</summary>
    internal static class SyzygyMath
    {
        // SF19-000588: las únicas instanciaciones originales son int y WDLScore.
        // Las sobrecargas conservan SignOf(value) sin introducir generic math en C# 8.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SignOf(int value) => (0 < value ? 1 : 0) - (value < 0 ? 1 : 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SignOf(WDLScore value) => (0 < value ? 1 : 0) - (value < 0 ? 1 : 0);
    }
}
