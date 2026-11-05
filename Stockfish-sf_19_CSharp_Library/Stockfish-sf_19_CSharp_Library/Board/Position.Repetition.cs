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
using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    public sealed partial class Position
    {
        /// <summary>SF19-000440: repetición relativa a la raíz; conserva distancia y signo originales.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRepetition(int ply)
        {
            int repetition = CurrentState.repetition;
            return repetition != 0 && repetition < ply;
        }

        /// <summary>SF19-000441: alguna repetición en el historial reversible desde la última barrera.</summary>
        public bool HasRepeated()
        {
            StateInfo stc = CurrentState;
            int end = Math.Min(stc.rule50, stc.pliesFromNull);
            while (end-- >= 4)
            {
                if (stc.repetition != 0) return true;
                stc = stc.previous!;
            }
            return false;
        }

        /// <summary>SF19-000439: regla 50 o repetición; no detecta ahogado ni todas las causas de tablas.</summary>
        public bool IsDraw(int ply)
        {
            if (CurrentState.rule50 > 99 && (Checkers() == 0 || new MoveList(this, GenType.LEGAL).Size() != 0))
                return true;
            return IsRepetition(ply);
        }

        // The caller owns the buffer for this search activation; public snapshots remain independent.
        internal bool IsDraw(int ply, Move[] scratch)
        {
            if (CurrentState.rule50 > 99 && (Checkers() == 0 || MoveGenerator.Generate(this, scratch, 0, GenType.LEGAL) != 0))
                return true;
            return IsRepetition(ply);
        }

        /// <summary>SF19-000442: consulta el historial y las jugadas reversibles de las tablas cuckoo.</summary>
        /// <remarks>Usa st.key cruda, no Key() con AdjustKey50. Requiere una cadena de estados válida.</remarks>
        public bool UpcomingRepetition(int ply)
        {
            StateInfo state = CurrentState;
            int end = Math.Min(state.rule50, state.pliesFromNull);
            if (end < 3) return false;

            Key originalKey = state.key;
            StateInfo stp = state.previous!;
            Key other = originalKey ^ stp.key ^ Zobrist.Side;
            for (int i = 3; i <= end; i += 2)
            {
                stp = stp.previous!;
                other ^= stp.key ^ stp.previous!.key ^ Zobrist.Side;
                stp = stp.previous!;
                if (other != 0) continue;

                Key moveKey = originalKey ^ stp.key;
                int j = RepetitionTable.H1(moveKey);
                if (RepetitionTable.KeyAt(j) == moveKey
                    || RepetitionTable.KeyAt(j = RepetitionTable.H2(moveKey)) == moveKey)
                {
                    Move move = RepetitionTable.MoveAt(j);
                    Square s1 = move.FromSq(), s2 = move.ToSq();
                    if (((Attacks.BetweenBB(s1, s2) ^ s2) & Pieces()) == 0)
                    {
                        if (ply > i) return true;
                        if (stp.repetition != 0) return true;
                    }
                }
            }
            return false;
        }
    }
}
