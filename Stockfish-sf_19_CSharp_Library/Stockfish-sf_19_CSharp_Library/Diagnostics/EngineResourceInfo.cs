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
using Stockfish_sf_19_CSharp_Library.Infrastructure.Numa;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Resources of the current managed implementation, not simulated native topology.</summary>
    public readonly struct EngineResourceInfo
    {
        public int AvailableProcessors { get; }
        /// <summary>Configured worker count; workers are created lazily.</summary>
        public int Threads { get; }
        public bool UsesLargePages => false;
        public string ThreadBinding { get; }
        private readonly string topology;
        public string ProcessorDescription => topology;
        internal EngineResourceInfo(int threads, NumaConfig config, string binding)
        {
            AvailableProcessors = config.CpuCount; Threads = threads; ThreadBinding = binding.Length == 0 ? "none" : binding;
            topology = config.ToString();
        }
    }
}
