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
using System.Linq;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Memory;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Numa;
using System.Threading;
using System.Threading.Tasks;
using WorkerPool = Stockfish_sf_19_CSharp_Library.Infrastructure.Threading.ThreadPool;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Configuration;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Evaluation;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;
using Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy;

namespace Stockfish_sf_19_CSharp_Library
{
    /// <summary>Reusable engine with a persistent pool of asynchronous search workers. No console dependency.</summary>
    /// <remarks>Commands are serialized. Mutating commands stop and join the previous search.
    /// Search callbacks run on the worker; verification/info callbacks run on the host.
    /// Callbacks may call Stop/PonderHit/SetPonderHit/IsSearching only; schedule other
    /// commands on the host thread. A callback exception faults the returned task and Wait.
    /// Borrowed network weights must remain read-only for the engine's lifetime.</remarks>
    public sealed class Engine : IDisposable
    {
        public static readonly int MaxHashMB = UIntPtr.Size == 8 ? 33554432 : 2048;
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private readonly object commands = new object (), signals = new object ();
        private readonly OptionsMap options = new OptionsMap();
        private readonly TranspositionTable tt = new TranspositionTable();
        private readonly UpdateContextReference updates = new UpdateContextReference();
        private readonly SearchManager manager;
        private readonly string directory;
        private Network? network;
        private readonly NumaReplicationContext numaContext = new NumaReplicationContext(NumaConfig.FromSystem(NumaAutoPolicy.BundledL3(32)));
        private LazyNumaReplicatedSystemWide<Network>? networkReplicas;
        private EvalFile networkFile = new EvalFile();
        private readonly WorkerPool threads = new WorkerPool();
        private readonly TbProbe tablebases = new TbProbe();
        private readonly ProtocolLogger protocolLog = new ProtocolLogger();
        private Position position = new Position();
        private Task<SearchResult>? active;
        private SearchControl? control;
        private bool started, disposed, lastNumaChange;
        private bool? pendingPonder;
        private Action? onStart;
        private Action<string>? verify;
        public Engine(string? binaryDirectory = null, IMonotonicClock? clock = null) : this(binaryDirectory, clock, null, null, null)
        {
        }

        /// <summary>Creates an engine with a tuning registry without changing constructor overload resolution.</summary>
        public static Engine CreateWithTuning(Tune tuning, string? binaryDirectory = null, IMonotonicClock? clock = null)
            => new Engine(binaryDirectory, clock, null, null, tuning ?? throw new ArgumentNullException(nameof(tuning)));

        private Engine(string? binaryDirectory, IMonotonicClock? clock, Network? weights, string? name, Tune? tuning)
        {
            network = weights;
            networkFile.Current = name;
            directory = binaryDirectory ?? CommandLine.ApplicationDirectory;
            manager = new SearchManager(updates, clock);
            position.Set(StartFen, false, new StateInfo());
            options.Add("Debug Log File", new Option("", o => { protocolLog.Start(o.AsString()); return null; }));
            options.Add("NumaPolicy", new Option("auto", o => (lastNumaChange = SetNumaConfigCore(o.AsString()))
                ? NumaConfigInformationAsStringCore() + (threads.Empty ? "" : "\n" + ThreadAllocationInformationAsStringCore())
                : "NumaPolicy: invalid value '" + o.AsString() + "', keeping previous config."));
            options.Add("Threads", new Option(1, 1, threads.MaximumThreads, o =>
            {
                // Network loading stays lazy until the first search, as in the one-worker facade.
                ResizeThreadsCore(o.AsInt());
                return threads.Empty ? null : ThreadAllocationInformationAsStringCore();
            }));
            options.Add("Hash", new Option(16, 1, MaxHashMB, o =>
            {
                tt.Resize(new UIntPtr((uint)o.AsInt()), threads);
                return null;
            }));
            options.Add("Clear Hash", new Option(o =>
            {
                ClearCore();
                return null;
            }));
            options.Add("Ponder", new Option(false));
            options.Add("MultiPV", new Option(1, 1, ChessConstants.MAX_MOVES));
            options.Add("Skill Level", new Option(20, 0, 20));
            options.Add("Move Overhead", new Option(10, 0, 5000));
            options.Add("nodestime", new Option(0, 0, 10000));
            options.Add("UCI_Chess960", new Option(false));
            options.Add("UCI_LimitStrength", new Option(false));
            options.Add("UCI_Elo", new Option(Skill.LowestElo, Skill.LowestElo, Skill.HighestElo));
            options.Add("UCI_ShowWDL", new Option(false));
            options.Add("SyzygyPath", new Option("", o => { InitTablebases(o.AsString()); return null; }));
            options.Add("SyzygyProbeDepth", new Option(1, 1, 100));
            options.Add("Syzygy50MoveRule", new Option(true));
            options.Add("SyzygyProbeLimit", new Option(7, 0, 7));
            options.Add("EvalFile", new Option(name ?? EvalFile.DefaultName, o =>
            {
                LoadCore(o.AsString());
                return null;
            }));
            (tuning ?? new Tune()).Init(options);
            updates.Value.OnStart = SearchStarted;
            tt.Resize(new UIntPtr(16u), threads);
            if (network != null) InstallNetworkReplicas(network);
        }

