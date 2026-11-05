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
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        // SF19-000417. Como en C++, estos auxiliares son privados y los invoca la carga FEN.
        private void SetCheckInfo()
        {
            UpdateSliderBlockers(Color.WHITE);
            UpdateSliderBlockers(Color.BLACK);
            Square kingSquare = SquareOf(PieceType.KING, ~sideToMove);
            var (bishopAttacks, rookAttacks) = Attacks.BothAttacksBB(kingSquare, Pieces());
            StateInfo state = CurrentState;
            state.checkSquares[PieceType.PAWN] = Attacks.AttacksBB(PieceType.PAWN, kingSquare, ~sideToMove);
            state.checkSquares[PieceType.KNIGHT] = Attacks.AttacksBB(PieceType.KNIGHT, kingSquare);
            state.checkSquares[PieceType.BISHOP] = bishopAttacks;
            state.checkSquares[PieceType.ROOK] = rookAttacks;
            state.checkSquares[PieceType.QUEEN] = state.checkSquares[PieceType.BISHOP] | state.checkSquares[PieceType.ROOK];
            state.checkSquares[PieceType.KING] = 0;
        }

        // SF19-000419. La clave depende de cantidades, no de las casillas ocupadas.
        private Key ComputeMaterialKey()
        {
            Key key = 0;
            for (int i = 0; i < Zobrist.Pieces.Count; ++i)
            {
                Piece pc = Zobrist.Pieces[i];
                for (int count = 0; count < pieceCount[pc]; ++count)
                    key ^= Zobrist.Psq(pc, (Square)(8 + count));
            }
            return key;
        }

        // SF19-000444: verificación independiente de la clave de material almacenada.
        public bool MaterialKeyIsOk() => ComputeMaterialKey() == CurrentState.materialKey;

        /// <summary>SF19-000418: recalcula los campos derivados al preparar una posición.</summary>
        private void SetState()
        {
            StateInfo state = CurrentState;
            state.key = 0;
            state.minorPieceKey = 0;
            state.nonPawnKey[Color.WHITE] = state.nonPawnKey[Color.BLACK] = 0;
            state.pawnKey = Zobrist.NoPawns;
            state.nonPawnMaterial[Color.WHITE] = state.nonPawnMaterial[Color.BLACK] = Value.VALUE_ZERO;
            state.checkersBB = AttackersTo(SquareOf(PieceType.KING, sideToMove)) & Pieces(~sideToMove);
            SetCheckInfo();

            for (Bitboard pieces = Pieces(); pieces != 0;)
            {
                Square square = Bitboards.PopLsb(ref pieces);
                Piece pc = PieceOn(square);
                state.key ^= Zobrist.Psq(pc, square);
                if (PieceType.TypeOf(pc) == PieceType.PAWN)
                    state.pawnKey ^= Zobrist.Psq(pc, square);
                else
                {
                    state.nonPawnKey[Color.ColorOf(pc)] ^= Zobrist.Psq(pc, square);
                    if (PieceType.TypeOf(pc) != PieceType.KING)
                    {
                        state.nonPawnMaterial[Color.ColorOf(pc)] += Values.PieceValue[pc];
                        if (PieceType.TypeOf(pc) <= PieceType.BISHOP)
                            state.minorPieceKey ^= Zobrist.Psq(pc, square);
                    }
                }
            }
            if (state.epSquare != Square.SQ_NONE)
                state.key ^= Zobrist.EnPassant(File.FileOf(state.epSquare));
            if (sideToMove == Color.BLACK)
                state.key ^= Zobrist.Side;
            state.key ^= Zobrist.Castling((CastlingRights)state.castlingRights);
            state.materialKey = ComputeMaterialKey();
        }
    }
}
