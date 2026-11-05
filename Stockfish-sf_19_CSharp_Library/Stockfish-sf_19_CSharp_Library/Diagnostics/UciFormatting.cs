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

using File = Stockfish_sf_19_CSharp_Library.Types.File;
using Rank = Stockfish_sf_19_CSharp_Library.Types.Rank;
using Square = Stockfish_sf_19_CSharp_Library.Types.Square;
using Move = Stockfish_sf_19_CSharp_Library.Types.Move;
using MoveType = Stockfish_sf_19_CSharp_Library.Types.MoveType;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Formato de texto UCI reutilizable por consola y otros consumidores.</summary>
    public static partial class UciFormatting
    {
        /// <summary>SF19-000781: formato original de dos caracteres; SQ_NONE conserva a9.</summary>
        public static string Square(Square square) => new string(new[]
        {
            (char)('a' + File.FileOf(square)),
            (char)('1' + Rank.RankOf(square))
        });

        /// <summary>SF19-000782: formato UCI original, incluidos centinelas y enroques Chess960.</summary>
        public static string Move(Move move, bool chess960)
        {
            if (move == Types.Move.None()) return "(none)";
            if (move == Types.Move.Null()) return "0000";
            Square from = move.FromSq();
            Square to = move.ToSq();
            if (move.TypeOf() == MoveType.CASTLING && !chess960)
                to = Types.Square.MakeSquare(to > from ? File.FILE_G : File.FILE_C, Rank.RankOf(from));
            string text = Square(from) + Square(to);
            if (move.TypeOf() == MoveType.PROMOTION) text += " pnbrqk"[move.PromotionType()];
            return text;
        }
    }
}
