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
    /// <summary>SF19-000901: StatsEntry&lt;i16,8192,false&gt;, retaining its own managed identity.</summary>
    public struct TTMoveHistory
    {
        public struct Limit8192 : IStatsLimit { public int Limit => 8192; }
        private StatsEntry<short, LocalStatsStorage<short, AtomicInt16>, Limit8192> entry;
        public TTMoveHistory(short value) { entry = new StatsEntry<short, LocalStatsStorage<short, AtomicInt16>, Limit8192>(value); }
        public TTMoveHistory(ref TTMoveHistory source) { entry = default; CopyFrom(ref source); }
        public short Load() => entry.Load();
        public void Assign(short value) => entry.Assign(value);
        public void Update(int bonus) => entry.Update(bonus);
        public void CopyFrom(ref TTMoveHistory source) => entry.CopyFrom(ref source.entry);
        public static implicit operator short(TTMoveHistory value) => value.Load();
        public static implicit operator TTMoveHistory(short value) => new TTMoveHistory(value);
    }
}
