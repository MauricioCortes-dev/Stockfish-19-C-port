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
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        // SF19-000430: ref conserva la escritura a través del puntero original.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddDirtyThreat(ref DirtyThreats dts, bool putPiece, Piece pc,
            Piece threatened, Square square, Square threatenedSquare) =>
            dts.list.PushBack(new DirtyThreat(pc, threatened, square, threatenedSquare, putPiece));

        // SF19-000432: una dama solo puede ser amenazada por otra dama deslizante.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CanSliderThreat(Piece pc, Piece slider) =>
            PieceType.TypeOf(pc) != PieceType.QUEEN || PieceType.TypeOf(slider) == PieceType.QUEEN;

        /// <summary>SF19-000433/895: rutas escalar y por bloques; computeRay sustituye al parámetro de plantilla.</summary>
        private void UpdatePieceThreats(Piece pc, bool putPiece, Square square, ref DirtyThreats dts,
            Bitboard noRaysContaining, bool computeRay = true) =>
            UpdatePieceThreatsCore(pc, putPiece, square, ref dts, noRaysContaining, computeRay, MoveProcessing.UseBlocks);

        private void UpdatePieceThreatsScalar(Piece pc, bool putPiece, Square square, ref DirtyThreats dts,
            Bitboard noRaysContaining, bool computeRay) =>
            UpdatePieceThreatsCore(pc, putPiece, square, ref dts, noRaysContaining, computeRay, false);

        private void UpdatePieceThreatsCore(Piece pc, bool putPiece, Square square, ref DirtyThreats dts,
            Bitboard noRaysContaining, bool computeRay, bool blocks)
        {
            Bitboard occupied = Pieces();
            var (bishopAttacks, rookAttacks) = Attacks.BothAttacksBB(square, occupied);
            Bitboard sliderAttacks = bishopAttacks | rookAttacks;
            Bitboard occupiedNoKing = occupied ^ Pieces(PieceType.KING);
            PieceType pt = PieceType.TypeOf(pc);
            Bitboard sliders = (Pieces(PieceType.BISHOP, PieceType.QUEEN) & bishopAttacks)
                             | (Pieces(PieceType.ROOK, PieceType.QUEEN) & rookAttacks);

            // El parámetro ref explícito evita capturar/copiar el agregado mutable.
            void ProcessSliders(bool addDirectAttacks, ref DirtyThreats changes)
            {
                Bitboard remaining = sliders;
                while (remaining)
                {
                    Square sliderSquare = Bitboards.PopLsb(ref remaining);
                    Piece slider = PieceOn(sliderSquare);
                    Bitboard ray = Attacks.RayPassBB(sliderSquare, square);
                    Bitboard discovered = ray & sliderAttacks & occupiedNoKing;
                    Debug.Assert(!Bitboards.MoreThanOne(discovered));
                    if (discovered != 0 && (ray & noRaysContaining) != noRaysContaining)
                    {
                        Square threatenedSquare = Bitboards.Lsb(discovered);
                        Piece threatenedPiece = PieceOn(threatenedSquare);
                        if (CanSliderThreat(threatenedPiece, slider))
                            AddDirtyThreat(ref changes, !putPiece, slider, threatenedPiece, sliderSquare, threatenedSquare);
                    }
                    if (addDirectAttacks && CanSliderThreat(pc, slider))
                        AddDirtyThreat(ref changes, putPiece, slider, pc, sliderSquare, square);
                }
            }

            // Los reyes no emiten amenazas directas, pero pueden abrir/cerrar rayos.
            if (pt == PieceType.KING)
            {
                if (computeRay) ProcessSliders(false, ref dts);
                return;
            }
            Bitboard threatTargets = pt == PieceType.PAWN ? Pieces(PieceType.KNIGHT, PieceType.ROOK)
                : pt == PieceType.BISHOP || pt == PieceType.ROOK ? Pieces(PieceType.PAWN, PieceType.KNIGHT, PieceType.BISHOP, PieceType.ROOK)
                : occupiedNoKing;
            Bitboard threatened = (pt == PieceType.BISHOP ? bishopAttacks
                : pt == PieceType.ROOK ? rookAttacks
                : pt == PieceType.QUEEN ? sliderAttacks
                : pt == PieceType.PAWN ? Attacks.AttacksBB(pt, square, Color.ColorOf(pc))
                : Attacks.AttacksBB(pt, square)) & threatTargets;
            Bitboard incomingThreats = Attacks.AttacksBB(PieceType.KNIGHT, square) & Pieces(PieceType.KNIGHT);
            if (pt == PieceType.KNIGHT || pt == PieceType.ROOK)
                incomingThreats |= (Attacks.AttacksBB(PieceType.PAWN, square, Color.WHITE) & Pieces(Color.BLACK, PieceType.PAWN))
                                | (Attacks.AttacksBB(PieceType.PAWN, square, Color.BLACK) & Pieces(Color.WHITE, PieceType.PAWN));

            if (blocks)
            {
                WriteMultipleDirties(threatened, new DirtyThreat(pc, Piece.NO_PIECE, square, Square.SQ_A1, putPiece),
                    DirtyThreat.ThreatenedSqOffset, DirtyThreat.ThreatenedPcOffset, ref dts);
                Bitboard directSliders = pt == PieceType.QUEEN ? (Bitboard)(sliders & Pieces(PieceType.QUEEN)) : sliders;
                WriteMultipleDirties(directSliders | incomingThreats,
                    new DirtyThreat(Piece.NO_PIECE, pc, Square.SQ_A1, square, putPiece),
                    DirtyThreat.PcSqOffset, DirtyThreat.PcOffset, ref dts);
                if (computeRay) ProcessSliders(false, ref dts);
                return;
            }
            while (threatened)
            {
                Square threatenedSquare = Bitboards.PopLsb(ref threatened);
                Piece threatenedPiece = PieceOn(threatenedSquare);
                Debug.Assert(threatenedSquare != square);
                Debug.Assert(threatenedPiece != Piece.NO_PIECE);
                AddDirtyThreat(ref dts, putPiece, pc, threatenedPiece, square, threatenedSquare);
            }
            if (computeRay) ProcessSliders(true, ref dts);
            else incomingThreats |= pt == PieceType.QUEEN ? (Bitboard)(sliders & Pieces(PieceType.QUEEN)) : sliders;

            while (incomingThreats)
            {
                Square sourceSquare = Bitboards.PopLsb(ref incomingThreats);
                Piece sourcePiece = PieceOn(sourceSquare);
                Debug.Assert(sourceSquare != square);
                Debug.Assert(sourcePiece != Piece.NO_PIECE);
                AddDirtyThreat(ref dts, putPiece, sourcePiece, pc, sourceSquare, square);
            }
        }
        /// <summary>SF19-000431: logical compress, board lookup, widen, shift and ternary OR.</summary>
        private void WriteMultipleDirties(Bitboard mask, DirtyThreat template, int squareShift, int pieceShift, ref DirtyThreats output)
        {
            int count = Bitboards.Popcount(mask);
            if (count > 16) throw new ArgumentException("A threat block contains at most sixteen incoming lanes.");
            Span<ushort> squares = stackalloc ushort[16];
            BlockLanes.CompressSquares(mask, squares);
            int first = output.list.MakeSpace(count);
            for (int i = 0; i < count; ++i)
            {
                Square sq = (Square)(int)squares[i];
                uint raw = template.Raw() | ((uint)squares[i] << squareShift) | ((uint)(int)PieceOn(sq) << pieceShift);
                output.list[first + i] = new DirtyThreat(raw);
            }
        }
    }
}
