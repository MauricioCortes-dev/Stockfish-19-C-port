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
using System.Reflection;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Runtime;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    public enum PrefetchRw { READ, WRITE }
    public enum PrefetchLoc { NONE, LOW, MODERATE, HIGH }
    /// <summary>Advisory hints only. Unsupported/disabled runtimes perform no memory access.
    /// The caller keeps the addressed storage alive; NONE still requests a non-temporal hint.</summary>
    public static unsafe class CachePrefetch
    {
        private unsafe delegate void Hint(void* address);
        private static readonly Hint?[] hints=Create();
        public static bool Enabled => hints[3]!=null;
        public static int GetIntelHint(PrefetchRw rw, PrefetchLoc loc, bool writeHintAvailable=false) =>
            rw==PrefetchRw.WRITE ? (writeHintAvailable?7:3) : loc==PrefetchLoc.NONE?0:loc==PrefetchLoc.LOW?1:loc==PrefetchLoc.MODERATE?2:3;
        public static void Prefetch(IntPtr address, PrefetchRw rw=PrefetchRw.READ, PrefetchLoc loc=PrefetchLoc.HIGH)
        {
            int index=GetIntelHint(rw,loc); // CLR Sse exposes read hints; WRITE uses the original T0 alternative.
            hints[index]?.Invoke((void*)address);
        }
        private static Hint?[] Create()
        {
            var result=new Hint?[4];
#if ENABLE_IL2CPP || STOCKFISH_AOT_SAFE
            return result;
#else
            if(Environment.GetEnvironmentVariable("STOCKFISH_NO_PREFETCH")=="1" || !RuntimeCapabilities.Supports("X86.Sse")) return result;
            var type=Type.GetType("System.Runtime.Intrinsics.X86.Sse, System.Private.CoreLib",false)
                ?? Type.GetType("System.Runtime.Intrinsics.X86.Sse, System.Runtime.Intrinsics",false);
            string[] names={"PrefetchNonTemporal","Prefetch2","Prefetch1","Prefetch0"};
            try
            {
                for(int i=0;i<result.Length;++i)
                {
                    var method=type?.GetMethod(names[i],BindingFlags.Public|BindingFlags.Static);
                    if(method!=null) result[i]=(Hint)method.CreateDelegate(typeof(Hint));
                }
            }
            catch(Exception e) when(e is ArgumentException || e is MemberAccessException || e is NotSupportedException)
            { return new Hint?[4]; }
            return result;
#endif
        }
    }
}
