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
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Protocol parsing without IO; integers retain their original widths.</summary>
    public static class UciCommands
    {
        public static string[] Tokens(string text) => text.Split(new[] { ' ', '\t', '\r', '\n', '\v', '\f' }, StringSplitOptions.RemoveEmptyEntries);
        public static LimitsType ParseLimits(string text, TimePoint startTime)
        {
            var result = new LimitsType
            {
                StartTime = startTime
            };
            string[] tokens = Tokens(text);
            for (int i = 0; i < tokens.Length; ++i)
            {
                string token = tokens[i];
                if (token == "searchmoves")
                {
                    while (++i < tokens.Length)
                        result.SearchMoves.Add(UciParsing.ToLower(tokens[i]));
                    break;
                }

                if (token == "infinite")
                {
                    result.Infinite = 1;
                    continue;
                }

                if (token == "ponder")
                {
                    result.PonderMode = true;
                    continue;
                }

                if (token != "wtime" && token != "btime" && token != "winc" && token != "binc" && token != "movestogo" && token != "depth" && token != "nodes" && token != "movetime" && token != "mate" && token != "perft")
                    continue;
                if (++i >= tokens.Length)
                    throw new FormatException("Missing argument for '" + token + "'.");
                if (token == "nodes")
                {
                    if (!ulong.TryParse(tokens[i], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out ulong nodes))
                        throw new FormatException("Invalid argument for 'nodes'.");
                    result.Nodes = nodes;
                    continue;
                }

                if (!long.TryParse(tokens[i], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long n) || n < 0)
                    throw new FormatException("Invalid argument for '" + token + "'.");
                if (token == "wtime")
                    result.Time[0] = n;
                else if (token == "btime")
                    result.Time[1] = n;
                else if (token == "winc")
                    result.Increment[0] = n;
                else if (token == "binc")
                    result.Increment[1] = n;
                else if (token == "movetime")
                    result.MoveTime = n;
                else
                {
                    if (n > int.MaxValue)
                        throw new FormatException("Invalid argument for '" + token + "'.");
                    if (token == "depth")
                        result.Depth = (int)n;
                    else if (token == "movestogo")
                        result.MovesToGo = (int)n;
                    else if (token == "mate")
                        result.Mate = (int)n;
                    else
                        result.Perft = (int)n;
                }
            }

            return result;
        }
    }
}
