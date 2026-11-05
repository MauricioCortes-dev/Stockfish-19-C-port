using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    // Preserve scalar operations even when the containing table is contiguous.
    // In particular, shared copies must still perform individual atomic loads/stores.
    internal interface IHistoryCell<T, TSelf> where T : struct where TSelf : struct
    {
        void Fill(in T value);
        void CopyFrom(ref TSelf source);
        void Swap(ref TSelf other);
    }

    // Only for closed inline, reference-free history layouts verified by tests.
    internal static class HistoryFlatAccess
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<TCell> Cells<TOwner, TCell>(ref TOwner owner, int expected)
            where TOwner : struct where TCell : struct
        {
            var cells = MemoryMarshal.Cast<TOwner, TCell>(MemoryMarshal.CreateSpan(ref owner, 1));
            if (cells.Length != expected) throw new InvalidOperationException("Unexpected history layout.");
            return cells;
        }

        internal static void Fill<TOwner, TCell, T>(ref TOwner owner, int count, in T value)
            where TOwner : struct where T : struct where TCell : struct, IHistoryCell<T, TCell>
        {
            var cells = Cells<TOwner, TCell>(ref owner, count);
            for (int i = 0; i < cells.Length; ++i) cells[i].Fill(in value);
        }

        internal static void Copy<TOwner, TCell, T>(ref TOwner owner, ref TOwner source, int count)
            where TOwner : struct where T : struct where TCell : struct, IHistoryCell<T, TCell>
        {
            var destination = Cells<TOwner, TCell>(ref owner, count);
            var origin = Cells<TOwner, TCell>(ref source, count);
            for (int i = 0; i < destination.Length; ++i) destination[i].CopyFrom(ref origin[i]);
        }

        internal static void Swap<TOwner, TCell, T>(ref TOwner owner, ref TOwner other, int count)
            where TOwner : struct where T : struct where TCell : struct, IHistoryCell<T, TCell>
        {
            var left = Cells<TOwner, TCell>(ref owner, count);
            var right = Cells<TOwner, TCell>(ref other, count);
            for (int i = 0; i < left.Length; ++i) left[i].Swap(ref right[i]);
        }
    }
}
