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
using System.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000491: ejecuta una jugada legal usando los registros internos reutilizables.</summary>
        public void DoMove(Move m, StateInfo newSt)
        {
            RequireNewState(newSt);
            scratchDirties.dirtyThreats = default;
            scratchDirties.dirtyPawnPairs = default;
            DoMove(m, newSt, GivesCheck(m), ref scratchDirties);
        }

        private void RequireNewState(StateInfo newSt)
        {
            if (newSt == null) throw new ArgumentNullException(nameof(newSt));
            if (ReferenceEquals(newSt, CurrentState))
                throw new ArgumentException("El nuevo estado debe ser distinto del estado actual.", nameof(newSt));
        }

        /// <summary>SF19-000428: mantiene incrementalmente una jugada legal y sus registros de cambios.</summary>
        /// <remarks>givesCheck debe corresponder a GivesCheck(m). El llamador prepara los Dirties;
        /// se conservan las amenazas previas de su lista. TT/historias solo se usan para prefetch en C++.</remarks>
        public void DoMove(Move m, StateInfo newSt, bool givesCheck, ref Dirties dirties)
        {
            RequireNewState(newSt);
            Debug.Assert(m.IsOk());
            StateInfo oldSt = CurrentState;
            Key k = oldSt.key ^ Zobrist.Side;
            newSt.CopyPrefixFrom(oldSt);
            newSt.previous = oldSt;
            st = newSt;
            ++gamePly;
            ++newSt.rule50;
            ++newSt.pliesFromNull;

            ref DirtyPawnPairs dpps = ref dirties.dirtyPawnPairs;
            ref DirtyThreats dts = ref dirties.dirtyThreats;
            ref DirtyPiece dp = ref dirties.dirtyPiece;
            dpps.before[Color.WHITE] = Pieces(Color.WHITE, PieceType.PAWN);
            dpps.before[Color.BLACK] = Pieces(Color.BLACK, PieceType.PAWN);
            Color us = sideToMove;
            Color them = ~us;
            Square from = m.FromSq();
            Square to = m.ToSq();
            Piece pc = PieceOn(from);
            Piece captured = m.TypeOf() == MoveType.EN_PASSANT ? Piece.MakePiece(them, PieceType.PAWN) : PieceOn(to);
            dp.pc = pc;
            dp.from = from;
            dp.to = to;
            dp.add_sq = Square.SQ_NONE;
            Debug.Assert(Color.ColorOf(pc) == us);
            Debug.Assert(captured == Piece.NO_PIECE || Color.ColorOf(captured) == (m.TypeOf() != MoveType.CASTLING ? them : us));
            Debug.Assert(PieceType.TypeOf(captured) != PieceType.KING);

            if (m.TypeOf() == MoveType.CASTLING)
            {
                Debug.Assert(pc == Piece.MakePiece(us, PieceType.KING));
                Debug.Assert(captured == Piece.MakePiece(us, PieceType.ROOK));
                DoCastling(us, from, ref to, out Square rfrom, out Square rto, ref dts, ref dp);
                k ^= Zobrist.Psq(captured, rfrom) ^ Zobrist.Psq(captured, rto);
                newSt.nonPawnKey[us] ^= Zobrist.Psq(captured, rfrom) ^ Zobrist.Psq(captured, rto);
                captured = Piece.NO_PIECE;
            }
            else if (captured != Piece.NO_PIECE)
            {
                Square capsq = to;
                if (PieceType.TypeOf(captured) == PieceType.PAWN)
                {
                    if (m.TypeOf() == MoveType.EN_PASSANT)
                    {
                        capsq -= Direction.PawnPush(us);
                        Debug.Assert(pc == Piece.MakePiece(us, PieceType.PAWN));
                        Debug.Assert(to == newSt.epSquare);
                        Debug.Assert(Rank.RelativeRank(us, to) == Rank.RANK_6);
                        Debug.Assert(PieceOn(to) == Piece.NO_PIECE);
                        Debug.Assert(PieceOn(capsq) == Piece.MakePiece(them, PieceType.PAWN));
                        RemovePiece(capsq, ref dts);
                    }
                    newSt.pawnKey ^= Zobrist.Psq(captured, capsq);
                }
                else
                {
                    newSt.nonPawnMaterial[them] -= Values.PieceValue[captured];
                    newSt.nonPawnKey[them] ^= Zobrist.Psq(captured, capsq);
                    if (PieceType.TypeOf(captured) <= PieceType.BISHOP)
                        newSt.minorPieceKey ^= Zobrist.Psq(captured, capsq);
                }
                dp.remove_pc = captured;
                dp.remove_sq = capsq;
                k ^= Zobrist.Psq(captured, capsq);
                newSt.materialKey ^= Zobrist.Psq(captured, (Square)(8 + pieceCount[captured] - (m.TypeOf() != MoveType.EN_PASSANT ? 1 : 0)));
                newSt.rule50 = 0;
            }
            else dp.remove_sq = Square.SQ_NONE;

            k ^= Zobrist.Psq(pc, from) ^ Zobrist.Psq(pc, to);
            if (newSt.epSquare != Square.SQ_NONE)
            {
                k ^= Zobrist.EnPassant(Types.File.FileOf(newSt.epSquare));
                newSt.epSquare = Square.SQ_NONE;
            }
            k ^= Zobrist.Castling((CastlingRights)newSt.castlingRights);
            newSt.castlingRights &= ~(castlingRightsMask[from] | castlingRightsMask[to]);
            k ^= Zobrist.Castling((CastlingRights)newSt.castlingRights);

            if (PieceType.TypeOf(pc) == PieceType.PAWN)
            {
                if (((int)to ^ (int)from) == 16)
                {
                    Square epSquare = to - Direction.PawnPush(us);
                    Bitboard pawns = Attacks.AttacksBB(PieceType.PAWN, epSquare, us) & Pieces(them, PieceType.PAWN);
                    if (pawns != 0)
                    {
                        Square ksq = SquareOf(PieceType.KING, them);
                        Bitboard notBlockers = ~oldSt.blockersForKing[them];
                        bool noDiscovery = (from & notBlockers) != 0 || Types.File.FileOf(from) == Types.File.FileOf(ksq);
                        if (noDiscovery && (pawns & (notBlockers | Attacks.LineBB(epSquare, ksq))) != 0)
                        {
                            newSt.epSquare = epSquare;
                            k ^= Zobrist.EnPassant(Types.File.FileOf(epSquare));
                        }
                    }
                }
                else if (m.TypeOf() == MoveType.PROMOTION)
                {
                    PieceType pt = m.PromotionType();
                    Piece promotion = Piece.MakePiece(us, pt);
                    Debug.Assert(Rank.RelativeRank(us, to) == Rank.RANK_8);
                    Debug.Assert(pt >= PieceType.KNIGHT && pt <= PieceType.QUEEN);
                    dp.add_pc = promotion;
                    dp.add_sq = to;
                    dp.to = Square.SQ_NONE;
                    k ^= Zobrist.Psq(promotion, to);
                    newSt.materialKey ^= Zobrist.Psq(promotion, (Square)(8 + pieceCount[promotion]))
                        ^ Zobrist.Psq(pc, (Square)(8 + pieceCount[pc] - 1));
                    newSt.nonPawnKey[us] ^= Zobrist.Psq(promotion, to);
                    if (pt <= PieceType.BISHOP) newSt.minorPieceKey ^= Zobrist.Psq(promotion, to);
                    newSt.nonPawnMaterial[us] += Values.PieceValue[promotion];
                }
                newSt.pawnKey ^= Zobrist.Psq(pc, from) ^ Zobrist.Psq(pc, to);
                newSt.rule50 = 0;
            }
            else
            {
                newSt.nonPawnKey[us] ^= Zobrist.Psq(pc, from) ^ Zobrist.Psq(pc, to);
                if (PieceType.TypeOf(pc) <= PieceType.BISHOP)
                    newSt.minorPieceKey ^= Zobrist.Psq(pc, from) ^ Zobrist.Psq(pc, to);
            }
            // Las pistas opcionales de prefetch C++ no cambian el estado del motor.
            newSt.key = k;
            if (m.TypeOf() != MoveType.CASTLING)
            {
                Piece toPc = m.TypeOf() == MoveType.PROMOTION ? Piece.MakePiece(us, m.PromotionType()) : pc;
                if (captured != Piece.NO_PIECE && m.TypeOf() != MoveType.EN_PASSANT)
                {
                    RemovePiece(from, ref dts);
                    SwapPiece(to, toPc, ref dts);
                }
                else if (pc == toPc) MovePiece(from, to, ref dts);
                else
                {
                    RemovePiece(from, ref dts);
                    PutPiece(toPc, to, ref dts);
                }
            }
            newSt.capturedPiece = captured;
            newSt.checkersBB = givesCheck ? (Bitboard)(AttackersTo(SquareOf(PieceType.KING, them)) & Pieces(us)) : 0;
            sideToMove = ~sideToMove;
            SetCheckInfo();
            newSt.repetition = 0;
            int end = Math.Min(newSt.rule50, newSt.pliesFromNull);
            if (end >= 4)
            {
                StateInfo stp = oldSt.previous!;
                for (int i = 4; i <= end; i += 2)
                {
                    stp = stp.previous!.previous!;
                    if (stp.key == newSt.key)
                    {
                        newSt.repetition = stp.repetition != 0 ? -i : i;
                        break;
                    }
                }
            }
            Debug.Assert(PosIsOk());
            dpps.after[Color.WHITE] = Pieces(Color.WHITE, PieceType.PAWN);
            dpps.after[Color.BLACK] = Pieces(Color.BLACK, PieceType.PAWN);
            Debug.Assert(dp.pc != Piece.NO_PIECE);
            Debug.Assert(!((captured != Piece.NO_PIECE || m.TypeOf() == MoveType.CASTLING) ^ (dp.remove_sq != Square.SQ_NONE)));
            Debug.Assert(dp.from != Square.SQ_NONE);
            Debug.Assert(!((dp.add_sq != Square.SQ_NONE) ^ (m.TypeOf() == MoveType.PROMOTION || m.TypeOf() == MoveType.CASTLING)));
        }

        /// <summary>SF19-000429: restaura representación y enlace al estado anterior.</summary>
        public void UndoMove(Move m)
        {
            Debug.Assert(m.IsOk());
            StateInfo state = CurrentState;
            sideToMove = ~sideToMove;
            Color us = sideToMove;
            Square from = m.FromSq();
            Square to = m.ToSq();
            Piece pc = PieceOn(to);
            Debug.Assert(Empty(from) || m.TypeOf() == MoveType.CASTLING);
            Debug.Assert(PieceType.TypeOf(state.capturedPiece) != PieceType.KING);
            if (m.TypeOf() == MoveType.PROMOTION)
            {
                Debug.Assert(Rank.RelativeRank(us, to) == Rank.RANK_8);
                Debug.Assert(PieceType.TypeOf(pc) == m.PromotionType());
                Debug.Assert(PieceType.TypeOf(pc) >= PieceType.KNIGHT && PieceType.TypeOf(pc) <= PieceType.QUEEN);
                pc = Piece.MakePiece(us, PieceType.PAWN);
                SwapPiece(to, pc);
            }
            if (m.TypeOf() == MoveType.CASTLING)
                UndoCastling(us, from, to);
            else
            {
                MovePiece(to, from);
                if (state.capturedPiece != Piece.NO_PIECE)
                {
                    Square capsq = to;
                    if (m.TypeOf() == MoveType.EN_PASSANT)
                    {
                        capsq -= Direction.PawnPush(us);
                        Debug.Assert(PieceType.TypeOf(pc) == PieceType.PAWN);
                        Debug.Assert(to == state.previous!.epSquare);
                        Debug.Assert(Rank.RelativeRank(us, to) == Rank.RANK_6);
                        Debug.Assert(PieceOn(capsq) == Piece.NO_PIECE);
                        Debug.Assert(state.capturedPiece == Piece.MakePiece(~us, PieceType.PAWN));
                    }
                    PutPiece(state.capturedPiece, capsq);
                }
            }
            st = state.previous;
            --gamePly;
            Debug.Assert(PosIsOk());
        }

        /// <summary>SF19-000436: cambia el turno sin mover piezas; presupone ausencia de jaque.</summary>
        public void DoNullMove(StateInfo newSt)
        {
            RequireNewState(newSt);
            Debug.Assert(Checkers() == 0);
            StateInfo previous = CurrentState;
            newSt.CopyFrom(previous);
            newSt.previous = previous;
            st = newSt;
            if (newSt.epSquare != Square.SQ_NONE)
            {
                newSt.key ^= Zobrist.EnPassant(Types.File.FileOf(newSt.epSquare));
                newSt.epSquare = Square.SQ_NONE;
            }
            newSt.key ^= Zobrist.Side;
            newSt.pliesFromNull = 0;
            newSt.capturedPiece = Piece.NO_PIECE;
            sideToMove = ~sideToMove;
            SetCheckInfo();
            newSt.repetition = 0;
            Debug.Assert(PosIsOk());
        }

        /// <summary>SF19-000437: recupera el estado anterior a DoNullMove.</summary>
        public void UndoNullMove()
        {
            Debug.Assert(Checkers() == 0);
            st = CurrentState.previous;
            sideToMove = ~sideToMove;
        }
    }
}
