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

using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Registro mutable por valor de las piezas/casillas modificadas por una jugada.</summary>
    /// <remarks>
    /// SF19-000730. Se conservan los campos originales para completar el registro por etapas.
    /// Default inicializa a cero, como DirtyPiece{}; no representa por sí solo un cambio válido.
    /// Los productores garantizan pc != NO_PIECE y usan SQ_NONE para campos inactivos.
    /// </remarks>
    public struct DirtyPiece
    {
        public Piece pc;
        public Square from;
        public Square to;
        public Square remove_sq;
        public Square add_sq;
        public Piece remove_pc;
        public Piece add_pc;

        /// <summary>Equivalente C# de inicializar los siete miembros del agregado C++.</summary>
        public DirtyPiece(Piece pc, Square from, Square to, Square remove_sq, Square add_sq,
                          Piece remove_pc, Piece add_pc)
        {
            this.pc = pc;
            this.from = from;
            this.to = to;
            this.remove_sq = remove_sq;
            this.add_sq = add_sq;
            this.remove_pc = remove_pc;
            this.add_pc = add_pc;
        }
    }
}

