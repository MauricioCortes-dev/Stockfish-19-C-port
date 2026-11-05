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

namespace Stockfish_sf_19_CSharp_Library.Configuration
{
    /// <summary>UCI names: ASCII C-locale case folding, independent of the current culture.</summary>
    public sealed class CaseInsensitiveLess : IComparer<string>
    {
        public static CaseInsensitiveLess Instance { get; } = new CaseInsensitiveLess();

        public int Compare(string? a, string? b)
        {
            if (a == null)
                return b == null ? 0 : -1;
            if (b == null)
                return 1;
            for (int i = 0; i < Math.Min(a.Length, b.Length); ++i)
            {
                int x = Fold(a[i]), y = Fold(b[i]);
                if (x != y)
                    return x - y;
            }

            return a.Length.CompareTo(b.Length);
        }

        private static int Fold(char c) => c >= 'A' && c <= 'Z' ? c + 32 : c;
    }
}
