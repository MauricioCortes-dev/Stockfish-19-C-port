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
namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Runtime
{
    /// <summary>Build requirements, distinct from detected CPU capabilities. Portable IL requires no x86 ISA.</summary>
    public static class BuildConfiguration
    {
        public const bool HasPopCnt = false;
        public const bool HasPext = false;
        public static bool Is64Bit => IntPtr.Size == 8;
        public static bool IsLittleEndian => BitConverter.IsLittleEndian;
    }
}
