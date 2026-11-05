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
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Tablas de claves originales, generadas una vez y publicadas completas por el CLR.</summary>
    public static class Zobrist
    {
        private static readonly Key[,] psq = new Key[16, 64];
        private static readonly Key[] enpassant = new Key[8];
        private static readonly Key[] castling = new Key[16];
        public static readonly Key Side;
        public static readonly Key NoPawns;

        internal static readonly IReadOnlyList<Piece> Pieces = Array.AsReadOnly(new Piece[]
        {
            Piece.W_PAWN, Piece.W_KNIGHT, Piece.W_BISHOP, Piece.W_ROOK, Piece.W_QUEEN, Piece.W_KING,
            Piece.B_PAWN, Piece.B_KNIGHT, Piece.B_BISHOP, Piece.B_ROOK, Piece.B_QUEEN, Piece.B_KING
        });

        static Zobrist()
        {
            var rng = new PRNG(1070372);
            foreach (Piece pc in Pieces)
                for (Square s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                    psq[pc, s] = rng.Rand<Key>();

            // Consume primero los 768 números, incluso para las filas que después se ponen a cero.
            for (int s = 56; s < 64; ++s) psq[Piece.W_PAWN, s] = 0;
            for (int s = 0; s < 8; ++s) psq[Piece.B_PAWN, s] = 0;
            for (File f = File.FILE_A; f <= File.FILE_H; ++f) enpassant[f] = rng.Rand<Key>();
            for (int cr = 0; cr <= CastlingRights.ANY_CASTLING; ++cr) castling[cr] = rng.Rand<Key>();
            Side = rng.Rand<Key>();
            NoPawns = rng.Rand<Key>();
        }

        internal static void Init() { } // La llamada fuerza el constructor estático explícito.

        public static Key Psq(Piece pc, Square square)
        {
            if ((int)pc >= 16) throw new ArgumentOutOfRangeException(nameof(pc));
            if (!Square.IsOk(square)) throw new ArgumentOutOfRangeException(nameof(square));
            return psq[pc, square];
        }
        public static Key EnPassant(File file)
        {
            if ((int)file >= 8) throw new ArgumentOutOfRangeException(nameof(file));
            return enpassant[file];
        }
        public static Key Castling(CastlingRights rights)
        {
            if ((int)rights >= 16) throw new ArgumentOutOfRangeException(nameof(rights));
            return castling[rights];
        }
    }
}
