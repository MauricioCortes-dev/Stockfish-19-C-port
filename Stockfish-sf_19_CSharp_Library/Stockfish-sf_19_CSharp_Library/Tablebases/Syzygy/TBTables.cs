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
using Stockfish_sf_19_CSharp_Library.Types;
namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Material-key index with the original Robin Hood insertion and ownership rules.</summary>
    internal sealed class TBTables : IDisposable
    {
        internal struct Entry
        {
            internal Key Key;
            internal TBTable? Wdl, Dtz;
            internal TBTable? Get(TBType type) => type == TBType.WDL ? Wdl : Dtz;
        }
        private const int Size = 4096;
        private readonly Entry[] hashTable = new Entry[Size + 1];
        private readonly List<TBTable> wdlTable = new List<TBTable>(), dtzTable = new List<TBTable>();
        internal int FoundWdlFiles { get; private set; }
        internal int FoundDtzFiles { get; private set; }
        internal int MaxCardinality { get; private set; }
        private void Insert(Key key, TBTable wdl, TBTable dtz)
        {
            int home = (int)((ulong)key & (Size - 1));
            var entry = new Entry { Key = key, Wdl = wdl, Dtz = dtz };
            for (int bucket = home; bucket < Size; ++bucket)
            {
                Key otherKey = hashTable[bucket].Key;
                if (otherKey == key || hashTable[bucket].Wdl == null) { hashTable[bucket] = entry; return; }
                int otherHome = (int)((ulong)otherKey & (Size - 1));
                if (otherHome > home)
                {
                    Entry displaced = hashTable[bucket]; hashTable[bucket] = entry; entry = displaced;
                    home = otherHome; key = otherKey;
                }
            }
            throw new InvalidOperationException("TB hash table size too low!");
        }
        internal TBTable? Get(Key key, TBType type = default)
        {
            for (int bucket = (int)((ulong)key & (Size - 1)); ; ++bucket)
                if (hashTable[bucket].Key == key || hashTable[bucket].Get(type) == null) return hashTable[bucket].Get(type);
        }
        internal void Add(string[] paths, params PieceType[] pieces)
        {
            const string chars = " PNBRQK";
            string code = "";
            foreach (PieceType pt in pieces) code += chars[pt];
            code = code.Insert(code.IndexOf('K', 1), "v");
            if (TBFile.Find(paths, code + ".rtbz") != null) ++FoundDtzFiles;
            if (TBFile.Find(paths, code + ".rtbw") == null) return;
            ++FoundWdlFiles; MaxCardinality = Math.Max(pieces.Length, MaxCardinality);
            var wdl = new TBTable(code); var dtz = new TBTable(wdl);
            wdlTable.Add(wdl); dtzTable.Add(dtz);
            Insert(wdl.Key, wdl, dtz); Insert(wdl.Key2, wdl, dtz);
        }
        internal void Initialize(string[] paths)
        {
            if (paths.Length == 0) return;
            var k = PieceType.KING;
            for (PieceType p1 = PieceType.PAWN; p1 < k; ++p1)
            {
                Add(paths, k, p1, k);
                for (PieceType p2 = PieceType.PAWN; p2 <= p1; ++p2)
                {
                    Add(paths, k, p1, p2, k); Add(paths, k, p1, k, p2);
                    for (PieceType p3 = PieceType.PAWN; p3 < k; ++p3) Add(paths, k, p1, p2, k, p3);
                    for (PieceType p3 = PieceType.PAWN; p3 <= p2; ++p3)
                    {
                        Add(paths, k, p1, p2, p3, k);
                        for (PieceType p4 = PieceType.PAWN; p4 <= p3; ++p4)
                        {
                            Add(paths, k, p1, p2, p3, p4, k);
                            for (PieceType p5 = PieceType.PAWN; p5 <= p4; ++p5) Add(paths, k, p1, p2, p3, p4, p5, k);
                            for (PieceType p5 = PieceType.PAWN; p5 < k; ++p5) Add(paths, k, p1, p2, p3, p4, k, p5);
                        }
                        for (PieceType p4 = PieceType.PAWN; p4 < k; ++p4)
                        {
                            Add(paths, k, p1, p2, p3, k, p4);
                            for (PieceType p5 = PieceType.PAWN; p5 <= p4; ++p5) Add(paths, k, p1, p2, p3, k, p4, p5);
                        }
                    }
                    for (PieceType p3 = PieceType.PAWN; p3 <= p1; ++p3)
                        for (PieceType p4 = PieceType.PAWN; p4 <= (p1 == p3 ? p2 : p3); ++p4) Add(paths, k, p1, p2, k, p3, p4);
                }
            }
        }
        internal string Info() => "Found " + FoundWdlFiles + " WDL and " + FoundDtzFiles + " DTZ tablebase files (up to " + MaxCardinality + "-man).";
        internal void Clear()
        {
            foreach (TBTable table in wdlTable) table.Dispose();
            foreach (TBTable table in dtzTable) table.Dispose();
            wdlTable.Clear(); dtzTable.Clear(); Array.Clear(hashTable, 0, hashTable.Length);
            MaxCardinality = FoundDtzFiles = FoundWdlFiles = 0;
        }
        public void Dispose() => Clear();
    }
}
