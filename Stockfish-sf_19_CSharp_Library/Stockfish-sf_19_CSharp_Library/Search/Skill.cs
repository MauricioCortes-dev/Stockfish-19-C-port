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
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Infrastructure;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Native statistical handicap. Default PRNG is process-shared and seeded from the monotonic clock.</summary>
    public sealed class Skill
    {
        public const int LowestElo = 1320, HighestElo = 3190;
        public double Level { get; }
        public Move Best { get; private set; } = Move.None();

        private static PRNG random = new PRNG(unchecked((ulong)(long)MonotonicClock.Instance.Now) | 1UL);
        private static readonly object randomLock = new object ();
        private readonly Func<uint>? next;
        public Skill(int skillLevel, int elo, Func<uint>? next = null)
        {
            this.next = next;
            if (elo != 0)
            {
                double e = (double)(elo - LowestElo) / (HighestElo - LowestElo);
                Level = Math.Min(19, Math.Max(0, ((37.2473 * e - 40.8525) * e + 22.2943) * e - 0.311438));
            }
            else
                Level = skillLevel;
        }

        public bool Enabled() => Level < 20;
        public bool TimeToPick(Depth depth) => depth == 1 + (int)Level;
        private uint Next()
        {
            if (next != null)
                return next();
            lock (randomLock)
                return random.Rand<uint>();
        }

        public Move PickBest(RootMoves roots, int multiPV)
        {
            if (multiPV < 1 || multiPV > roots.Count)
                throw new ArgumentOutOfRangeException(nameof(multiPV));
            Value top = roots[0].score, min = top;
            for (int i = 1; i < multiPV; ++i)
            {
                top = Math.Max(top, roots[i].score);
                min = Math.Min(min, roots[i].score);
            }

            int delta = Math.Min(top - min, Values.PawnValue), max = -(int)Value.VALUE_INFINITE;
            double weakness = 120 - 2 * Level;
            for (int i = 0; i < multiPV; ++i)
            {
                int push = checked((int)(weakness * (top - (int)roots[i].score) + delta * (Next() % (uint)(int)weakness))) / 128;
                if (roots[i].score + push >= max)
                {
                    max = roots[i].score + push;
                    Best = roots[i].pv[0];
                }
            }

            return Best;
        }
    }
}
