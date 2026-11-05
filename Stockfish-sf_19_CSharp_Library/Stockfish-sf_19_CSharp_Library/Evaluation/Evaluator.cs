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
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;

namespace Stockfish_sf_19_CSharp_Library.Evaluation
{
    public static class Evaluator
    {
        public static Value Evaluate(Network network, Position pos, AccumulatorStack stack, AccumulatorCaches caches, int optimism = 0)
        {
            if (pos.Checkers() != 0)
                throw new InvalidOperationException("Static evaluation requires a position not in check.");
            var(psqt, positional) = network.Evaluate(pos, stack, caches);
            int nnue = psqt + positional;
            int complexity = Math.Abs((int)(psqt - positional));
            // C++ narrows these widened results to int. Preserve GCC/MinGW modulo-32 conversion.
            optimism = unchecked((int)(optimism + (long)optimism * complexity / 476));
            nnue = unchecked((int)(nnue - (long)nnue * complexity / 18236));
            int material = checked(534 * pos.Count(PieceType.PAWN) + (int)pos.NonPawnMaterial());
            int v = unchecked((int)(nnue + ((long)nnue * material + (long)optimism * 7675) / 91000));
            v = checked(v - checked(v * pos.Rule50Count()) / 199);
            return Math.Clamp(v, (int)Value.VALUE_TB_LOSS_IN_MAX_PLY + 1, (int)Value.VALUE_TB_WIN_IN_MAX_PLY - 1);
        }

        private static string Signed(int value) => value >= 0 ? "+" + value.ToString(CultureInfo.InvariantCulture) : value.ToString(CultureInfo.InvariantCulture);
        private static string SignedPawns(Value value, Position pos)
        {
            double p = 0.01 * UciFormatting.ToCp(value, pos);
            return (p >= 0 ? "+" : "-") + Math.Abs(p).ToString("F2", CultureInfo.InvariantCulture);
        }

        public static string Trace(Position pos, Network network)
        {
            if (pos.Checkers() != 0)
                return "Final evaluation: none (in check)";
            var stack = new AccumulatorStack();
            var caches = new AccumulatorCaches(network);
            string result = "\n" + NnueDiagnostics.Trace(pos, network, caches) + "\n";
            var(psqt, positional) = network.Evaluate(pos, stack, caches);
            Value v = psqt + positional;
            result += "NNUE evaluation          " + Signed(v) + " (side to move, internal units)\n";
            v = pos.SideToMove() == Color.WHITE ? (int)v : -v;
            result += "NNUE evaluation        " + SignedPawns(v, pos) + " (white side)\n";
            v = Evaluate(network, pos, stack, caches);
            v = pos.SideToMove() == Color.WHITE ? (int)v : -v;
            return result + "Final evaluation      " + SignedPawns(v, pos) + " (white side) [with scaled NNUE, ...]\n";
        }
    }
}
