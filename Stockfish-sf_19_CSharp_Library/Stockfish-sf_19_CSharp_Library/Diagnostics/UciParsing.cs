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
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Reusable UCI text conversion, independent of the console and search engine.</summary>
    public static class UciParsing
    {
        /// <summary>SF19-000783: lowercase ASCII, matching tolower in the native C locale.</summary>
        /// <remarks>Other UTF-16 code units are preserved. No trimming or Unicode case folding.</remarks>
        public static string ToLower(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; ++i)
                if (chars[i] >= 'A' && chars[i] <= 'Z') chars[i] = (char)(chars[i] + ('a' - 'A'));
            return new string(chars);
        }

        /// <summary>SF19-000784: find an exact UCI match among the position's legal moves.</summary>
        /// <remarks>Position must be initialized. No match, including 0000, returns Move.None().</remarks>
        public static Move ToMove(Position position, string text)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            text = ToLower(text);
            foreach (Move move in new MoveList(position, GenType.LEGAL))
                if (text == UciFormatting.Move(move, position.IsChess960())) return move;
            return Move.None();
        }
    }
}
