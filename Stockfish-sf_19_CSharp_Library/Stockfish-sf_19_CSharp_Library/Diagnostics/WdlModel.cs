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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Stockfish's material-dependent statistical WDL model, in per mille units.</summary>
    public static class WdlModel
    {
        // SF19-000776: preserve the order of operations of the original fitted polynomials.
        internal static WinRateParams Parameters(Position pos)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            int material = pos.Count(PieceType.PAWN) + 3 * pos.Count(PieceType.KNIGHT)
                         + 3 * pos.Count(PieceType.BISHOP) + 5 * pos.Count(PieceType.ROOK)
                         + 9 * pos.Count(PieceType.QUEEN);
            double m = Math.Max(17, Math.Min(material, 78)) / 58.0;
            double a = (((-142.72052667 * m + 372.35176398) * m - 340.71073572) * m) + 415.23490212;
            double b = (((5.93832785 * m + 15.61267078) * m - 30.57816876) * m) + 69.63866711;
            return new WinRateParams(a, b);
        }

        /// <summary>SF19-000777: rounded probability of a win, from 0 through 1000.</summary>
        public static int WinRate(Value v, Position pos)
        {
            WinRateParams p = Parameters(pos);
            return (int)(0.5 + 1000 / (1 + Math.Exp((p.a - (double)(int)v) / p.b)));
        }
    }
}
