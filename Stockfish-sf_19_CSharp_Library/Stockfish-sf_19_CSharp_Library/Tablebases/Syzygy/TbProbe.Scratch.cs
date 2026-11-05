using System;
using System.Collections.Generic;
using System.Threading;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    public sealed partial class TbProbe
    {
        // One bank per service/thread; simultaneous queries never borrow the same slot.
        // Dispose is protected by lifetime's writer lock, after all public queries drain.
        private readonly ThreadLocal<ProbeScratchBank> queryScratch =
            new ThreadLocal<ProbeScratchBank>(() => new ProbeScratchBank());

        internal sealed class ProbeScratch
        {
            internal readonly StateInfo State = new StateInfo();
            internal readonly Move[] Moves = new Move[ChessConstants.MAX_MOVES];
            internal readonly Move[] Responses = new Move[ChessConstants.MAX_MOVES];
            internal readonly List<Move> Played = new List<Move>();
            internal void Release()
            {
                // Only after UndoMove: never retain a caller's historical state chain.
                State.previous = null;
                Played.Clear();
            }
        }

        internal sealed class ProbeScratchBank
        {
            private readonly List<ProbeScratch> slots = new List<ProbeScratch>();
            internal int Depth { get; private set; }
            internal ProbeScratch Acquire()
            {
                if (Depth == slots.Count) slots.Add(new ProbeScratch());
                return slots[Depth++];
            }
            internal void Release()
            {
                if (Depth == 0) throw new InvalidOperationException("No active Syzygy scratch slot.");
                slots[--Depth].Release();
            }
        }
    }
}
