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

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Resultado inmutable: conteo/desglose o el PositionSetError devuelto al cargar el FEN.</summary>
    public sealed class PerftResult
    {
        public bool Success => Error == null;
        public ulong Nodes { get; }
        public IReadOnlyList<PerftEntry> Entries { get; }
        public PositionSetError? Error { get; }
        // Los factories transfieren un array nuevo que no conserva ningún productor.
        private PerftResult(ulong nodes, PerftEntry[] entries, PositionSetError? error)
        {
            Nodes = nodes;
            Entries = Array.AsReadOnly(entries);
            Error = error;
        }
        internal static PerftResult Completed(ulong nodes, List<PerftEntry> entries) =>
            new PerftResult(nodes, entries.ToArray(), null);
        internal static PerftResult Rejected(PositionSetError error) =>
            new PerftResult(0, Array.Empty<PerftEntry>(), error);
    }
}
