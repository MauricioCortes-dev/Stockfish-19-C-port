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
    /// <summary>Non-owning output adapter; only characters successfully sent are logged.</summary>
    public sealed class ProtocolLogWriter : TextWriter
    {
        private readonly TextWriter destination;
        private readonly ProtocolLogger log;
        public ProtocolLogWriter(TextWriter destination, ProtocolLogger log) { this.destination = destination ?? throw new ArgumentNullException(nameof(destination)); this.log = log ?? throw new ArgumentNullException(nameof(log)); }
        public override Encoding Encoding => destination.Encoding;
        public override string NewLine { get => destination.NewLine; set => destination.NewLine = value; }
        public override void Write(char value) { destination.Write(value); log.Record(value, false); }
        public override void Write(string? value) { if (value != null) foreach (char c in value) Write(c); }
        public override void Write(char[] buffer, int index, int count)
        {
            destination.Write(buffer, index, count);
            for (int i = index; i < index + count; ++i) log.Record(buffer[i], false);
        }
        public override void WriteLine() => Write(destination.NewLine);
        public override void WriteLine(string? value) { Write(value); WriteLine(); }
        public override void Flush() { log.Flush(); destination.Flush(); }
        protected override void Dispose(bool disposing) { if (disposing) Flush(); }
    }
}
