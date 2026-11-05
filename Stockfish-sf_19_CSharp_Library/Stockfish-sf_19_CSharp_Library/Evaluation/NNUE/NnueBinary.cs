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
using System.Runtime.InteropServices;
using System.Text;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Little endian integers and Stockfish's signed LEB128 blocks.
    /// Invalid/truncated input throws; destinations may contain a decoded prefix, as native streams do.</summary>
    public static class NnueBinary
    {
        internal static int Width<T>(bool signedOnly = false)
            where T : unmanaged
        {
            Type t = typeof(T);
            bool signed = t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(BiasType) || t == typeof(WeightType) || t == typeof(ThreatWeightType) || t == typeof(PSQTWeightType);
            bool supported = signed || t == typeof(byte) || t == typeof(ushort) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong) || t == typeof(IndexType) || t == typeof(TransformedFeatureType);
            if (!supported || (signedOnly && !signed))
                throw new ArgumentException("Unsupported NNUE integer type.");
            Span<T> one = stackalloc T[1];
            return MemoryMarshal.AsBytes(one).Length;
        }

        internal static void ReadExactly(Stream stream, Span<byte> bytes)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            while (!bytes.IsEmpty)
            {
                int count = stream.Read(bytes);
                if (count == 0)
                    throw new EndOfStreamException();
                bytes = bytes.Slice(count);
            }
        }

        public static T ReadLittleEndian<T>(Stream stream)
            where T : unmanaged
        {
            Width<T>();
            Span<T> v = stackalloc T[1];
            ReadLittleEndian(stream, v);
            return v[0];
        }

        public static void ReadLittleEndian<T>(Stream stream, Span<T> values)
            where T : unmanaged
        {
            int width = Width<T>();
            Span<byte> bytes = MemoryMarshal.AsBytes(values);
            ReadExactly(stream, bytes);
            if (!BitConverter.IsLittleEndian)
                for (int i = 0; i < bytes.Length; i += width)
                    bytes.Slice(i, width).Reverse();
        }

        public static void WriteLittleEndian<T>(Stream stream, T value)
            where T : unmanaged
        {
            Width<T>();
            Span<T> v = stackalloc T[1];
            v[0] = value;
            WriteLittleEndian<T>(stream, v);
        }

        public static void WriteLittleEndian<T>(Stream stream, ReadOnlySpan<T> values)
            where T : unmanaged
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            int width = Width<T>();
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(values);
            if (BitConverter.IsLittleEndian)
                stream.Write(bytes);
            else
            {
                Span<byte> one = stackalloc byte[8];
                for (int i = 0; i < bytes.Length; i += width)
                {
                    bytes.Slice(i, width).CopyTo(one);
                    one.Slice(0, width).Reverse();
                    stream.Write(one.Slice(0, width));
                }
            }
        }

        public static void WriteLeb128<T>(Stream stream, ReadOnlySpan<T> values)
            where T : unmanaged
        {
            int width = Width<T>();
            if (typeof(T) != typeof(long))
                Width<T>(true);
            uint count = 0;
            foreach (T item in values)
            {
                long value = Signed(item, width);
                byte b;
                do
                {
                    b = (byte)(value & 127);
                    value >>= 7;
                    count = unchecked(count + 1);
                }
                while ((b & 64) == 0 ? value != 0 : value != -1);
            }

            stream.Write(Encoding.ASCII.GetBytes(NnueConstants.Leb128MagicString));
            WriteLittleEndian(stream, count);
            byte[] buffer = new byte[4096];
            int used = 0;
            foreach (T item in values)
            {
                long value = Signed(item, width);
                while (true)
                {
                    byte b = (byte)(value & 127);
                    value >>= 7;
                    bool last = (b & 64) == 0 ? value == 0 : value == -1;
                    buffer[used++] = last ? b : (byte)(b | 128);
                    if (used == buffer.Length)
                    {
                        stream.Write(buffer, 0, used);
                        used = 0;
                    }

                    if (last)
                        break;
                }
            }

            if (used != 0)
                stream.Write(buffer, 0, used);
        }

        private static long Signed<T>(T item, int width)
            where T : unmanaged
        {
            Span<T> v = stackalloc T[1];
            v[0] = item;
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(v);
            return width == 1 ? unchecked((sbyte)bytes[0]) : width == 2 ? MemoryMarshal.Read<short>(bytes) : width == 4 ? MemoryMarshal.Read<int>(bytes) : MemoryMarshal.Read<long>(bytes);
        }

        public static void ReadLeb128<T>(Stream stream, Span<T> values)
            where T : unmanaged
        {
            var reader = new Leb128Reader(stream);
            reader.Read(values);
            reader.Complete();
        }

        /// <summary>One compressed block may contain several arrays, including different signed widths.
        /// This reader owns only an 8192-byte read-ahead buffer; it does not own the stream.</summary>
        public sealed class Leb128Reader
        {
            private readonly Stream stream;
            private readonly byte[] buffer = new byte[8192];
            private uint bytesLeft;
            private int pos, end;
            public Leb128Reader(Stream stream)
            {
                this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
                byte[] magic = new byte[NnueConstants.Leb128MagicString.Length];
                ReadExactly(stream, magic);
                if (Encoding.ASCII.GetString(magic) != NnueConstants.Leb128MagicString)
                    throw new InvalidDataException("Invalid LEB128 magic.");
                bytesLeft = ReadLittleEndian<uint>(stream);
            }

            public void Read<T>(Span<T> output)
                where T : unmanaged
            {
                int width = Width<T>(true);
                uint result = 0;
                ulong shift = 0;
                for (int i = 0; i < output.Length;)
                {
                    if (pos == end)
                    {
                        int want = (int)Math.Min(bytesLeft, (uint)buffer.Length);
                        end = 0;
                        pos = 0;
                        while (end < want)
                        {
                            int n = stream.Read(buffer, end, want - end);
                            if (n == 0)
                                break;
                            end += n;
                        }

                        if (end == 0)
                            throw new EndOfStreamException();
                    }

                    byte b = buffer[pos++];
                    --bytesLeft;
                    result |= (uint)(b & 127) << (int)(shift % 32);
                    shift += 7;
                    if ((b & 128) == 0)
                    {
                        uint v = shift >= 32 || (b & 64) == 0 ? result : result | ~((1U << (int)shift) - 1);
                        Span<byte> raw = MemoryMarshal.AsBytes(output.Slice(i++, 1));
                        if (width == 1)
                            raw[0] = unchecked((byte)v);
                        else if (width == 2)
                        {
                            short s = unchecked((short)v);
                            MemoryMarshal.Write(raw, ref s);
                        }
                        else
                        {
                            int s = unchecked((int)v);
                            MemoryMarshal.Write(raw, ref s);
                        }

                        result = 0;
                        shift = 0;
                    }
                }
            }

            public void Complete()
            {
                if (bytesLeft != 0)
                    throw new InvalidDataException("Unconsumed LEB128 payload.");
            }
        }
    }
}
