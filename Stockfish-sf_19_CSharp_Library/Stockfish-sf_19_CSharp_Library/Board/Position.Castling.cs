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
    public sealed partial class Position
    {
        // SF19-000435: dos variantes del template Do; retirar ambas piezas antes de colocarlas
        // conserva los solapamientos de Chess960 sin tratar el enroque como dos MovePiece.
        private void DoCastling(Color us, Square from, ref Square to, out Square rfrom, out Square rto,
            ref DirtyThreats dts, ref DirtyPiece dp)
        {
            bool kingSide = to > from;
            rfrom = to;
            rto = Square.RelativeSquare(us, kingSide ? Square.SQ_F1 : Square.SQ_D1);
            to = Square.RelativeSquare(us, kingSide ? Square.SQ_G1 : Square.SQ_C1);
            dp.to = to;
            dp.remove_pc = dp.add_pc = Piece.MakePiece(us, PieceType.ROOK);
            dp.remove_sq = rfrom;
            dp.add_sq = rto;
            RemovePiece(from, ref dts);
            RemovePiece(rfrom, ref dts);
            PutPiece(Piece.MakePiece(us, PieceType.KING), to, ref dts);
            PutPiece(Piece.MakePiece(us, PieceType.ROOK), rto, ref dts);
        }

        private void UndoCastling(Color us, Square from, Square to)
        {
            bool kingSide = to > from;
            Square rfrom = to;
            Square rto = Square.RelativeSquare(us, kingSide ? Square.SQ_F1 : Square.SQ_D1);
            to = Square.RelativeSquare(us, kingSide ? Square.SQ_G1 : Square.SQ_C1);
            RemovePiece(to);
            RemovePiece(rto);
            PutPiece(Piece.MakePiece(us, PieceType.KING), from);
            PutPiece(Piece.MakePiece(us, PieceType.ROOK), rfrom);
        }

        // SF19-000416: el llamador ya ha localizado rey y torre en su fila inicial.
        private void SetCastlingRight(Color c, Square rfrom)
        {
            Square kfrom = SquareOf(PieceType.KING, c);
            CastlingRights cr = c & (kfrom < rfrom ? CastlingRights.KING_SIDE : CastlingRights.QUEEN_SIDE);
            CurrentState.castlingRights |= cr;
            castlingRightsMask[kfrom] |= cr;
            castlingRightsMask[rfrom] |= cr;
            castlingRookSquare[cr] = rfrom;

            Square kto = Square.RelativeSquare(c, (cr & CastlingRights.KING_SIDE) != 0 ? Square.SQ_G1 : Square.SQ_C1);
            Square rto = Square.RelativeSquare(c, (cr & CastlingRights.KING_SIDE) != 0 ? Square.SQ_F1 : Square.SQ_D1);
            castlingPath[cr] = (Attacks.BetweenBB(rfrom, rto) | Attacks.BetweenBB(kfrom, kto)) & ~(kfrom | rfrom);
        }
    }
}
