using Stockfish_sf_19_CSharp_Library.Types;
using System.Runtime.CompilerServices;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>SF19-000731: registro inmutable de una amenaza, conservando sus 32 bits originales.</summary>
    public readonly struct DirtyThreat
    {
        public const int PcSqOffset = 0;
        public const int ThreatenedSqOffset = 8;
        public const int ThreatenedPcOffset = 16;
        public const int PcOffset = 20;
        private readonly uint data;

        public DirtyThreat(uint raw) => data = raw;

        public DirtyThreat(Piece pc, Piece threatened_pc, Square pc_sq, Square threatened_sq, bool add)
        {
            // No se recortan entradas: PIECE_NB y SQ_NONE conservan sus acarreos/bits originales.
            data = ((add ? 1U : 0U) << 31) | (uint)(pc << PcOffset)
                | (uint)(threatened_pc << ThreatenedPcOffset)
                | (uint)(threatened_sq << ThreatenedSqOffset) | (uint)(pc_sq << PcSqOffset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece Pc() => (int)((data >> PcOffset) & 0xf);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece ThreatenedPc() => (int)((data >> ThreatenedPcOffset) & 0xf);
        // Raw admite todo u32; un byte externo a 0..64 conserva el rechazo de Square.
        public Square ThreatenedSq() => (int)((data >> ThreatenedSqOffset) & 0xff);
        public Square PcSq() => (int)((data >> PcSqOffset) & 0xff);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add() => (data >> 31) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Raw() => data;
        public static implicit operator DirtyThreat(uint raw) => new DirtyThreat(raw);
    }
}
