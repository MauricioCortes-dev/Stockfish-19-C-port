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
    /// <summary>Tablas cuckoo originales de jugadas reversibles utilizadas por Position.UpcomingRepetition.</summary>
    internal static class RepetitionTable
    {
        internal const int Capacity = 8192;
        private static readonly Key[] cuckoo = new Key[Capacity];
        private static readonly Move[] cuckooMove = new Move[Capacity];
        internal static readonly int Count;

        static RepetitionTable()
        {
            int count = 0;
            foreach (Piece pc in Zobrist.Pieces)
                for (Square s1 = Square.SQ_A1; s1 <= Square.SQ_H8; ++s1)
                    for (Square s2 = s1 + 1; s2 <= Square.SQ_H8; ++s2)
                        if (PieceType.TypeOf(pc) != PieceType.PAWN && (Attacks.AttacksBB(PieceType.TypeOf(pc), s1, (Bitboard)0) & s2) != 0)
                        {
                            Move move = new Move(s1, s2);
                            Key key = Zobrist.Psq(pc, s1) ^ Zobrist.Psq(pc, s2) ^ Zobrist.Side;
                            int i = H1(key);
                            while (true)
                            {
                                Key oldKey = cuckoo[i]; cuckoo[i] = key; key = oldKey;
                                Move oldMove = cuckooMove[i]; cuckooMove[i] = move; move = oldMove;
                                if (move == Move.None()) break;
                                i = i == H1(key) ? H2(key) : H1(key);
                            }
                            ++count;
                        }
            if (count != 3668) throw new InvalidOperationException("La tabla cuckoo debe contener 3668 jugadas reversibles.");
            Count = count;
        }

        internal static void Init() { }
        internal static int H1(Key key) => (int)(key & 0x1fffUL);
        internal static int H2(Key key) => (int)((key >> 16) & 0x1fffUL);
        internal static Key KeyAt(int index) => cuckoo[index];
        internal static Move MoveAt(int index) => cuckooMove[index];
    }
}
