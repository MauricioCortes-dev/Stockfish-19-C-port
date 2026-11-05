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
        public Color SideToMove() => sideToMove;
        public int GamePly() => gamePly;
        public bool IsChess960() => chess960;
        public StateInfo? State() => st;
        public Square EpSquare() => CurrentState.epSquare;
        public int Rule50Count() => CurrentState.rule50;
        public Piece CapturedPiece() => CurrentState.capturedPiece;
        public Value NonPawnMaterial(Color color)
        {
            RequireColor(color);
            return CurrentState.nonPawnMaterial[color];
        }
        public Value NonPawnMaterial() => NonPawnMaterial(Color.WHITE) + NonPawnMaterial(Color.BLACK);
        public bool CanCastle(CastlingRights rights) => (CurrentState.castlingRights & rights) != 0;
        public bool CastlingImpeded(CastlingRights rights)
        {
            RequireCastlingSide(rights);
            return (Pieces() & castlingPath[rights]) != 0;
        }
        public Square CastlingRookSquare(CastlingRights rights)
        {
            RequireCastlingSide(rights);
            return castlingRookSquare[rights];
        }

        public Bitboard Checkers() => CurrentState.checkersBB;
        public Bitboard BlockersForKing(Color color)
        {
            RequireColor(color);
            return CurrentState.blockersForKing[color];
        }
        public Bitboard Pinners(Color color)
        {
            RequireColor(color);
            return CurrentState.pinners[color];
        }
        public Bitboard CheckSquares(PieceType pt)
        {
            RequirePieceType(pt);
            return CurrentState.checkSquares[pt];
        }

        public Key Key() => AdjustKey50(CurrentState.key);
        /// <summary>Original approximate next-position key used for prefetch, not an exact KeyAfter API.</summary>
        public Key PrefetchKey(Move move)
        {
            Square from = move.FromSq(), to = move.ToSq();
            Piece pc = PieceOn(from), captured = PieceOn(to);
            Key k = CurrentState.key ^ Zobrist.Side;
            k ^= Zobrist.Psq(captured, to) ^ Zobrist.Psq(pc, to) ^ Zobrist.Psq(pc, from);
            if (captured != Piece.NO_PIECE || PieceType.TypeOf(pc) == PieceType.PAWN) return k;
            return AdjustKey50(k, true);
        }

        public Key PawnKey() => CurrentState.pawnKey;
        public Key MaterialKey() => CurrentState.materialKey;
        public Key MinorPieceKey() => CurrentState.minorPieceKey;
        public Key NonPawnKey(Color color)
        {
            RequireColor(color);
            return CurrentState.nonPawnKey[color];
        }
        private Key AdjustKey50(Key key, bool afterMove = false)
        {
            int threshold = 14 - (afterMove ? 1 : 0);
            int rule50 = CurrentState.rule50;
            if (rule50 < threshold) return key;
            return key ^ Types.Key.MakeKey((ulong)((rule50 - threshold) / 8));
        }
    }
}
