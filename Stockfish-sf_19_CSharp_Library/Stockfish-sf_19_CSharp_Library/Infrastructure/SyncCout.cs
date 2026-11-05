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
using System.IO;
using System.Threading;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Whole-message serialization per destination; callers must pair Start/End.</summary>
    public sealed class SyncCout
    {
        private readonly object gate = new object ();
        public void Start() => Monitor.Enter(gate);
        public void End() => Monitor.Exit(gate);
        public void Write(TextWriter output, Action<TextWriter> write)
        {
            if (output == null || write == null)
                throw new ArgumentNullException();
            Start();
            try
            {
                write(output);
                output.Flush();
            }
            finally
            {
                End();
            }
        }
    }
}
