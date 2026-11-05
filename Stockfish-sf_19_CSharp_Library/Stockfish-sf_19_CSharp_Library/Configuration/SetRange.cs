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

namespace Stockfish_sf_19_CSharp_Library.Configuration
{
    public readonly struct SetRange
    {
        private readonly Func<int, (int Min, int Max)>? fun;
        private readonly (int Min, int Max) range;
        public SetRange(int min, int max)
        {
            range = (min, max);
            fun = null;
        }

        public SetRange(Func<int, (int Min, int Max)> function)
        {
            fun = function ?? throw new ArgumentNullException(nameof(function));
            range = default;
        }

        public (int Min, int Max) Invoke(int value) => fun == null ? range : fun(value);
        public static (int Min, int Max) DefaultRange(int value) => value > 0 ? (0, checked(2 * value)) : (checked(2 * value), 0);
        public static SetRange Default => new SetRange(DefaultRange);
    }
}
