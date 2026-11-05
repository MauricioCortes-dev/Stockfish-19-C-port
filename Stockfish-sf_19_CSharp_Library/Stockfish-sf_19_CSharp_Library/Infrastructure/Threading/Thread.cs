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
using System.Runtime.ExceptionServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Stockfish_sf_19_CSharp_Library.Search;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Threading
{
    /// <summary>Persistent worker thread with one job slot, native idle-loop semantics and synchronous retirement.</summary>
    /// <remarks>Jobs run outside the monitor. Wait observes the last job's exception until another job is accepted.
    /// Blocking operations from this thread are rejected. Dispose joins even after a job fault.</remarks>
    public sealed class Thread : IDisposable
    {
        private readonly object gate = new object(), disposal = new object();
        private readonly NativeThread native;
        private Action? job;
        private ExceptionDispatchInfo? failure;
        private bool searching = true, exit, closing, disposed;
        private Worker? worker;
        private ISearchManager? manager;
        public int Id { get; }
        public int ManagedThreadId => native.ManagedThreadId;
        public ISearchManager Manager { get { lock (gate) { ThrowIfClosing(); return manager!; } } }
        public Worker Worker { get { lock (gate) { ThrowIfClosing(); return worker!; } } }
        public bool IsBusy { get { lock (gate) return searching; } }
        public bool IsAlive => native.IsAlive;

        public Thread(int id, Func<Worker> factory, ISearchManager? manager = null)
            : this(id, factory, manager, NativeThread.Create) { }

        internal Thread(int id, Func<Worker> factory, ISearchManager? manager,
            Func<NativeThreadOptions, Action, string?, NativeThread> createThread)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            Id = id;
            this.manager = manager ?? new NullSearchManager();
            // Same requested stack as NativeThreadOptions.largeStack / TH_STACK_SIZE.
            native = createThread(new NativeThreadOptions().SetLargeStack(true), IdleLoop, "Stockfish worker " + id);
            native.RequireStarted();
            try
            {
                WaitForSearchFinished();
                RunCustomJob(() => worker = factory() ?? throw new InvalidOperationException("Worker factory returned null."));
                WaitForSearchFinished();
            }
            catch { Dispose(); throw; }
        }

        public void RunCustomJob(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            EnsureExternal();
            lock (gate)
            {
                while (searching && !closing) Monitor.Wait(gate);
                ThrowIfClosing();
                failure = null;
                job = action;
                searching = true;
                Monitor.PulseAll(gate);
            }
        }

        /// <summary>Launch the prepared search on this dedicated thread. Observe failures through the returned task.</summary>
        public Task<SearchResult> StartSearching()
        {
            var completion = new TaskCompletionSource<SearchResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            RunCustomJob(() =>
            {
                try { completion.SetResult(worker!.StartPreparedSearch()); }
                catch (Exception error)
                {
                    worker!.Control.RequestStop();
                    completion.SetException(error);
                }
            });
            return completion.Task;
        }

        public void EnsureNetworkReplicated() => RunCustomJob(() => worker!.EnsureNetworkReplicated());

        public void ClearWorker() => RunCustomJob(() => worker!.Clear());

        public void WaitForSearchFinished()
        {
            EnsureExternal();
            ExceptionDispatchInfo? error;
            lock (gate)
            {
                while (searching) Monitor.Wait(gate);
                error = failure;
            }
            error?.Throw();
        }

        private void IdleLoop()
        {
            lock (gate)
            {
                searching = false;
                Monitor.PulseAll(gate);
            }
            while (true)
            {
                lock (gate)
                {
                    while (!searching && !exit) Monitor.Wait(gate);
                    if (exit) return;
                }
                // The invocation frame is gone before idle is published. A parked loop must not
                // retain the last delegate in a JIT-live local, even if a null assignment is removed.
                ExceptionDispatchInfo? error = ExecuteJob();
                lock (gate)
                {
                    failure = error;
                    searching = false;
                    Monitor.PulseAll(gate);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ExceptionDispatchInfo? ExecuteJob()
        {
            Action? action;
            lock (gate) { action = job; job = null; }
            try { action!(); return null; }
            catch (Exception e) { return ExceptionDispatchInfo.Capture(e); }
        }

        private void EnsureExternal()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == native.ManagedThreadId)
                throw new InvalidOperationException("A worker cannot block on or retire itself.");
        }
        private void ThrowIfClosing() { if (closing) throw new ObjectDisposedException(nameof(Thread)); }

        public void Dispose()
        {
            EnsureExternal();
            lock (disposal)
            {
                lock (gate)
                {
                    if (disposed) return;
                    closing = true;
                    Monitor.PulseAll(gate);
                    while (searching) Monitor.Wait(gate);
                    exit = true;
                    Monitor.PulseAll(gate);
                }
                native.Join();
                lock (gate)
                {
                    worker = null;
                    manager = null;
                    disposed = true;
                }
            }
        }
    }
}
