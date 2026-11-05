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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>One material table and its lazily published, immutable metadata.</summary>
    internal sealed class TBTable : IDisposable
    {
        internal readonly TBType Type;
        internal readonly string Code;
        internal readonly Key Key, Key2;
        internal readonly int PieceCount;
        internal readonly bool HasPawns, HasUniquePieces;
        internal readonly byte[] PawnCount = new byte[2];
        private readonly object loadGate = new object();
        private Loaded? loaded;
        internal int Sides => Type == TBType.WDL ? 2 : 1;

        internal TBTable(string code)
        {
            Type = TBType.WDL; Code = code;
            var pos = new Position(); var state = new StateInfo();
            // Like C++, material-code setup can report a king attacked after fully setting state.
            pos.Set(code, Color.WHITE, state);
            Key = pos.MaterialKey(); PieceCount = pos.Count(PieceType.ALL_PIECES);
            HasPawns = pos.Pieces(PieceType.PAWN) != 0;
            for (Color c = Color.WHITE; c <= Color.BLACK; ++c)
                for (PieceType pt = PieceType.PAWN; pt < PieceType.KING; ++pt)
                    if (pos.Count(pt, c) == 1) HasUniquePieces = true;
            bool white = pos.Count(PieceType.PAWN, Color.BLACK) == 0
                || (pos.Count(PieceType.PAWN, Color.WHITE) != 0 && pos.Count(PieceType.PAWN, Color.BLACK) >= pos.Count(PieceType.PAWN, Color.WHITE));
            PawnCount[0] = (byte)pos.Count(PieceType.PAWN, white ? Color.WHITE : Color.BLACK);
            PawnCount[1] = (byte)pos.Count(PieceType.PAWN, white ? Color.BLACK : Color.WHITE);
            pos.Set(code, Color.BLACK, state); Key2 = pos.MaterialKey();
        }
        internal TBTable(TBTable wdl)
        {
            Type = TBType.DTZ; Code = wdl.Code; Key = wdl.Key; Key2 = wdl.Key2;
            PieceCount = wdl.PieceCount; HasPawns = wdl.HasPawns; HasUniquePieces = wdl.HasUniquePieces;
            Array.Copy(wdl.PawnCount, PawnCount, 2);
        }
        internal sealed class Loaded
        {
            internal readonly TBFile? File;
            internal readonly PairsData[,] Items;
            internal readonly ExceptionDispatchInfo? Error;
            internal readonly long Map;
            internal Loaded(TBFile? file, PairsData[,] items, ExceptionDispatchInfo? error = null, long map = 0) { File = file; Items = items; Error = error; Map = map; }
        }
        internal Loaded Map(string[] paths, SyzygyIndex index)
        {
            Loaded? result = Volatile.Read(ref loaded);
            if (result == null)
                lock (loadGate)
                {
                    result = loaded;
                    if (result == null)
                    {
                        TBFile? file = null;
                        try
                        {
                            file = TBFile.Open(paths, Code + (Type == TBType.WDL ? ".rtbw" : ".rtbz"), Type);
                            var items = new PairsData[Sides, 4];
                            long map = file != null ? SyzygyDecoder.Set(this, items, file, index) : 0;
                            result = new Loaded(file, items, map: map);
                        }
                        catch (Exception e) when (e is System.IO.InvalidDataException || e is OverflowException)
                        {
                            file?.Dispose();
                            Exception error = e is OverflowException
                                ? new System.IO.InvalidDataException("Corrupt Syzygy table " + Code + ": numeric size overflow", e) : e;
                            result = new Loaded(null, new PairsData[Sides, 4], ExceptionDispatchInfo.Capture(error));
                        }
                        catch { file?.Dispose(); throw; }
                        // Release publication of all arrays and file ownership, including cached absence/error.
                        Volatile.Write(ref loaded, result);
                    }
                }
            result.Error?.Throw();
            return result;
        }
        // Owner's write lock guarantees there are no readers here.
        public void Dispose() => loaded?.File?.Dispose();
    }
}
