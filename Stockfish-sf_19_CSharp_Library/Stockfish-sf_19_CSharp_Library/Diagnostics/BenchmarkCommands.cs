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
using System.Collections;
using System.Collections.Generic;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Value-semantic command vector. Mutations publish a new private array.</summary>
    public struct BenchmarkCommands : IReadOnlyList<string>
    {
        private string[]? items;
        internal BenchmarkCommands(IEnumerable<string> source) { items = new List<string>(source).ToArray(); }
        public int Count => items?.Length ?? 0;
        public string this[int index]
        {
            get => (items ?? Array.Empty<string>())[index];
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                var copy = (string[])(items ?? Array.Empty<string>()).Clone();
                copy[index] = value;
                items = copy;
            }
        }
        public void Add(string command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            var copy = new string[Count + 1];
            if (items != null) Array.Copy(items, copy, items.Length);
            copy[Count] = command;
            items = copy;
        }
        public void Clear() => items = null;
        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)(items ?? Array.Empty<string>())).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
