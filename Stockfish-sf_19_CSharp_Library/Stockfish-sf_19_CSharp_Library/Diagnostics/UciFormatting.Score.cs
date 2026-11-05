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
using System.Globalization;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    public static partial class UciFormatting
    {
        /// <summary>SF19-000779: material-normalized centipawns, including C++ round semantics.</summary>
        public static int ToCp(Value v, Position pos)
        {
            WinRateParams p = WdlModel.Parameters(pos);
            return checked((int)Math.Round(checked(100 * (int)v) / p.a, MidpointRounding.AwayFromZero));
        }

        /// <summary>SF19-000778: UCI cp/mate text; tablebase scores use the original 20000 anchor.</summary>
        public static string FormatScore(Score score) => score.Visit(
            mate => "mate " + ((mate.plies > 0 ? checked(mate.plies + 1) : mate.plies) / 2).ToString(CultureInfo.InvariantCulture),
            tb => "cp " + checked((tb.win ? 20000 : -20000) - tb.plies).ToString(CultureInfo.InvariantCulture),
            units => "cp " + units.value.ToString(CultureInfo.InvariantCulture));

        /// <summary>SF19-000780: win, draw and loss probabilities in per mille, summing to 1000.</summary>
        public static string Wdl(Value v, Position pos)
        {
            int w = WdlModel.WinRate(v, pos);
            int l = WdlModel.WinRate(-v, pos);
            int d = 1000 - w - l;
            return w.ToString(CultureInfo.InvariantCulture) + " " + d.ToString(CultureInfo.InvariantCulture)
                 + " " + l.ToString(CultureInfo.InvariantCulture);
        }
    }
}
