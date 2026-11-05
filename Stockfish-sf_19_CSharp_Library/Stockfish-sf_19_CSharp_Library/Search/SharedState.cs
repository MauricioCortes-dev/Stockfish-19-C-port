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
using Stockfish_sf_19_CSharp_Library.Infrastructure.Numa;
using Stockfish_sf_19_CSharp_Library.Configuration;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;
using Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy;
using WorkerPool = Stockfish_sf_19_CSharp_Library.Infrastructure.Threading.ThreadPool;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Borrowed engine resources, local network replicas and pool-owned per-domain history banks.</summary>
    /// <remarks>Options and network parameters must stay unchanged during jobs. The host owns TT/network lifetime.
    /// Replicas and topology may only change while all workers are retired or idle.</remarks>
    public sealed class SharedState
    {
        public OptionsMap Options { get; }
        public WorkerPool Threads { get; }
        public TranspositionTable TT { get; }
        public Network Network { get; }
        public TbProbe? Tablebases { get; }
        private readonly LazyNumaReplicated<Network>? replicas;
        private readonly LazyNumaReplicatedSystemWide<Network>? systemWide;
        public NumaConfig NumaConfig { get; }
        private readonly Dictionary<int, SharedHistories> histories = new Dictionary<int, SharedHistories>();
        public SharedHistories SharedHistories => HistoryForNode(0);
        public SharedHistories HistoryForNode(int node) => histories.TryGetValue(node, out var bank) ? bank : throw new InvalidOperationException("Node has no workers.");
        public Network NetworkForNode(NumaReplicatedAccessToken token) => systemWide != null ? systemWide[token] : replicas == null ? Network : replicas[token];
        public SharedState(OptionsMap options, WorkerPool threads, TranspositionTable tt, Network network, TbProbe? tablebases = null, NumaConfig? numaConfig = null, LazyNumaReplicated<Network>? networkReplicas = null, LazyNumaReplicatedSystemWide<Network>? systemWideReplicas = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Threads = threads ?? throw new ArgumentNullException(nameof(threads));
            TT = tt ?? throw new ArgumentNullException(nameof(tt));
            Network = network ?? throw new ArgumentNullException(nameof(network));
            Tablebases = tablebases;
            NumaConfig = numaConfig ?? new NumaConfig();
            replicas = networkReplicas; systemWide = systemWideReplicas;
        }
        internal void InstallHistories(SharedHistories? value) { histories.Clear(); if (value != null) histories.Add(0, value); }
        internal void InstallHistory(int node, SharedHistories value) => histories.Add(node, value);
    }
}
