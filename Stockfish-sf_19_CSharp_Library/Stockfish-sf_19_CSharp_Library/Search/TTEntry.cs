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
using System.Runtime.InteropServices;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>Ten inline bytes; each access is atomic, Read/Save/CopyFrom as a whole are not.</summary>
    /// <remarks>SF19-000673..000675/000911/000912. Concurrent use requires a mutable field/ref.
    /// Ordinary struct copies and boxing require quiescence; use the ref copy APIs otherwise.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct TTEntry
    {
        private RelaxedAtomic<ushort, AtomicUInt16> key16;
        private RelaxedAtomic<byte, AtomicUInt8> depth8;
        private RelaxedAtomic<byte, AtomicUInt8> genBound8;
        private RelaxedAtomic<Move, AtomicMove> move16;
        private RelaxedAtomic<short, AtomicInt16> value16;
        private RelaxedAtomic<short, AtomicInt16> eval16;

        public TTEntry(ref TTEntry source) { this = default; CopyFrom(ref source); }

        public TTData Read()
        {
            // Preserve original evaluation order, including two independent packed-byte loads.
            return new TTData(move16.Load(), value16.Load(), eval16.Load(),
                Depth.DEPTH_NONE + depth8.Load(),
                (genBound8.Load() & TTConstants.BOUND_MASK) >> TTConstants.BOUND_SHIFT,
                (genBound8.Load() & TTConstants.PV_MASK) != 0);
        }

        public bool IsOccupied() => depth8.Load() != 0;

        // Live field loads for the table's key scan and replacement policy.
        internal ushort ReadKey16() => key16.Load();
        internal byte ReadDepth8() => depth8.Load();
        internal void Penalize(int penalty)
            => depth8.Store(unchecked((byte)Math.Max(checked(depth8.Load() - penalty), 0)));

        public void Save(Key k, Value v, bool pv, Bound b, Depth d, Move m, Value ev, byte currGeneration)
        {
            if ((bool)m || unchecked((ushort)(ulong)k) != key16.Load())
                move16.Store(m);

            // Do not cache loads or eagerly validate arguments: both alter observable races/short-circuiting.
            if (b == Bound.BOUND_EXACT || unchecked((ushort)(ulong)k) != key16.Load()
                || checked((int)d - (int)Depth.DEPTH_NONE + 2 * (pv ? 1 : 0)) > depth8.Load() - 4
                || RelativeAge(currGeneration) != 0)
            {
                int encoded = checked((int)d - (int)Depth.DEPTH_NONE);
                // Managed guards adapt assertions on this write path, after the early move store.
                if (encoded <= 0 || encoded >= 256) throw new ArgumentOutOfRangeException(nameof(d));
                if (currGeneration > TTConstants.GENERATION_MASK) throw new ArgumentOutOfRangeException(nameof(currGeneration));
                key16.Store(unchecked((ushort)(ulong)k));
                depth8.Store((byte)encoded);
                genBound8.Store((byte)(currGeneration | ((int)b << TTConstants.BOUND_SHIFT) | ((pv ? 1 : 0) << TTConstants.PV_SHIFT)));
                value16.Store(unchecked((short)(int)v));
                eval16.Store(unchecked((short)(int)ev));
            }
            else if (depth8.Load() + (int)Depth.DEPTH_NONE >= 5
                && ((genBound8.Load() & TTConstants.BOUND_MASK) >> TTConstants.BOUND_SHIFT) != (int)Bound.BOUND_EXACT)
            {
                short snapshot = value16.Load();
                if (Math.Abs((int)snapshot) < (int)Value.VALUE_INFINITE && ((Value)snapshot).IsDecisive())
                    depth8.Store((byte)Math.Max(depth8.Load() - 1, 0));
            }
        }

        public byte RelativeAge(byte currGeneration)
            => (byte)((currGeneration - genBound8.Load()) & TTConstants.GENERATION_MASK);

        public void CopyFrom(ref TTEntry source)
        {
            key16.CopyFrom(ref source.key16);
            depth8.CopyFrom(ref source.depth8);
            genBound8.CopyFrom(ref source.genBound8);
            move16.CopyFrom(ref source.move16);
            value16.CopyFrom(ref source.value16);
            eval16.CopyFrom(ref source.eval16);
        }
    }
}
