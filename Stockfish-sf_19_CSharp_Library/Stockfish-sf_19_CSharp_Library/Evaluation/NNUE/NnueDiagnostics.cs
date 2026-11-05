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
using System.Text;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    public static class NnueDiagnostics
    {
        public static string FormatCpAlignedDot(Value value, Position pos)
        {
            double pawns = Math.Abs(0.01 * UciFormatting.ToCp(value, pos));
            return (value < 0 ? "-" : value > 0 ? "+" : " ") + pawns.ToString("F2", CultureInfo.InvariantCulture).PadLeft(6);
        }

        public static string Trace(Position pos, Network network, AccumulatorCaches caches)
        {
            var stack = new AccumulatorStack();
            stack.Reset();
            var trace = network.TraceEvaluate(pos, stack, caches);
            var s = new StringBuilder();
            s.Append("NNUE network contributions (Normalized, ").Append(pos.SideToMove() == Color.WHITE ? "White to move)" : "Black to move)").Append('\n');
            const string border = "+------------+------------+------------+------------+\n";
            s.Append(border).Append("|   Bucket   |  Material  | Positional |   Total    |\n|            |   (PSQT)   |  (Layers)  |            |\n").Append(border);
            for (int b = 0; b < 8; ++b)
            {
                s.Append("|  ").Append(b).Append("         |  ").Append(FormatCpAlignedDot(trace.Psqt[b], pos)).Append("   |  ").Append(FormatCpAlignedDot(trace.Positional[b], pos)).Append("   |  ").Append(FormatCpAlignedDot(trace.Psqt[b] + trace.Positional[b], pos)).Append("   |");
                if (b == trace.CorrectBucket)
                    s.Append(" <-- this bucket is used");
                s.Append('\n');
            }

            return s.Append(border).ToString();
        }
    }
}
