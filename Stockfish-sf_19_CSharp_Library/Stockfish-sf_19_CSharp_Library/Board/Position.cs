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
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Posición mutable con identidad, carga FEN, consultas y transiciones incrementales.</summary>
    public sealed partial class Position
    {
        private readonly Piece[] board = new Piece[64];
        private readonly Bitboard[] byTypeBB = new Bitboard[8];
        private readonly Bitboard[] byColorBB = new Bitboard[Color.Count];
        private readonly int[] pieceCount = new int[16];
        private readonly int[] castlingRightsMask = new int[64];
        private readonly Square[] castlingRookSquare = new Square[16];
        private readonly Bitboard[] castlingPath = new Bitboard[16];
        private readonly IReadOnlyList<Piece> boardView;

        // Set y las transiciones mantienen estos campos. El constructor crea almacenamiento vacío.
#pragma warning disable CS0649, CS0169, CS0414
        private StateInfo? st;
        private int gamePly;
        private Color sideToMove;
        private bool chess960;
        private Dirties scratchDirties;
#pragma warning restore CS0649, CS0169, CS0414

        /// <summary>SF19-000894: almacenamiento independiente; no representa por sí solo una posición legal.</summary>
        public Position() => boardView = Array.AsReadOnly(board);

        private StateInfo CurrentState => st ?? throw new InvalidOperationException("La posición requiere un StateInfo preparado.");
        private static void RequireColor(Color color)
        {
            if (color == Color.COLOR_NB) throw new ArgumentOutOfRangeException(nameof(color));
        }
        private static void RequirePieceType(PieceType pt)
        {
            if (pt == PieceType.PIECE_TYPE_NB) throw new ArgumentOutOfRangeException(nameof(pt));
        }
        private static void RequireCastlingSide(CastlingRights rights)
        {
            if (rights != CastlingRights.WHITE_OO && rights != CastlingRights.WHITE_OOO
                && rights != CastlingRights.BLACK_OO && rights != CastlingRights.BLACK_OOO)
                throw new InvalidOperationException("Se requiere un único derecho de enroque.");
        }
    }
}
