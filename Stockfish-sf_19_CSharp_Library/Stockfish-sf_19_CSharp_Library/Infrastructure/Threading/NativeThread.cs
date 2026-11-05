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
using System.Threading;
using RuntimeThread = System.Threading.Thread;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Threading
{
    /// <summary>One joinable OS-backed CLR thread. Creation can fail without retaining its callable.</summary>
    /// <remarks>Assignment aliases this owner (CLR adaptation of move-only C++ ownership).
    /// Join/Dispose wait for completion and rethrow callback exceptions on the joining caller.
    /// Dispose must not run on this thread. Options request stack reservation, not committed memory.</remarks>
    public sealed class NativeThread : IDisposable
    {
        public const int LargeStackSize = 8 * 1024 * 1024;
        private sealed class Invocation
        {
            internal Action? Action;
            internal ExceptionDispatchInfo? Failure;
            internal void Run()
            {
                try { Action!(); }
                catch (Exception error) { Failure = ExceptionDispatchInfo.Capture(error); }
                finally { Action = null; }
            }
        }
        private readonly object gate = new object();
        private readonly Invocation invocation = new Invocation();
        private RuntimeThread? thread;
        private int managedThreadId;
        public Exception? CreationError { get; private set; }
        public bool Joinable { get { lock (gate) return thread != null; } }
        public bool IsAlive => thread?.IsAlive ?? false;
        public int ManagedThreadId => managedThreadId;
        public int RequestedStackSize { get; private set; }

        public NativeThread() { }
        public static NativeThread Create(NativeThreadOptions options, Action action, string? name = null)
            => Create(options, action, name, (start, stack) => stack == 0 ? new RuntimeThread(start) : new RuntimeThread(start, stack));

        /// <summary>Bind state by value before starting (the managed counterpart of decayed C++ arguments).</summary>
        public static NativeThread Create<T>(NativeThreadOptions options, Action<T> action, T state, string? name = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return Create(options, () => action(state), name);
        }

        internal static NativeThread Create(NativeThreadOptions options, Action action, string? name,
            Func<ThreadStart, int, RuntimeThread> factory)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var result = new NativeThread { RequestedStackSize = options.LargeStack ? LargeStackSize : 0 };
            result.invocation.Action = action;
            try
            {
                var native = factory(result.invocation.Run, result.RequestedStackSize);
                native.IsBackground = true;
                if (name != null) native.Name = name;
                native.Start();
                result.managedThreadId = native.ManagedThreadId;
                result.thread = native;
            }
            catch (Exception error) when (error is OutOfMemoryException || error is ThreadStateException)
            {
                result.invocation.Action = null;
                result.CreationError = error;
            }
            return result;
        }
        internal void RequireStarted()
        {
            if (CreationError != null) ExceptionDispatchInfo.Capture(CreationError).Throw();
            if (thread == null) throw new InvalidOperationException("Native thread was not started.");
        }
        public void Join()
        {
            RuntimeThread? target = Volatile.Read(ref thread);
            if (target != null && RuntimeThread.CurrentThread.ManagedThreadId == target.ManagedThreadId)
                throw new InvalidOperationException("A native thread cannot join itself.");
            lock (gate)
            {
                if (thread != null) { thread.Join(); thread = null; }
                invocation.Failure?.Throw();
            }
        }
        public void Dispose() => Join();
    }
}
