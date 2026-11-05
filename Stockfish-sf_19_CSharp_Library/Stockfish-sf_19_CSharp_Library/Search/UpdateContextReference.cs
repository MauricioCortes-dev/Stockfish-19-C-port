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
namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Owned storage for the original const UpdateContext reference. Update only while idle.</summary>
    /// <remarks>Reference assignment aliases this owner. Construct from Value for an independent context copy.</remarks>
    public sealed class UpdateContextReference
    {
        public UpdateContext Value;
        public UpdateContextReference(UpdateContext value = default)
        {
            Value = value;
        }
    }
}
