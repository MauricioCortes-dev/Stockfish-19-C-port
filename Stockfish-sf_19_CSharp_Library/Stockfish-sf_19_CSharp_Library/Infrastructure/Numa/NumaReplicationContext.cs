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
namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Numa
{
    /// <summary>Owns topology and registrations. Reconfiguration stages all copies before publishing any change.</summary>
    /// <remarks>The host must quiesce users of borrowed replicas before mutation/reconfiguration.
    /// Accessors synchronize publication; they do not make arbitrary mutation of T safe.</remarks>
    public sealed class NumaReplicationContext : IDisposable
    {
        internal readonly object Gate = new object();
        private readonly HashSet<NumaReplicatedBase> objects = new HashSet<NumaReplicatedBase>();
        private NumaConfig config;
        private bool disposed;
        public NumaReplicationContext(NumaConfig config) { this.config = config ?? throw new ArgumentNullException(nameof(config)); }
        public NumaConfig Config { get { lock (Gate) { Check(); return config; } } }
        internal void Attach(NumaReplicatedBase value) { lock (Gate) { Check(); objects.Add(value); } }
        internal void Detach(NumaReplicatedBase value) { lock (Gate) objects.Remove(value); }
        private void Check() { if (disposed) throw new ObjectDisposedException(nameof(NumaReplicationContext)); }
        public void SetNumaConfig(NumaConfig next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            lock (Gate)
            {
                Check(); var commits = new List<Action>();
                try { foreach (var obj in objects) commits.Add(obj.PrepareConfig(next)); }
                catch { foreach (var obj in objects) obj.CancelPreparedConfig(); throw; }
                config = next;
                foreach (var commit in commits) commit();
            }
        }
        public void Dispose()
        {
            lock (Gate)
            {
                if (objects.Count != 0) throw new InvalidOperationException("Dispose replicated objects before their context.");
                disposed = true;
            }
        }
    }
}
