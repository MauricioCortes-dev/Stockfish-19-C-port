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

using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000107. Four shared entries; Assign/CopyFrom are not an aggregate transaction.</summary>
    public struct CorrectionBundle<T, TScalar, TLimit>
        where T : struct where TScalar : struct, ISignedAtomicStorage<T> where TLimit : struct, IStatsLimit
    {
        public StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit> Pawn;
        public StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit> Minor;
        public StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit> NonPawnWhite;
        public StatsEntry<T, SharedStatsStorage<T, TScalar>, TLimit> NonPawnBlack;
        public CorrectionBundle(T value) { this = default; Assign(value); }
        public CorrectionBundle(ref CorrectionBundle<T, TScalar, TLimit> source) { this = default; CopyFrom(ref source); }
        public void Assign(T value)
        { Pawn.Assign(value); Minor.Assign(value); NonPawnWhite.Assign(value); NonPawnBlack.Assign(value); }
        public void CopyFrom(ref CorrectionBundle<T, TScalar, TLimit> source)
        {
            Pawn.CopyFrom(ref source.Pawn); Minor.CopyFrom(ref source.Minor);
            NonPawnWhite.CopyFrom(ref source.NonPawnWhite); NonPawnBlack.CopyFrom(ref source.NonPawnBlack);
        }
    }
}
