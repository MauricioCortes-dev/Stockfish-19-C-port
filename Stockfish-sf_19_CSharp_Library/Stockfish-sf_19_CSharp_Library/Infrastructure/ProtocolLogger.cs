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
using System.Text;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Per-engine character log. Reader/writer adapters borrow it; no global Console redirection.</summary>
    public sealed class ProtocolLogger : IDisposable
    {
        private readonly object gate = new object();
        private StreamWriter? file;
        private char last = '\n';
        private bool disposed;
        public void Start(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            lock (gate)
            {
                RequireAlive();
                var previous = file; file = null; previous?.Dispose();
                if (path.Length != 0) file = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read), new UTF8Encoding(false));
            }
        }
        internal void Record(char c, bool input)
        {
            lock (gate)
            {
                RequireAlive();
                if (file == null) return;
                if (last == '\n') file.Write(input ? ">> " : "<< ");
                file.Write(c); last = c;
            }
        }
        public void Flush() { lock (gate) { RequireAlive(); file?.Flush(); } }
        private void RequireAlive() { if (disposed) throw new ObjectDisposedException(nameof(ProtocolLogger)); }
        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                try { file?.Dispose(); } finally { file = null; disposed = true; }
            }
        }
    }
}