        public Engine(Network readOnlyNetwork, string networkName, IMonotonicClock? clock = null)
            : this(null, clock, readOnlyNetwork ?? throw new ArgumentNullException(nameof(readOnlyNetwork)),
                string.IsNullOrEmpty(networkName) ? throw new ArgumentException(nameof(networkName)) : networkName, null)
        {
        }

        /// <summary>Creates a tuned engine borrowing the supplied read-only network.</summary>
        public static Engine CreateWithTuning(Tune tuning, Network readOnlyNetwork, string networkName, IMonotonicClock? clock = null)
            => new Engine(null, clock, readOnlyNetwork ?? throw new ArgumentNullException(nameof(readOnlyNetwork)),
                string.IsNullOrEmpty(networkName) ? throw new ArgumentException(nameof(networkName)) : networkName,
                tuning ?? throw new ArgumentNullException(nameof(tuning)));

        public bool IsSearching
        {
            get
            {
                lock (signals)
                    return active != null && !active.IsCompleted;
            }
        }

        public DebugStatistics DebugStats => manager.Diagnostics;
        public void SetOnDiagnostics(Action<DebugStatistics>? listener) => Command(() => manager.OnDiagnostics = listener);

        public string Fen => Read(() => position.Fen());
        public PositionSetError? Flip() => Command(() => position.Flip());
        public string Visualize() => Command(() => PositionDisplay.Format(position, tablebases));
        public EngineResourceInfo ResourceInfo => Read(() => new EngineResourceInfo(options["Threads"].AsInt(), numaContext.Config, ThreadBindingInformationAsStringCore()));
        public System.IO.TextReader LogInput(System.IO.TextReader input) => new ProtocolLogReader(input, protocolLog);
        public System.IO.TextWriter LogOutput(System.IO.TextWriter output) => new ProtocolLogWriter(output, protocolLog);

        public string OptionDescriptions => Read(() => options.ToString());

        public string GetOptionValue(string name) => Read(() => options[name].CurrentValue);
        public void SetInfoListener(Action<string?> listener) => Command(() => options.AddInfoListener(listener));
        public void SetOnUpdateNoMoves(Action<InfoShort>? listener) => Command(() => updates.Value.OnUpdateNoMoves = listener);
        public void SetOnUpdateFull(Action<InfoFull>? listener) => Command(() => updates.Value.OnUpdateFull = listener);
        public void SetOnIteration(Action<InfoIteration>? listener) => Command(() => updates.Value.OnIteration = listener);
        public void SetOnBestmove(Action<string, string>? listener) => Command(() => updates.Value.OnBestmove = listener);
        public void SetOnInfoString(Action<string>? listener) => Command(() => updates.Value.OnInfoString = listener);
        public void SetOnStart(Action? listener) => Command(() => onStart = listener);
        public void SetOnVerifyNetwork(Action<string>? listener) => Command(() => verify = listener);
        public bool SetOption(string payload) => Command(() => options.SetOption(payload));
        public PositionSetError? SetPosition(string fen, IEnumerable<string> moves) => Command(() =>
        {
            if (moves == null)
                throw new ArgumentNullException(nameof(moves));
            // Build transactionally: invalid FEN or moves cannot destroy the last usable position.
            var next = new Position();
            PositionSetError? error = next.Set(fen, options["UCI_Chess960"].AsInt() != 0, new StateInfo());
            if (error != null)
                return error;
            foreach (string text in moves)
            {
                Move move = UciParsing.ToMove(next, text);
                if (move == Move.None())
                    return new PositionSetError("Illegal move: " + text);
                next.DoMove(move, new StateInfo());
            }

            position = next; // StateInfo.previous owns the entire chain, including pre-root repetition.
            return null;
        });
        public Task<SearchResult> Go(LimitsType limits) => Go(limits, null);

