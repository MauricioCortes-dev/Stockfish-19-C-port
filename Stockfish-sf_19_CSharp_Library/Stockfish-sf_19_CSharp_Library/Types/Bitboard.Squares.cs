using Stockfish_sf_19_CSharp_Library.Board;

namespace Stockfish_sf_19_CSharp_Library.Types
{
    public readonly partial struct Bitboard
    {
        public static Bitboard operator &(Bitboard board, Square square) => board.value & (ulong)Bitboards.SquareBB(square);
        public static Bitboard operator |(Bitboard board, Square square) => board.value | (ulong)Bitboards.SquareBB(square);
        public static Bitboard operator ^(Bitboard board, Square square) => board.value ^ (ulong)Bitboards.SquareBB(square);
        public static Bitboard operator &(Square square, Bitboard board) => board & square;
        public static Bitboard operator |(Square square, Bitboard board) => board | square;
        public static Bitboard operator ^(Square square, Bitboard board) => board ^ square;
    }
}
