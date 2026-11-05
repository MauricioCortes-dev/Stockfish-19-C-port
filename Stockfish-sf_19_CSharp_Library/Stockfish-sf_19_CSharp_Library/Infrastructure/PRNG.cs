using System;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>SF19-000190: xorshift64* con estado mutable y copias independientes por valor.</summary>
    public struct PRNG
    {
        private ulong state;

        public PRNG(ulong seed)
        {
            if (seed == 0) throw new ArgumentOutOfRangeException(nameof(seed), seed, "La semilla debe ser distinta de cero.");
            state = seed;
        }

        public ulong Rand()
        {
            if (state == 0) throw new InvalidOperationException("PRNG requiere una semilla no nula antes de utilizarse.");
            state ^= state >> 12;
            state ^= state << 25;
            state ^= state >> 27;
            return unchecked(state * 2685821657736338717UL);
        }

        public ulong SparseRand()
        {
            ulong first = Rand(), second = Rand(), third = Rand();
            return first & second & third;
        }

        // C# 8 carece de una restricción genérica para conversiones numéricas.
        // Se validan los destinos antes de consumir el estado del generador.
        public T Rand<T>()
        {
            RequireSupported<T>();
            return ConvertBits<T>(Rand());
        }
        public T SparseRand<T>()
        {
            RequireSupported<T>();
            return ConvertBits<T>(SparseRand());
        }
        private static void RequireSupported<T>()
        {
            Type type = typeof(T);
            if (type != typeof(ulong) && type != typeof(long) && type != typeof(uint) && type != typeof(int)
                && type != typeof(ushort) && type != typeof(short) && type != typeof(byte) && type != typeof(sbyte)
                && type != typeof(Key) && type != typeof(Bitboard))
                throw new NotSupportedException("PRNG admite enteros de anchura fija, Key y Bitboard.");
        }
        private static T ConvertBits<T>(ulong bits)
        {
            object converted;
            unchecked
            {
                if (typeof(T) == typeof(ulong)) converted = bits;
                else if (typeof(T) == typeof(long)) converted = (long)bits;
                else if (typeof(T) == typeof(uint)) converted = (uint)bits;
                else if (typeof(T) == typeof(int)) converted = (int)bits;
                else if (typeof(T) == typeof(ushort)) converted = (ushort)bits;
                else if (typeof(T) == typeof(short)) converted = (short)bits;
                else if (typeof(T) == typeof(byte)) converted = (byte)bits;
                else if (typeof(T) == typeof(sbyte)) converted = (sbyte)bits;
                else if (typeof(T) == typeof(Key)) converted = new Key(bits);
                else converted = new Bitboard(bits); // RequireSupported ya validó el destino.
            }
            return (T)converted;
        }
    }
}
