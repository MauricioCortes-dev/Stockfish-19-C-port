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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>An explicit native assignment operation; it is not an implicit conversion.</summary>
    public delegate void MultiArrayAssignment<T, TValue>(ref T destination, in TValue value);

    /// <summary>SF19-000186/000187: explicit leaf and recursive child specializations for C# 8.</summary>
    public static class MultiArrayHelper
    {
        public static void Assign<T>(ref T destination, in T value) => destination = value;

        public static MultiArrayAssignment<MultiArray<T, TStorage>, TValue> Nested<T, TStorage, TValue>(
            MultiArrayAssignment<T, TValue> assignment)
            where TStorage : struct, IMultiArrayStorage<T, TStorage>
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));
            return (ref MultiArray<T, TStorage> child, in TValue value) => child.Fill(in value, assignment);
        }
    }
}
