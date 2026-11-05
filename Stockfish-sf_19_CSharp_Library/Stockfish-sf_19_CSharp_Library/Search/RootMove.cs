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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.MoveGeneration;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000536: mutable root candidate with independent value copies of both PVs.</summary>
    public struct RootMove : IComparable<RootMove>, IEquatable<RootMove>
    {
        public ulong effort;
        public Value score, previousScore, averageScore, meanSquaredScore, uciScore;
        public bool inexactLower, inexactUpper, previousScoreExact;
        public int selDepth, tbRank;
        public Value tbScore;
        public RootPVMoves pv, previousPV;

        public RootMove(Move m)
        {
            this = default;
            score = previousScore = averageScore = uciScore = -Value.VALUE_INFINITE;
            meanSquaredScore = -Value.VALUE_INFINITE * Value.VALUE_INFINITE;
            // C++ leaves tbScore indeterminate. Managed zero initialization is not a native result.
            pv.PushBack(m);
        }

        public bool IsInexact() => inexactLower || inexactUpper;
        public bool IsExactLoss() => score != -Value.VALUE_INFINITE && score.IsLoss() && !IsInexact();
        public void UnsetInexact() { inexactLower = inexactUpper = false; }

        public static bool operator ==(RootMove left, Move right) => left.pv[0] == right;
        public static bool operator !=(RootMove left, Move right) => !(left == right);
        public static bool operator <(RootMove left, RootMove right)
            => right.score != left.score ? right.score < left.score : right.previousScore < left.previousScore;
        public static bool operator >(RootMove left, RootMove right) => right < left;
        public int CompareTo(RootMove other) => this < other ? -1 : other < this ? 1 : 0;

        /// <summary>SF19-000530: extend a one-move root PV with a legal TT reply, restoring the position.</summary>
        /// <remarks>The root move must be legal. Position/RootMove require exclusive use.
        /// Managed exceptions after DoMove also undo the move; scratch Dirties are not restored.</remarks>
        public bool ExtractPonderFromTT(TranspositionTable tt, Position pos)
        {
            if (tt == null) throw new ArgumentNullException(nameof(tt));
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            if (pv.Count != 1 || pv[0] == Move.None())
                throw new InvalidOperationException("Ponder extraction requires exactly one non-null root move.");

            var st = new StateInfo();
            pos.DoMove(pv[0], st); // Native TT argument supplies prefetch only.
            try
            {
                if (!pos.IsDraw(1))
                {
                    var (ttHit, ttData, _) = tt.Probe(pos.Key());
                    if (ttHit && new MoveList(pos, GenType.LEGAL).Contains(ttData.move))
                        pv.PushBack(ttData.move);
                }
            }
            finally { pos.UndoMove(pv[0]); }
            return pv.Count > 1;
        }

        // .NET aggregate equality is separate from the original Move match and score ordering.
        public bool Equals(RootMove other)
            => effort == other.effort && score == other.score && previousScore == other.previousScore
            && averageScore == other.averageScore && meanSquaredScore == other.meanSquaredScore
            && uciScore == other.uciScore && inexactLower == other.inexactLower && inexactUpper == other.inexactUpper
            && previousScoreExact == other.previousScoreExact && selDepth == other.selDepth && tbRank == other.tbRank
            && tbScore == other.tbScore && PvEquals(pv, other.pv) && PvEquals(previousPV, other.previousPV);
        public override bool Equals(object? obj) => obj is RootMove other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = effort.GetHashCode();
                hash = hash * 31 + score.GetHashCode(); hash = hash * 31 + previousScore.GetHashCode();
                hash = hash * 31 + averageScore.GetHashCode(); hash = hash * 31 + meanSquaredScore.GetHashCode();
                hash = hash * 31 + uciScore.GetHashCode(); hash = hash * 31 + inexactLower.GetHashCode();
                hash = hash * 31 + inexactUpper.GetHashCode(); hash = hash * 31 + previousScoreExact.GetHashCode();
                hash = hash * 31 + selDepth; hash = hash * 31 + tbRank; hash = hash * 31 + tbScore.GetHashCode();
                hash = hash * 31 + pv.Count;
                foreach (Move m in pv) hash = hash * 31 + m.GetHashCode();
                hash = hash * 31 + previousPV.Count;
                foreach (Move m in previousPV) hash = hash * 31 + m.GetHashCode();
                return hash;
            }
        }
        private static bool PvEquals(RootPVMoves left, RootPVMoves right)
        {
            if (left.Count != right.Count) return false;
            for (int i = 0; i < left.Count; ++i) if (left[i] != right[i]) return false;
            return true;
        }
    }
}
