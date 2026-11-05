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
using static Stockfish_sf_19_CSharp_Library.Types.Value;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Normalización de puntuaciones al escribir/leer la tabla de transposiciones.</summary>
    public static class TranspositionValues
    {
        /// <summary>SF19-000521: distancia desde la raíz a distancia desde la posición actual.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Value ValueToTT(Value v, int ply) => v.IsWin() ? v + ply : v.IsLoss() ? v - ply : (int)v;

        /// <summary>SF19-000522: restaura distancia desde la raíz y limita resultados dudosos por regla 50.</summary>
        public static Value ValueFromTT(Value v, int ply, int r50c)
        {
            checked
            {
                if (!v.IsValid()) return VALUE_NONE;

                if (v.IsWin())
                {
                    if (v.IsMate() && VALUE_MATE - v > 100 - r50c)
                        return VALUE_TB_WIN_IN_MAX_PLY - 1;
                    if (VALUE_TB - v > 100 - r50c)
                        return VALUE_TB_WIN_IN_MAX_PLY - 1;
                    return v - ply;
                }

                if (v.IsLoss())
                {
                    if (v.IsMated() && VALUE_MATE + v > 100 - r50c)
                        return VALUE_TB_LOSS_IN_MAX_PLY + 1;
                    if (VALUE_TB + v > 100 - r50c)
                        return VALUE_TB_LOSS_IN_MAX_PLY + 1;
                    return v + ply;
                }

                return v;
            }
        }
    }
}