        /// <summary>Reports actual prepared resources on the host, before network verification and search.
        /// The optional reporter follows the same non-reentrant callback contract as verification.</summary>
        public Task<SearchResult> Go(LimitsType limits, Action<string>? reportResources)
        {
            EnsureHost();
            lock (commands)
            {
                ThrowIfDisposed();
                Join(true);
                if (limits == null)
                    throw new ArgumentNullException(nameof(limits));
                if (limits.Perft != 0)
                    throw new ArgumentException("Use Perft for perft requests.", nameof(limits));
                // Creating the lazy pool requires weights. Validate the initial load silently on
                // success, then report resources and verification in the original UCI order.
                if (network == null) LoadCore(options["EvalFile"].AsString(), false);
                EnsureWorkers();
                reportResources?.Invoke(NumaConfigInformationAsStringCore());
                reportResources?.Invoke(ThreadAllocationInformationAsStringCore());
                VerifyCore();
                var owned = new LimitsType(limits);
                var signal = new SearchControl();
                lock (signals)
                {
                    control = signal;
                    started = false;
                    pendingPonder = null;
                    active = threads.StartThinking(position, owned, signal);
                    return active;
                }
            }
        }

        public void Stop()
        {
            lock (signals)
                control?.RequestStop();
        }

        public void PonderHit() => SetPonderHit(false);

        /// <summary>Assign the main manager's ponder flag. The last early assignment wins.</summary>
        public void SetPonderHit(bool pondering)
        {
            lock (signals)
            {
                if (active == null || active.IsCompleted)
                    return;
                pendingPonder = pondering;
                if (started)
                    manager.SetPonderHit(pondering);
            }
        }

        private void SearchStarted()
        {
            lock (signals)
            {
                started = true;
                if (pendingPonder.HasValue)
                    manager.SetPonderHit(pendingPonder.Value);
            }

            onStart?.Invoke();
        }

        public void WaitForSearchFinished()
        {
            EnsureHost();
            lock (commands)
            {
                ThrowIfDisposed();
                Join(false);
            }
        }

        private void Join(bool stop)
        {
            if (stop)
                Stop();
            Task<SearchResult>? task;
            lock (signals)
                task = active;
            try
            {
                try { task?.GetAwaiter().GetResult(); }
                finally { threads.WaitForAllJobs(); }
            }
            finally
            {
                lock (signals)
                {
                    active = null;
                    control = null;
                    started = false;
                }
            }
        }

        public void SearchClear() => Command(ClearCore);
        private void ClearCore()
        {
            tt.Clear(threads);
            threads.Clear();
            manager.Clear();
            InitTablebases(options["SyzygyPath"].AsString());
        }

