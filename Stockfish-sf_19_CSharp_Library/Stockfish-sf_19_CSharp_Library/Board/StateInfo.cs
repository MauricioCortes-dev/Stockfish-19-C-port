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
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>SF19-000447: estado mutable con identidad estable en la cadena de posiciones.</summary>
    /// <remarks>Los arrays tienen tamaño fijo y pertenecen a este estado. Asignar la clase comparte identidad;
    /// CopyFrom copia campos, conservando únicamente la identidad del enlace previous.</remarks>
    public sealed class StateInfo
    {
        // Prefijo copiado por Position::do_move, hasta offsetof(StateInfo, key).
        public Key materialKey;
        public Key pawnKey;
        public Key minorPieceKey;
        public readonly Key[] nonPawnKey = new Key[Color.Count];
        public readonly Value[] nonPawnMaterial = new Value[Color.Count];
        public int castlingRights;
        public int rule50;
        public int pliesFromNull;
        public Square epSquare;

        // Campos que una jugada ordinaria vuelve a calcular.
        public Key key;
        public Bitboard checkersBB;
        public StateInfo? previous;
        public readonly Bitboard[] blockersForKing = new Bitboard[Color.Count];
        public readonly Bitboard[] pinners = new Bitboard[Color.Count];
        public readonly Bitboard[] checkSquares = new Bitboard[8];
        public Piece capturedPiece;
        public int repetition;

        /// <summary>Adaptación de la copia del prefijo; no enlaza previous ni recalcula campos de Position.</summary>
        public void CopyPrefixFrom(StateInfo source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            materialKey = source.materialKey;
            pawnKey = source.pawnKey;
            minorPieceKey = source.minorPieceKey;
            Array.Copy(source.nonPawnKey, nonPawnKey, nonPawnKey.Length);
            Array.Copy(source.nonPawnMaterial, nonPawnMaterial, nonPawnMaterial.Length);
            castlingRights = source.castlingRights;
            rule50 = source.rule50;
            pliesFromNull = source.pliesFromNull;
            epSquare = source.epSquare;
        }

        /// <summary>Adaptación de memcpy(sizeof(StateInfo)): copia todos los valores y el puntero lógico previous.</summary>
        public void CopyFrom(StateInfo source)
        {
            CopyPrefixFrom(source);
            key = source.key;
            checkersBB = source.checkersBB;
            previous = source.previous;
            Array.Copy(source.blockersForKing, blockersForKing, blockersForKing.Length);
            Array.Copy(source.pinners, pinners, pinners.Length);
            Array.Copy(source.checkSquares, checkSquares, checkSquares.Length);
            capturedPiece = source.capturedPiece;
            repetition = source.repetition;
        }
    }
}
