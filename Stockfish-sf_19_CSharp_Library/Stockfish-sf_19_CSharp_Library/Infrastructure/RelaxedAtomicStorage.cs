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
using System.Threading;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Inline representation policy. Load/Store must be individually atomic for shared use.</summary>
    public interface IRelaxedAtomicStorage<T> where T : struct
    {
        T Load();
        void Store(T value);
        T One { get; }
        T Add(T left, T right);
        T Subtract(T left, T right);
    }

    /// <summary>Signed arithmetic/promotion policy, also supporting ordinary thread-owned storage.</summary>
    public interface ISignedAtomicStorage<T> : IRelaxedAtomicStorage<T> where T : struct
    {
        long Maximum { get; }
        int ArithmeticBits { get; }
        long ToWide(T value);
        T Narrow(long value);
        T LoadUnshared();
        void StoreUnshared(T value);
    }
    public struct AtomicInt8 : ISignedAtomicStorage<sbyte>
    {
        private sbyte value;
        public sbyte Load() => Volatile.Read(ref value);
        public void Store(sbyte item) => Volatile.Write(ref value, item);
        public sbyte One => 1;
        public sbyte Add(sbyte left, sbyte right) => unchecked((sbyte)(left + right));
        public sbyte Subtract(sbyte left, sbyte right) => unchecked((sbyte)(left - right));
        public long Maximum => sbyte.MaxValue;
        public int ArithmeticBits => 32;
        public long ToWide(sbyte item) => item;
        public sbyte Narrow(long item) => unchecked((sbyte)item);
        public sbyte LoadUnshared() => value;
        public void StoreUnshared(sbyte item) => value = item;
    }
    public struct AtomicUInt8 : IRelaxedAtomicStorage<byte>
    {
        private byte value;
        public byte Load() => Volatile.Read(ref value);
        public void Store(byte item) => Volatile.Write(ref value, item);
        public byte One => 1;
        public byte Add(byte left, byte right) => unchecked((byte)(left + right));
        public byte Subtract(byte left, byte right) => unchecked((byte)(left - right));
    }
    public struct AtomicInt16 : ISignedAtomicStorage<short>
    {
        private short value;
        public short Load() => Volatile.Read(ref value);
        public void Store(short item) => Volatile.Write(ref value, item);
        public short One => 1;
        public short Add(short left, short right) => unchecked((short)(left + right));
        public short Subtract(short left, short right) => unchecked((short)(left - right));
        public long Maximum => short.MaxValue;
        public int ArithmeticBits => 32;
        public long ToWide(short item) => item;
        public short Narrow(long item) => unchecked((short)item);
        public short LoadUnshared() => value;
        public void StoreUnshared(short item) => value = item;
    }
    public struct AtomicUInt16 : IRelaxedAtomicStorage<ushort>
    {
        private ushort value;
        public ushort Load() => Volatile.Read(ref value);
        public void Store(ushort item) => Volatile.Write(ref value, item);
        public ushort One => 1;
        public ushort Add(ushort left, ushort right) => unchecked((ushort)(left + right));
        public ushort Subtract(ushort left, ushort right) => unchecked((ushort)(left - right));
    }
    public struct AtomicInt32 : ISignedAtomicStorage<int>
    {
        private int value;
        public int Load() => Volatile.Read(ref value);
        public void Store(int item) => Volatile.Write(ref value, item);
        public int One => 1;
        public int Add(int left, int right) => checked(left + right);
        public int Subtract(int left, int right) => checked(left - right);
        public long Maximum => int.MaxValue;
        public int ArithmeticBits => 32;
        public long ToWide(int item) => item;
        public int Narrow(long item) => unchecked((int)item);
        public int LoadUnshared() => value;
        public void StoreUnshared(int item) => value = item;
    }
    public struct AtomicUInt32 : IRelaxedAtomicStorage<uint>
    {
        private uint value;
        public uint Load() => Volatile.Read(ref value);
        public void Store(uint item) => Volatile.Write(ref value, item);
        public uint One => 1;
        public uint Add(uint left, uint right) => unchecked((uint)(left + right));
        public uint Subtract(uint left, uint right) => unchecked((uint)(left - right));
    }
    public struct AtomicInt64 : ISignedAtomicStorage<long>
    {
        private long value;
        public long Load() => Volatile.Read(ref value);
        public void Store(long item) => Volatile.Write(ref value, item);
        public long One => 1;
        public long Add(long left, long right) => checked(left + right);
        public long Subtract(long left, long right) => checked(left - right);
        public long Maximum => long.MaxValue;
        public int ArithmeticBits => 64;
        public long ToWide(long item) => item;
        public long Narrow(long item) => unchecked((long)item);
        public long LoadUnshared() => value;
        public void StoreUnshared(long item) => value = item;
    }
    public struct AtomicUInt64 : IRelaxedAtomicStorage<ulong>
    {
        private ulong value;
        public ulong Load() => Volatile.Read(ref value);
        public void Store(ulong item) => Volatile.Write(ref value, item);
        public ulong One => 1;
        public ulong Add(ulong left, ulong right) => unchecked((ulong)(left + right));
        public ulong Subtract(ulong left, ulong right) => unchecked((ulong)(left - right));
    }
    /// <summary>Move's 16-bit representation; C++ does not instantiate its arithmetic members.</summary>
    public struct AtomicMove : IRelaxedAtomicStorage<Move>
    {
        private ushort value;
        public Move Load() => new Move(Volatile.Read(ref value));
        public void Store(Move item) => Volatile.Write(ref value, item.Raw());
        public Move One => throw new NotSupportedException("Move has no atomic arithmetic.");
        public Move Add(Move left, Move right) => throw new NotSupportedException("Move has no atomic arithmetic.");
        public Move Subtract(Move left, Move right) => throw new NotSupportedException("Move has no atomic arithmetic.");
    }
}
