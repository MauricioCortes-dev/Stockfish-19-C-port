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
using System.Globalization;
using System.Text;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        private const string PieceToChar = " PNBRQK  pnbrqk";

        // Equivale a memset para los valores, preservando las identidades managed y sus vistas.
        private void ResetForFen(StateInfo si)
        {
            Array.Clear(board, 0, board.Length);
            Array.Clear(byTypeBB, 0, byTypeBB.Length);
            Array.Clear(byColorBB, 0, byColorBB.Length);
            Array.Clear(pieceCount, 0, pieceCount.Length);
            Array.Clear(castlingRightsMask, 0, castlingRightsMask.Length);
            Array.Clear(castlingRookSquare, 0, castlingRookSquare.Length);
            Array.Clear(castlingPath, 0, castlingPath.Length);
            si.CopyFrom(new StateInfo());
            st = si;
            gamePly = 0;
            sideToMove = default;
            chess960 = false;
            scratchDirties = default;
        }

        /// <summary>SF19-000415: carga FEN; null significa éxito. Un error puede dejar una carga parcial.</summary>
        public PositionSetError? Set(string fenStr, bool isChess960, StateInfo si)
        {
            if (fenStr == null) throw new ArgumentNullException(nameof(fenStr));
            if (si == null) throw new ArgumentNullException(nameof(si));
            var ss = new FenReader(fenStr);
            ResetForFen(si);

            int numPieces = 0;
            int file = File.FILE_A;
            int rank = Rank.RANK_8;
            char token;
            for (;;)
            {
                if (!ss.Read(out token))
                    return new PositionSetError("Invalid FEN. Unexpected end of stream.");
                if (FenReader.IsSpace(token)) break;
                if (token >= '0' && token <= '9')
                {
                    int diff = token - '0';
                    if (diff < 1 || diff > 8)
                        return new PositionSetError("Invalid FEN. Invalid number of squares to skip.");
                    file += diff;
                    if (file > File.FILE_NB)
                        return new PositionSetError("Invalid FEN. Invalid file reached.");
                }
                else if (token == '/')
                {
                    if (file != File.FILE_NB)
                        return new PositionSetError("Invalid FEN. Trying to end rank when not at the end of it.");
                    --rank;
                    file = File.FILE_A;
                    if (rank < Rank.RANK_1)
                        return new PositionSetError("Invalid FEN. Invalid rank reached.");
                }
                else
                {
                    if (file >= File.FILE_NB)
                        return new PositionSetError("Invalid FEN. Invalid file reached.");
                    int idx = PieceToChar.IndexOf(token);
                    if (idx < 0)
                        return new PositionSetError("Invalid FEN. Invalid piece: " + token);
                    if (++numPieces > 32)
                        return new PositionSetError("Invalid FEN. More than 32 pieces on the board.");
                    Square sq = Square.MakeSquare((File)file, (Rank)rank);
                    PutPiece((Piece)idx, sq);
                    ++file;
                }
            }

            if (rank != Rank.RANK_1 || file != File.FILE_NB)
                return new PositionSetError("Invalid FEN. Board state encoding ended but cursor not at end.");
            if ((Pieces(PieceType.PAWN) & (Bitboards.Rank1BB | Bitboards.Rank8BB)) != 0)
                return new PositionSetError("Unsupported position. Pawns on the first or eighth rank.");
            if (Count(PieceType.KING, Color.WHITE) != 1 || Count(PieceType.KING, Color.BLACK) != 1)
                return new PositionSetError("Unsupported position. Incorrect number of kings.");

            foreach (Color c in new[] { Color.WHITE, Color.BLACK })
            {
                if (Count(PieceType.PAWN, c) > 8)
                    return new PositionSetError("Unsupported position. " + (c == Color.WHITE ? "WHITE" : "BLACK") + " has more than 8 pawns.");
                int additional = Math.Max(Count(PieceType.KNIGHT, c) - 2, 0) + Math.Max(Count(PieceType.BISHOP, c) - 2, 0)
                               + Math.Max(Count(PieceType.ROOK, c) - 2, 0) + Math.Max(Count(PieceType.QUEEN, c) - 1, 0);
                if (additional > 8 - Count(PieceType.PAWN, c))
                    return new PositionSetError("Unsupported position. Too many pieces for " + (c == Color.WHITE ? "WHITE." : "BLACK."));
            }

            if (!ss.Read(out token))
                return new PositionSetError("Invalid FEN. Unexpected end of stream.");
            if (token != 'w' && token != 'b')
                return new PositionSetError("Invalid FEN. Invalid side to move: " + token);
            sideToMove = token == 'w' ? Color.WHITE : Color.BLACK;
            if (!ss.Read(out token) || !FenReader.IsSpace(token) || ss.Eof)
                return new PositionSetError("Invalid FEN. Expected whitespace after side to move.");

            int numCastlingRights = 0;
            for (;;)
            {
                if (!ss.Read(out token)) break;
                if (FenReader.IsSpace(token)) break;
                if (numCastlingRights == 0 && token == '-')
                {
                    ss.SkipWhite();
                    break;
                }
                if (++numCastlingRights > 4)
                    return new PositionSetError("Invalid FEN. Maximum of 4 castling rights can be specified.");

                Square rsq = Square.SQ_NONE, ksq = Square.SQ_NONE;
                Color c = token >= 'a' && token <= 'z' ? Color.BLACK : Color.WHITE;
                Piece rook = Piece.MakePiece(c, PieceType.ROOK);
                Piece king = Piece.MakePiece(c, PieceType.KING);
                token = FenReader.ToUpper(token);
                if (token == 'K' || token == 'Q')
                {
                    int dir = token == 'K' ? -1 : 1;
                    Square sq = Square.RelativeSquare(c, token == 'K' ? Square.SQ_H1 : Square.SQ_A1);
                    for (int i = 0; i < 7; ++i, sq = (Square)(sq + dir))
                    {
                        Piece pc = PieceOn(sq);
                        if (pc == king) { ksq = sq; break; }
                        else if (pc == rook && rsq == Square.SQ_NONE) rsq = sq;
                    }
                }
                else if (token >= 'A' && token <= 'H')
                {
                    Square candidate = Square.MakeSquare((File)(token - 'A'), Rank.RelativeRank(c, Rank.RANK_1));
                    if (PieceOn(candidate) == rook) rsq = candidate;
                    Square sq = Square.RelativeSquare(c, Square.SQ_B1);
                    for (int i = 0; i < 6; ++i, ++sq)
                        if (PieceOn(sq) == king) ksq = sq;
                }
                else
                    return new PositionSetError("Invalid FEN. Expected castling rights. Got: " + token);

                if (ksq != Square.SQ_NONE && rsq != Square.SQ_NONE)
                    SetCastlingRight(c, rsq);
            }

            bool enpassant = false, legalEP = false;
            char col = '-';
            if (ss.Read(out token)) col = token;
            if (col != '-')
            {
                if (!ss.Read(out char row))
                    return new PositionSetError("Invalid FEN. Unexpected end of stream.");
                if (col >= 'a' && col <= 'h' && row == (sideToMove == Color.WHITE ? '6' : '3'))
                {
                    si.epSquare = Square.MakeSquare((File)(col - 'a'), (Rank)(row - '1'));
                    Bitboard pawns = Attacks.AttacksBB(PieceType.PAWN, si.epSquare, ~sideToMove) & Pieces(sideToMove, PieceType.PAWN);
                    Bitboard target = Pieces(~sideToMove, PieceType.PAWN) & (si.epSquare + Direction.PawnPush(~sideToMove));
                    // El primer XOR devuelve ulong; restaurar Bitboard antes de operar con Square.
                    Bitboard occ = (Bitboard)(Pieces() ^ target) ^ si.epSquare;
                    enpassant = pawns != 0 && target != 0
                             && (Pieces() & (si.epSquare | (si.epSquare + Direction.PawnPush(sideToMove)))) == 0;
                    while (pawns)
                        legalEP |= (AttackersTo(SquareOf(PieceType.KING, sideToMove), occ ^ Bitboards.PopLsb(ref pawns))
                                    & Pieces(~sideToMove) & ~target) == 0;
                }
                else return new PositionSetError("Invalid FEN. Invalid en-passant square.");
            }
            if (!enpassant || !legalEP) si.epSquare = Square.SQ_NONE;

            ss.ReadInt(ref si.rule50);
            ss.ReadInt(ref gamePly);
            if (si.rule50 < 0 || si.rule50 > 32767)
                return new PositionSetError("Unsupported position. Rule50 counter out of range.");
            if (gamePly < 0 || gamePly > 100000)
                return new PositionSetError("Unsupported position. Game ply out of range.");
            gamePly = Math.Max(2 * (gamePly - 1), 0) + (sideToMove == Color.BLACK ? 1 : 0);
            chess960 = isChess960;
            SetState();
            if (AttackersToExist(SquareOf(PieceType.KING, ~sideToMove), Pieces(), sideToMove))
                return new PositionSetError("Unsupported position. King can be captured.");
            Debug.Assert(PosIsOk());
            return null;
        }

        /// <summary>SF19-000420: inicialización por código de material, por ejemplo KBPKN.</summary>
        public PositionSetError? Set(string code, Color c, StateInfo si)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (si == null) throw new ArgumentNullException(nameof(si));
            RequireColor(c);
            if (code.Length == 0 || code[0] != 'K')
                throw new ArgumentException("El código requiere dos lados que comiencen por K.", nameof(code));
            int secondKing = code.IndexOf('K', 1);
            int separator = code.IndexOf('v');
            if (secondKing < 0)
                throw new ArgumentException("El código requiere dos lados que comiencen por K.", nameof(code));
            string[] sides = { code.Substring(secondKing), code.Substring(0, separator < 0 ? secondKing : Math.Min(separator, secondKing)) };
            if (sides[0].Length > 7 || sides[1].Length > 7)
                throw new ArgumentException("Cada lado del código debe contener entre una y siete piezas.", nameof(code));
            sides[c] = FenReader.SwapToLower(sides[c]);
            string fenStr = "8/" + sides[0] + (char)(8 - sides[0].Length + '0') + "/8/8/8/8/" + sides[1]
                          + (char)(8 - sides[1].Length + '0') + "/8 w - - 0 10";
            return Set(fenStr, false, si);
        }

        /// <summary>SF19-000421: FEN normalizado; Chess960 usa letras de archivo Shredder-FEN.</summary>
        public string Fen()
        {
            StateInfo state = CurrentState;
            var ss = new StringBuilder();
            for (Rank r = Rank.RANK_8;; --r)
            {
                // El original incrementa File hasta 9 tras una fila vacía; ese valor
                // queda fuera del dominio del wrapper. Solo avanzamos mientras hay pieza.
                for (File f = File.FILE_A; f <= File.FILE_H;)
                {
                    int emptyCnt = 0;
                    for (; f <= File.FILE_H && Empty(Square.MakeSquare(f, r)); ++f) ++emptyCnt;
                    if (emptyCnt != 0) ss.Append(emptyCnt);
                    if (f <= File.FILE_H)
                    {
                        ss.Append(PieceToChar[PieceOn(Square.MakeSquare(f, r))]);
                        ++f;
                    }
                }
                if (r == Rank.RANK_1) break;
                ss.Append('/');
            }
            ss.Append(sideToMove == Color.WHITE ? " w " : " b ");
            if (CanCastle(CastlingRights.WHITE_OO)) ss.Append(chess960 ? (char)('A' + File.FileOf(CastlingRookSquare(CastlingRights.WHITE_OO))) : 'K');
            if (CanCastle(CastlingRights.WHITE_OOO)) ss.Append(chess960 ? (char)('A' + File.FileOf(CastlingRookSquare(CastlingRights.WHITE_OOO))) : 'Q');
            if (CanCastle(CastlingRights.BLACK_OO)) ss.Append(chess960 ? (char)('a' + File.FileOf(CastlingRookSquare(CastlingRights.BLACK_OO))) : 'k');
            if (CanCastle(CastlingRights.BLACK_OOO)) ss.Append(chess960 ? (char)('a' + File.FileOf(CastlingRookSquare(CastlingRights.BLACK_OOO))) : 'q');
            if (!CanCastle(CastlingRights.ANY_CASTLING)) ss.Append('-');
            ss.Append(state.epSquare == Square.SQ_NONE ? " - " : " " + UciFormatting.Square(state.epSquare) + " ");
            ss.Append(state.rule50.ToString(CultureInfo.InvariantCulture)).Append(' ')
              .Append((1 + (gamePly - (sideToMove == Color.BLACK ? 1 : 0)) / 2).ToString(CultureInfo.InvariantCulture));
            return ss.ToString();
        }

        /// <summary>SF19-000443: invierte filas/colores mediante FEN y vuelve a cargar en el mismo estado.</summary>
        public PositionSetError? Flip()
        {
            string[] parts = Fen().Split(' ');
            string[] ranks = parts[0].Split('/');
            Array.Reverse(ranks);
            string f = FenReader.SwapCase(string.Join("/", ranks) + " " + (parts[1] == "w" ? "B " : "W ") + parts[2] + " ");
            f += parts[3] == "-" ? "-" : parts[3].Substring(0, 1) + (parts[3][1] == '3' ? "6" : "3");
            f += " " + parts[4] + " " + parts[5];
            return Set(f, IsChess960(), CurrentState);
        }

        // Solo las extracciones usadas arriba: ASCII, noskipws, ws y enteros decimales con fail/eof.
        // No amplía el parser del original ni convierte el error de stream en un error FEN nuevo.
        private sealed class FenReader
        {
            private readonly string text;
            private int cursor;
            private bool failed;
            public bool Eof { get; private set; }
            public FenReader(string text) => this.text = text;
            public static bool IsSpace(char c) => c == ' ' || (c >= '\t' && c <= '\r');
            public static char ToUpper(char c) => c >= 'a' && c <= 'z' ? (char)(c - ('a' - 'A')) : c;
            public static string SwapToLower(string value)
            {
                var chars = value.ToCharArray();
                for (int i = 0; i < chars.Length; ++i)
                    if (chars[i] >= 'A' && chars[i] <= 'Z') chars[i] = (char)(chars[i] + ('a' - 'A'));
                return new string(chars);
            }
            public static string SwapCase(string value)
            {
                var chars = value.ToCharArray();
                for (int i = 0; i < chars.Length; ++i)
                    chars[i] = chars[i] >= 'a' && chars[i] <= 'z' ? ToUpper(chars[i])
                        : chars[i] >= 'A' && chars[i] <= 'Z' ? (char)(chars[i] + ('a' - 'A')) : chars[i];
                return new string(chars);
            }
            public bool Read(out char value)
            {
                value = default;
                if (failed) return false;
                if (cursor == text.Length) { failed = Eof = true; return false; }
                value = text[cursor++];
                return true;
            }
            public void SkipWhite()
            {
                if (failed) return;
                while (cursor < text.Length && IsSpace(text[cursor])) ++cursor;
                if (cursor == text.Length) Eof = true;
            }
            public void ReadInt(ref int value)
            {
                if (failed || Eof) { failed = true; return; }
                SkipWhite();
                if (Eof) { failed = true; return; }
                bool negative = false;
                if (text[cursor] == '-' || text[cursor] == '+') negative = text[cursor++] == '-';
                bool any = false;
                long magnitude = 0;
                while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
                {
                    any = true;
                    magnitude = Math.Min(2147483649L, magnitude * 10 + text[cursor++] - '0');
                }
                long signed = negative ? -magnitude : magnitude;
                value = (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, signed));
                if (cursor == text.Length) Eof = true;
                if (!any || signed < int.MinValue || signed > int.MaxValue) failed = true;
            }
        }
    }
}
