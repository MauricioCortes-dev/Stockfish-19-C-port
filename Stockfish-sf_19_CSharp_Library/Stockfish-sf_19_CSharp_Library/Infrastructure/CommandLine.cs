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
using System.IO;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Owns Unicode argv supplied by the CLR; executable name occupies slot zero.</summary>
    public sealed class CommandLine
    {
        private readonly string[] arguments;
        public IReadOnlyList<string> Arguments { get; }
        public int Argc => arguments.Length;

        public CommandLine(string executable, IEnumerable<string> args)
        {
            if (executable == null || args == null)
                throw new ArgumentNullException();
            var all = new List<string>
            {
                executable
            };
            foreach (string s in args)
                all.Add(s ?? throw new ArgumentException("Null argument."));
            arguments = all.ToArray();
            Arguments = Array.AsReadOnly(arguments);
        }

        public string Command => string.Join(" ", arguments, 1, arguments.Length - 1);

        public static string GetBinaryDirectory(string executable)
        {
            if (executable == null)
                throw new ArgumentNullException(nameof(executable));
            // AppContext identifies the application assembly even when the launcher is dotnet.
            string? parent = Path.GetDirectoryName(executable);
            if (!string.IsNullOrEmpty(parent))
                return parent;
            string? root = Path.GetPathRoot(executable);
            return string.IsNullOrEmpty(root) ? "." : root;
        }

        public static string GetWorkingDirectory() => Directory.GetCurrentDirectory();
        public static string ApplicationDirectory => AppContext.BaseDirectory;
    }
}
