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
using System.Globalization;
using System.IO;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Original dbg_* counters, owned per engine. Clear/Print observe fields independently.</summary>
    public sealed class DebugStatistics
    {
        public const int MaxDebugSlots = 32;
        private readonly DebugInfo[] hit = Create(2), mean = Create(2), stdev = Create(3), correl = Create(6);
        private readonly DebugExtremes[] extremes = CreateExtremes();
        private static DebugInfo[] Create(int n)
        {
            var a = new DebugInfo[MaxDebugSlots];
            for (int i = 0; i < a.Length; ++i)
                a[i] = new DebugInfo(n);
            return a;
        }

        private static DebugExtremes[] CreateExtremes()
        {
            var a = new DebugExtremes[MaxDebugSlots];
            for (int i = 0; i < a.Length; ++i)
                a[i] = new DebugExtremes();
            return a;
        }

        public void HitOn(bool condition, int slot = 0)
        {
            hit[slot].Add(0, 1);
            if (condition)
                hit[slot].Add(1, 1);
        }

        public void MeanOf(long value, int slot = 0)
        {
            mean[slot].Add(0, 1);
            mean[slot].Add(1, value);
        }

        public void StdevOf(long value, int slot = 0)
        {
            stdev[slot].Add(0, 1);
            stdev[slot].Add(1, value);
            stdev[slot].Add(2, unchecked(value * value));
        }

        public void ExtremesOf(long value, int slot = 0)
        {
            extremes[slot].Add(0, 1);
            extremes[slot].Maximum(1, value);
            extremes[slot].Minimum(2, value);
        }

        public void CorrelOf(long x, long y, int slot = 0)
        {
            DebugInfo d = correl[slot];
            d.Add(0, 1);
            d.Add(1, x);
            d.Add(2, unchecked(x * x));
            d.Add(3, y);
            d.Add(4, unchecked(y * y));
            d.Add(5, unchecked(x * y));
        }

        public void Clear()
        {
            foreach (var bank in new DebugInfo[][]
            {
                hit,
                mean,
                stdev,
                correl,
                extremes
            }

            )
                foreach (var d in bank)
                    d.Clear();
        }

        private static string Number(double d) => double.IsNaN(d) ? "nan" : double.IsPositiveInfinity(d) ? "inf" : double.IsNegativeInfinity(d) ? "-inf" : d.ToString("G6", CultureInfo.InvariantCulture).Replace('E', 'e');
        public void Print(TextWriter output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            for (int i = 0; i < MaxDebugSlots; ++i)
            {
                long n = hit[i][0];
                if (n != 0)
                    output.WriteLine(FormattableString.Invariant($"Hit #{i}: Total {n} Hits {hit[i][1]} Hit Rate (%) {Number(100.0 * ((double)hit[i][1] / n))}"));
            }

            for (int i = 0; i < MaxDebugSlots; ++i)
            {
                long n = mean[i][0];
                if (n != 0)
                    output.WriteLine(FormattableString.Invariant($"Mean #{i}: Total {n} Mean {Number((double)mean[i][1] / n)}"));
            }

            for (int i = 0; i < MaxDebugSlots; ++i)
            {
                long n = stdev[i][0];
                if (n != 0)
                {
                    double e = (double)stdev[i][1] / n;
                    output.WriteLine(FormattableString.Invariant($"Stdev #{i}: Total {n} Stdev {Number(Math.Sqrt((double)stdev[i][2] / n - e * e))}"));
                }
            }

            for (int i = 0; i < MaxDebugSlots; ++i)
            {
                long n = extremes[i][0];
                if (n != 0)
                    output.WriteLine(FormattableString.Invariant($"Extremity #{i}: Total {n} Min {extremes[i][2]} Max {extremes[i][1]}"));
            }

            for (int i = 0; i < MaxDebugSlots; ++i)
            {
                DebugInfo d = correl[i];
                long n = d[0];
                if (n == 0)
                    continue;
                double x = (double)d[1] / n, y = (double)d[3] / n;
                double r = ((double)d[5] / n - x * y) / (Math.Sqrt((double)d[2] / n - x * x) * Math.Sqrt((double)d[4] / n - y * y));
                output.WriteLine(FormattableString.Invariant($"Correl. #{i}: Total {n} Coefficient {Number(r)}"));
            }

            output.Flush();
        }
    }
}
