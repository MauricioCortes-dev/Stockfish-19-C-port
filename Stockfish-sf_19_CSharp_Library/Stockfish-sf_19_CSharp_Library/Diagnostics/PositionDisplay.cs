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
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Original ASCII board/FEN/key/checkers display with optional real WDL/DTZ.</summary>
    public static class PositionDisplay
    {
        public static string Format(Position pos, TbProbe? tablebases = null)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            const string pieces = " PNBRQK  pnbrqk";
            var text = new StringBuilder("\n +---+---+---+---+---+---+---+---+\n");
            for (Rank rank = Rank.RANK_8; ; --rank)
            {
                for (File file = File.FILE_A; file <= File.FILE_H; ++file)
                    text.Append(" | ").Append(pieces[pos.PieceOn(Square.MakeSquare(file, rank))]);
                text.Append(" | ").Append(1 + rank).Append("\n +---+---+---+---+---+---+---+---+\n");
                if (rank == Rank.RANK_1) break;
            }
            text.Append("   a   b   c   d   e   f   g   h\n\nFen: ").Append(pos.Fen())
                .Append("\nKey: ").Append(((ulong)pos.Key()).ToString("X16", CultureInfo.InvariantCulture)).Append("\nCheckers: ");
            for (Bitboard b = pos.Checkers(); b != 0; ) text.Append(UciFormatting.Square(Bitboards.PopLsb(ref b))).Append(' ');
            if (tablebases != null && tablebases.MaxCardinality >= pos.Count(PieceType.ALL_PIECES) && !pos.CanCastle(CastlingRights.ANY_CASTLING))
            {
                var copy = new Position();
                var error = copy.Set(pos.Fen(), pos.IsChess960(), new StateInfo());
                if (error != null) throw new InvalidOperationException(error.Message);
                var wdl = tablebases.ProbeWdl(copy, out ProbeState s1);
                int dtz = tablebases.ProbeDtz(copy, out ProbeState s2);
                text.Append("\nTablebases WDL: ").Append(((int)wdl).ToString(CultureInfo.InvariantCulture).PadLeft(4)).Append(" (").Append((int)s1).Append(')')
                    .Append("\nTablebases DTZ: ").Append(dtz.ToString(CultureInfo.InvariantCulture).PadLeft(4)).Append(" (").Append((int)s2).Append(')');
            }
            return text.ToString();
        }
    }
}
