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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    public static class MiscUtilities
    {
        public static bool IsAsciiWhitespace(char c) => c == ' ' || (c >= '\t' && c <= '\r');
        // strtoull examines only the first trailing character; this intentionally accepts "12 junk".
        public static UIntPtr? StrToSizeT(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0 || s[0] == '-')
                return null;
            int i = 0;
            while (i < s.Length && IsAsciiWhitespace(s[i]))
                ++i;
            bool minus = i < s.Length && s[i] == '-';
            if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                ++i;
            int start = i;
            ulong n = 0;
            while (i < s.Length && s[i] >= '0' && s[i] <= '9')
            {
                uint digit = (uint)(s[i++] - '0');
                if (n > (ulong.MaxValue - digit) / 10)
                    return null;
                n = n * 10 + digit;
            }

            if (i == start)
            {
                i = 0;
                n = 0;
            } // C strtoull's endptr equals the original pointer on no conversion.

            if (i < s.Length && s[i] != '\0' && !IsAsciiWhitespace(s[i]))
                return null;
            if (minus)
                n = unchecked(0UL - n);
            if (UIntPtr.Size == 4 && n > uint.MaxValue)
                return null;
            return UIntPtr.Size == 8 ? new UIntPtr(n) : new UIntPtr((uint)n);
        }

        public static NativePath NativePathFromUtf8(ReadOnlySpan<byte> bytes) => NativePath.FromUtf8(bytes);
        public static byte[]? ReadFileToString(ReadOnlySpan<byte> path) => ReadFileToString(NativePathFromUtf8(path));
        public static byte[]? ReadFileToString(NativePath path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            try
            {
                using var input = path.OpenRead();
                using var bytes = new MemoryStream();
                input.CopyTo(bytes, 65536);
                return bytes.ToArray();
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
        }

        public static byte[]? ReadFileToString(string path)
        {
            FileStream input;
            try
            {
                // ifstream permits another process to keep the file open for writing.
                input = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            using (input)
            using (var bytes = new MemoryStream())
            {
                // Read to EOF, including pseudo-files whose reported Length is zero.
                input.CopyTo(bytes, 65536);
                return bytes.ToArray();
            }
        }

        public static string RemoveWhitespace(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            var b = new StringBuilder(s.Length);
            foreach (char c in s)
                if (!IsAsciiWhitespace(c))
                    b.Append(c);
            return b.ToString();
        }

        public static bool IsWhitespace(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            foreach (char c in s)
                if (!IsAsciiWhitespace(c))
                    return false;
            return true;
        }

        public static string[] Split(string s, string delimiter)
        {
            if (s == null || delimiter == null)
                throw new ArgumentNullException();
            if (s.Length == 0)
                return Array.Empty<string>();
            if (delimiter.Length == 0)
                throw new ArgumentException("Empty delimiter.", nameof(delimiter));
            return s.Split(new[] { delimiter }, StringSplitOptions.None);
        }

        public static void MoveToFront<T>(IList<T> list, Predicate<T> predicate)
        {
            if (list == null || predicate == null)
                throw new ArgumentNullException();
            for (int i = 0; i < list.Count; ++i)
                if (predicate(list[i]))
                {
                    T value = list[i];
                    for (int j = i; j > 0; --j)
                        list[j] = list[j - 1];
                    list[0] = value;
                    break;
                }
        }

        public static double Interpolate(double x, double x0, double x1, double y0, double y1)
        {
            if (x0 == x1)
                throw new ArgumentException("Interpolation endpoints must differ.");
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
        }

        public static int Interpolate(int x, int x0, int x1, int y0, int y1)
        {
            if (x0 == x1)
                throw new ArgumentException("Interpolation endpoints must differ.");
            return checked(y0 + checked(checked(y1 - y0) * checked(x - x0)) / checked(x1 - x0));
        }

        public static byte[] Utf8FromWstring(ReadOnlySpan<uint> codeUnits)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var chars = new char[codeUnits.Length];
                for (int i = 0; i < chars.Length; ++i)
                    chars[i] = unchecked((char)codeUnits[i]);
                return Encoding.UTF8.GetBytes(chars);
            }

            // The Unix branch of the original narrows wchar_t directly, without UTF-8 encoding.
            var bytes = new byte[codeUnits.Length];
            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = unchecked((byte)codeUnits[i]);
            return bytes;
        }

        public static string PathFromUtf8(ReadOnlySpan<byte> bytes)
        {
            try
            {
                return new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)when (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                byte[] source = bytes.ToArray();
                int n = MultiByteToWideChar(0, 0, source, source.Length, null, 0);
                if (n == 0)
                    throw new IOException("Cannot decode the legacy Windows path.");
                var chars = new char[n];
                MultiByteToWideChar(0, 0, source, source.Length, chars, n);
                return new string (chars);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int MultiByteToWideChar(uint codePage, uint flags, byte[] input, int length, [Out] char[]? output, int capacity);
    }
}
