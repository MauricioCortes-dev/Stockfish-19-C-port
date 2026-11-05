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
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Read-only, demand-paged table file. Its owner excludes disposal during reads.</summary>
    internal sealed class TBFile : IDisposable
    {
        private readonly MemoryMappedFile mapping;
        private readonly MemoryMappedViewAccessor view;
        internal long Length { get; }
        internal string Name { get; }

        private TBFile(string name, FileStream stream, TBType type)
        {
            Name = name;
            Length = stream.Length;
            if (Length % 64 != 16) throw Corrupt("file size");
            mapping = MemoryMappedFile.CreateFromFile(stream, null, 0, MemoryMappedFileAccess.Read,
                HandleInheritability.None, true);
            try
            {
                view = mapping.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                uint magic = type == TBType.WDL ? 0x5D23E871U : 0xA50C66D7U;
                if (U32(0) != magic) throw Corrupt("magic");
            }
            catch { view?.Dispose(); mapping.Dispose(); throw; }
        }

        internal static string? Find(string[] paths, string name)
        {
            foreach (string path in paths)
            {
                string full = Path.Combine(path, name);
                try { using (FileStream stream = File.OpenRead(full)) return full; }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return null;
        }

        internal static TBFile? Open(string[] paths, string name, TBType type)
        {
            string? full = Find(paths, name);
            if (full == null) return null;
            FileStream stream;
            try { stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            using (stream) return new TBFile(full, stream, type);
        }

        // Division avoids overflowing count * stride. Offsets are relative to the file.
        internal static bool Fits(long offset, ulong count, ulong stride, long end) =>
            offset >= 0 && offset <= end && (count == 0 || stride <= (ulong)(end - offset) / count);
        internal void Require(long offset, ulong count, ulong stride = 1)
        {
            if (!Fits(offset, count, stride, Length)) throw Corrupt("out of bounds");
        }
        internal byte U8(long offset) { Require(offset, 1); return view.ReadByte(offset); }
        internal ushort U16(long offset) => SyzygyBinary.Number16(this, offset, true);
        internal uint U32(long offset) => SyzygyBinary.Number32(this, offset, true);
        internal ulong U64BE(long offset) => SyzygyBinary.Number64(this, offset, false);
        internal uint U32BE(long offset) => SyzygyBinary.Number32(this, offset, false);
        internal InvalidDataException Corrupt(string reason) => new InvalidDataException("Corrupt Syzygy table " + Name + ": " + reason);
        public void Dispose() { view.Dispose(); mapping.Dispose(); }
    }
}