        private void InitTablebases(string paths)
        {
            tablebases.Init(paths);
            if (tablebases.Enabled && paths.Split(new[] { System.IO.Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries).Length != 0)
                updates.Value.OnInfoString?.Invoke(tablebases.Info);
        }

        public void ResizeThreads() => Command(() => ResizeThreadsCore(options["Threads"].AsInt()));
        private void ResizeThreadsCore(int count)
        {
            if (network != null) ConfigureThreads(count);
            tt.Resize(new UIntPtr((uint)options["Hash"].AsInt()), threads);
        }

        public void SetTtSize(UIntPtr mib) => Command(() => tt.Resize(mib, threads));
        public int GetHashfull(int maxAge = 0) => Read(() => tt.Hashfull(maxAge));
        public void VerifyNetwork() => Command(VerifyCore);
        public void LoadNetwork(string path) => Command(() => options["EvalFile"].Assign(path));
        private void LoadCore(string path, bool reportVerification = true)
        {
            string requested = string.IsNullOrEmpty(path) ? EvalFile.DefaultName : path;
            // Network::load retains an already loaded path, even when the file has changed.
            // Its caller still clears worker histories/caches and manager state.
            if (network != null && networkFile.Current == requested)
            {
                threads.Clear();
                manager.Clear();
                return;
            }

            // A failed/truncated load never corrupts weights used by the previous worker.
            bool bootstrap = threads.Empty;
            var file = new EvalFile();
            using var candidate = LoadSharedCandidate(requested, file);
            try { candidate.Verify(reportVerification ? verify : null, file, path); }
            catch (NetworkVerificationException error) when (!reportVerification)
            {
                // Defer successful announcements only; retain the full failure diagnostic.
                verify?.Invoke(error.Message);
                throw;
            }
            // Retire borrowers before releasing any old network or its caches.
            threads.Set(new SharedState(options, threads, tt, candidate, tablebases), 0, updates, manager);
            InstallNetworkReplicas(candidate);
            networkFile = file;
            ConfigureThreads(options["Threads"].AsInt());
            if (bootstrap) PlaceBootstrappedHash();
            manager.Clear();
        }

        private void VerifyCore()
        {
            string requested = options["EvalFile"].AsString();
            if (network == null)
                LoadCore(requested);
            network!.Verify(verify, networkFile, requested);
            if (verify != null && networkReplicas != null)
            {
                var statuses = networkReplicas.GetStatusAndErrors(n => (n.AllocationStatus, n.SharedError));
                for (int i = 0; i < statuses.Length; ++i)
                    verify(FormatNetworkReplica(i, statuses[i].Status, statuses[i].Error));
            }
        }

        internal static string FormatNetworkReplica(int index, SystemWideSharedConstantAllocationStatus status, string? error)
        {
            string allocation = status switch
            {
                SystemWideSharedConstantAllocationStatus.NoAllocation => "No allocation.",
                SystemWideSharedConstantAllocationStatus.LocalMemory => "Local memory.",
                SystemWideSharedConstantAllocationStatus.SharedMemory => "Shared memory.",
                _ => "Unknown status."
            };
            return "Network replica " + (index + 1).ToString(CultureInfo.InvariantCulture) + ": " + allocation
                + (error == null ? "" : " " + error);
        }

        private void InstallNetworkReplicas(Network source)
        {
            var staged = new LazyNumaReplicatedSystemWide<Network>(numaContext, source, (n, domain) => n.CreateShared(domain), n => new Network(n));
            var previous = networkReplicas; networkReplicas = staged; network = staged.Primary; previous?.Dispose();
        }
        private void ConfigureThreads(int count)
        {
            threads.Set(new SharedState(options, threads, tt, network!, tablebases, numaContext.Config, systemWideReplicas: networkReplicas), count, updates, manager);
            threads.EnsureNetworkReplicated();
        }
        public bool SetNumaConfigFromOption(string policy) => Command(() =>
        {
            options["NumaPolicy"].Assign(policy); return lastNumaChange;
        });
        private bool SetNumaConfigCore(string policy)
        {
            NumaConfig? config = policy == "auto" || policy == "system" ? NumaConfig.FromSystem(NumaAutoPolicy.BundledL3(32))
                : policy == "hardware" ? NumaConfig.FromSystem(NumaAutoPolicy.BundledL3(32), false)
                : policy == "none" ? new NumaConfig() : NumaConfig.FromString(policy);
            if (config == null) return false;
            // No worker keeps a stale network/cache/history reference across reconfiguration.
            if (!threads.Empty) threads.Set(new SharedState(options, threads, tt, network!, tablebases), 0, updates, manager);
            numaContext.SetNumaConfig(config);
            if (networkReplicas != null) network = networkReplicas.Primary;
            ResizeThreadsCore(options["Threads"].AsInt()); return true;
        }
        public string GetNumaConfigAsString() => Read(() => numaContext.Config.ToString());
        public string NumaConfigInformationAsString() => Read(NumaConfigInformationAsStringCore);
        private string NumaConfigInformationAsStringCore() => "Available processors: " + numaContext.Config;
        public (int Current, int Total)[] GetBoundThreadCountByNumaNode() => Read(BoundThreadCountsCore);
        private (int Current, int Total)[] BoundThreadCountsCore()
        {
            int[] counts = threads.GetBoundThreadCountByNumaNode();
            if (counts.Length == 0) return Array.Empty<(int, int)>();
            return Enumerable.Range(0, numaContext.Config.NodeCount).Select(n => (n < counts.Length ? counts[n] : 0, numaContext.Config.CpuCountInNode(n))).ToArray();
        }
        public string ThreadBindingInformationAsString() => Read(ThreadBindingInformationAsStringCore);
        private string ThreadBindingInformationAsStringCore() => string.Join(":", BoundThreadCountsCore().Select(p => p.Current + "/" + p.Total));
        public string ThreadAllocationInformationAsString() => Read(ThreadAllocationInformationAsStringCore);
        private string ThreadAllocationInformationAsStringCore()
        {
            string binding = ThreadBindingInformationAsStringCore(); int count = threads.Count;
            return "Using " + count + (count > 1 ? " threads" : " thread") + (binding.Length == 0 ? "" : " with NUMA node thread binding: " + binding);
        }
        public string NetworkReplicationInformation => Read(NetworkReplicationInformationCore);
        private string NetworkReplicationInformationCore()
        {
            if (networkReplicas == null) return "NNUE not loaded";
            var statuses = networkReplicas.GetStatusAndErrors(n => (n.AllocationStatus, n.SharedError));
            return "NNUE replicas on logical nodes: " + string.Join(",", networkReplicas.MaterializedNodes) + "; " +
                string.Join("; ", statuses.Select((s, n) => n + ": " + s.Status + (s.Error == null ? "" : " (" + s.Error + ")")));
        }
        private Network LoadSharedCandidate(string requested, EvalFile file)
        {
            string domain = LazyNumaReplicatedSystemWide<Network>.PhysicalDiscriminator(numaContext.Config, 0);
            if (requested == EvalFile.DefaultName)
            {
                using var embedded = new Network { DefaultDirectory = network?.DefaultDirectory, EmbeddedNetwork = network?.EmbeddedNetwork ?? (() => EmbeddedNnue.OpenDefault() ?? System.IO.Stream.Null) };
                embedded.LoadInternal(file);
                if (file.Current == requested) return embedded.CreateShared(domain);
            }
            foreach (string? dir in new[] { "", directory, network?.DefaultDirectory })
            {
                if (dir == null) continue;
                try
                {
                    var loaded = Network.LoadSharedFile(System.IO.Path.Combine(dir, requested), domain, out string description);
                    file.Current = requested; file.NetDescription = description;
                    loaded.DefaultDirectory = network?.DefaultDirectory; loaded.EmbeddedNetwork = network?.EmbeddedNetwork ?? (() => EmbeddedNnue.OpenDefault() ?? System.IO.Stream.Null);
                    return loaded;
                }
                catch (System.IO.IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            // Preserve the optional embedding service and original verification failure contract.
            var candidate = new Network { DefaultDirectory = network?.DefaultDirectory, EmbeddedNetwork = network?.EmbeddedNetwork ?? (() => EmbeddedNnue.OpenDefault() ?? System.IO.Stream.Null) };
            candidate.Load(directory, requested, file); return candidate;
        }
        private void PlaceBootstrappedHash()
        {
            // Lazy loading allocates a bootstrap table before any bound worker exists.
            // Give the real bank its first touch on the configured pool; preserve SetTtSize's actual capacity.
            tt.Resize(new UIntPtr(tt.ClusterCount.ToUInt64() / 32768), threads);
        }
        private void EnsureWorkers()
        {
            if (threads.Empty) { ConfigureThreads(options["Threads"].AsInt()); PlaceBootstrappedHash(); }
        }
        public bool SaveNetwork(string? path) => Command(() =>
        {
            VerifyCore();
            return network!.Save(networkFile, path, verify);
        });
        public string TraceEval() => Command(() =>
        {
            VerifyCore();
            return Evaluator.Trace(position, network!);
        });
        public PerftResult Perft(string fen, Depth depth, bool isChess960) => Command(() => Diagnostics.Perft.Run(fen, depth, isChess960));
        public PerftResult Perft(Depth depth) => Command(() => Diagnostics.Perft.Run(position, depth));
        private T Read<T>(Func<T> action)
        {
            EnsureHost();
            lock (commands)
            {
                ThrowIfDisposed();
                return action();
            }
        }

        private T Command<T>(Func<T> action)
        {
            EnsureHost();
            lock (commands)
            {
                ThrowIfDisposed();
                Join(true);
                return action();
            }
        }

        private void Command(Action action) => Command(() =>
        {
            action();
            return 0;
        });
        private void EnsureHost()
        {
            if (Monitor.IsEntered(commands) || threads.IsWorkerThread)
                throw new InvalidOperationException("Schedule engine commands outside engine callbacks; Stop/PonderHit are permitted.");
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Engine));
        }

        public void Dispose()
        {
            EnsureHost();
            lock (commands)
            {
                if (disposed)
                    return;
                try
                {
                    Join(true);
                }
                finally
                {
                    threads.Dispose();
                    tt.Dispose();
                    tablebases.Dispose();
                    protocolLog.Dispose();
                    networkReplicas?.Dispose();
                    networkReplicas = null;
                    numaContext.Dispose();
                    network = null;
                    disposed = true;
                }
            }
        }
    }
}
