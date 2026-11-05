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
using System.Collections.Generic;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Conteo escalar original de hojas legales; no aplica tablas ni evaluación.</summary>
    public static class Perft
    {
        /// <summary>SF19-000410: recorre una posición preparada y conserva el desglose en su orden original.</summary>
        /// <remarks>Root sustituye la plantilla. Como en C++, depth&lt;=1 cuenta jugadas legales
        /// de la raíz, incluso para cero/negativos. No detiene el recorrido por regla 50 o repetición.</remarks>
        public static PerftResult Run(Position pos, Depth depth)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            var entries = new List<PerftEntry>();
            ulong nodes = Traverse(pos, depth, true, entries);
            return PerftResult.Completed(nodes, entries);
        }
        /// <summary>SF19-000411: conserva la alternativa de error del variant original.</summary>
        public static PerftResult Run(string fen, Depth depth, bool isChess960)
        {
            var st = new StateInfo();
            var pos = new Position();
            PositionSetError? error = pos.Set(fen, isChess960, st);
            if (error != null) return PerftResult.Rejected(error);
            return Run(pos, depth);
        }
        private static ulong Traverse(Position pos, Depth depth, bool root, List<PerftEntry> entries)
        {
            // Un estado por nivel, reutilizado entre hermanos únicamente después de UndoMove.
            var st = new StateInfo();
            ulong cnt, nodes = 0;
            bool leaf = depth == 2;
            foreach (Move m in new MoveList(pos, GenType.LEGAL))
            {
                if (root && depth <= 1)
                {
                    cnt = 1;
                    nodes = unchecked(nodes + 1);
                }
                else
                {
                    pos.DoMove(m, st);
                    try
                    {
                        cnt = leaf ? (ulong)new MoveList(pos, GenType.LEGAL).Size()
                                   : Traverse(pos, depth - 1, false, entries);
                        nodes = unchecked(nodes + cnt);
                    }
                    finally { pos.UndoMove(m); }
                }
                if (root) entries.Add(new PerftEntry(m, cnt));
            }
            return nodes;
        }
    }
}
