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
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Original bench/speedtest command generation, without allocating engine resources.</summary>
    public static class BenchmarkPlans
    {
        public const string SpeedTestCommand = "speedtest";
        public static BenchmarkCommands SetupBench(string currentFen, string arguments)
        {
            if (currentFen == null || arguments == null) throw new ArgumentNullException();
            string[] tokens = UciCommands.Tokens(arguments);
            string ttSize = tokens.Length > 0 ? tokens[0] : "16";
            string threads = tokens.Length > 1 ? tokens[1] : "1";
            string limit = tokens.Length > 2 ? tokens[2] : "13";
            string fenFile = tokens.Length > 3 ? tokens[3] : "default";
            string limitType = tokens.Length > 4 ? tokens[4] : "depth";
            string go = limitType == "eval" ? "eval" : "go " + limitType + " " + limit;
            IEnumerable<string> fens = fenFile == "default" ? BenchmarkPositions.Defaults
                : fenFile == "current" ? new[] { currentFen } : ReadPositions(fenFile);
            var list = new List<string>
            {
                "setoption name Threads value " + threads,
                "setoption name Hash value " + ttSize,
                "ucinewgame"
            };
            foreach (string fen in fens)
                if (fen.IndexOf("setoption", StringComparison.Ordinal) >= 0) list.Add(fen);
                else { list.Add("position fen " + fen); list.Add(go); }
            return new BenchmarkCommands(list);
        }
        private static IEnumerable<string> ReadPositions(string path)
        {
            // getline drops LF; native Windows text mode also translates CRLF.
            // Preserve other characters, including a BOM, rather than silently changing a FEN.
            using var reader = new StreamReader(path, new UTF8Encoding(false, true), false);
            var line = new StringBuilder();
            int c;
            while ((c = reader.Read()) >= 0)
            {
                if (c != '\n') { line.Append((char)c); continue; }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && line.Length > 0 && line[line.Length - 1] == '\r') --line.Length;
                if (line.Length != 0) yield return line.ToString();
                line.Clear();
            }
            if (line.Length != 0) yield return line.ToString();
        }
        public static BenchmarkSetup SetupBenchmark(string arguments, Action<string>? diagnostic = null) =>
            SetupBenchmark(arguments, Infrastructure.Numa.NumaTopology.HardwareConcurrency, Infrastructure.Numa.NumaTopology.MaximumThreads, Engine.MaxHashMB, diagnostic);

        /// <summary>Explicit platform limits make plans reproducible without creating threads or a TT.</summary>
        public static BenchmarkSetup SetupBenchmark(string arguments, int hardwareThreads, int maxThreads, int maxHashMb, Action<string>? diagnostic = null)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (hardwareThreads < 0 || maxThreads < 1 || maxHashMb < 1) throw new ArgumentOutOfRangeException();
            var input = new IntegerStream(arguments);
            BenchmarkSetup setup = default;
            int Clamp(string what, long value, long low, long high)
            {
                long fixedValue = Math.Max(low, Math.Min(value, high));
                if (fixedValue != value) diagnostic?.Invoke(FormattableString.Invariant($"info string speedtest: {what} {value} is outside [{low}, {high}]; using {fixedValue}"));
                return (int)fixedValue;
            }
            if (!input.TryRead(out long requested)) setup.threads = hardwareThreads;
            else
            {
                setup.threads = Clamp("threads", requested, 1, maxThreads);
                setup.originalInvocation += setup.threads.ToString(CultureInfo.InvariantCulture);
            }
            if (!input.TryRead(out requested)) setup.ttSize = Clamp("hash", 128L * setup.threads, 1, maxHashMb);
            else
            {
                setup.ttSize = Clamp("hash", requested, 1, maxHashMb);
                setup.originalInvocation += " " + setup.ttSize.ToString(CultureInfo.InvariantCulture);
            }
            long desiredTimeS;
            if (!input.TryRead(out desiredTimeS)) desiredTimeS = 150;
            else
            {
                desiredTimeS = Clamp("seconds", desiredTimeS, 1, int.MaxValue / 1000);
                setup.originalInvocation += " " + desiredTimeS.ToString(CultureInfo.InvariantCulture);
            }
            setup.filledInvocation = FormattableString.Invariant($"{setup.threads} {setup.ttSize} {desiredTimeS}");
            double CorrectedTime(int ply) => 50000.0 / (ply + 15.0);
            float totalTime = 0;
            foreach (var game in BenchmarkPositions.Games)
                for (int i = 0; i < game.Count; ++i) totalTime += (float)CorrectedTime(i + 1);
            float timeScaleFactor = (float)(desiredTimeS * 1000) / totalTime;
            var commands = new List<string>();
            foreach (var game in BenchmarkPositions.Games)
            {
                commands.Add("ucinewgame");
                int ply = 1;
                foreach (string fen in game)
                {
                    commands.Add("position fen " + fen);
                    int correctedTime = (int)(CorrectedTime(ply++) * timeScaleFactor);
                    commands.Add("go movetime " + correctedTime.ToString(CultureInfo.InvariantCulture));
                }
            }
            setup.commands = new BenchmarkCommands(commands);
            return setup;
        }
        // Formatted i64 extraction retains a failed stream and accepts an integer prefix,
        // as std::istream does (e.g. "2x" consumes 2, then the next extraction fails).
        private sealed class IntegerStream
        {
            private readonly string text;
            private int offset;
            private bool failed;
            internal IntegerStream(string text) { this.text = text; }
            internal bool TryRead(out long value)
            {
                value = 0;
                if (failed) return false;
                while (offset < text.Length && " \t\r\n\v\f".IndexOf(text[offset]) >= 0) ++offset;
                int first = offset;
                if (offset < text.Length && (text[offset] == '+' || text[offset] == '-')) ++offset;
                int digits = offset;
                while (offset < text.Length && text[offset] >= '0' && text[offset] <= '9') ++offset;
                if (digits == offset || !long.TryParse(text.Substring(first, offset - first), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                { failed = true; return false; }
                return true;
            }
        }
    }
}
