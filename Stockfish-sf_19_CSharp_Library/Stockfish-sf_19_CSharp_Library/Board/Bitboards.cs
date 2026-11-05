using System;
using System.Text;
using System.Runtime.CompilerServices;
using Stockfish_sf_19_CSharp_Library.Types;
using File = Stockfish_sf_19_CSharp_Library.Types.File;

namespace Stockfish_sf_19_CSharp_Library.Board
{
    /// <summary>Geometría y operaciones escalares de bitboard.h; no contiene tablas de ataques deslizantes.</summary>
    public static class Bitboards
    {
        public static readonly Bitboard FileABB = 0x0101010101010101UL;
        public static readonly Bitboard FileBBB = FileABB << 1;
        public static readonly Bitboard FileCBB = FileABB << 2;
        public static readonly Bitboard FileDBB = FileABB << 3;
        public static readonly Bitboard FileEBB = FileABB << 4;
        public static readonly Bitboard FileFBB = FileABB << 5;
        public static readonly Bitboard FileGBB = FileABB << 6;
        public static readonly Bitboard FileHBB = FileABB << 7;
        public static readonly Bitboard Rank1BB = 0xff;
        public static readonly Bitboard Rank2BB = Rank1BB << 8;
        public static readonly Bitboard Rank3BB = Rank1BB << 16;
        public static readonly Bitboard Rank4BB = Rank1BB << 24;
        public static readonly Bitboard Rank5BB = Rank1BB << 32;
        public static readonly Bitboard Rank6BB = Rank1BB << 40;
        public static readonly Bitboard Rank7BB = Rank1BB << 48;
        public static readonly Bitboard Rank8BB = Rank1BB << 56;

        private static readonly Bitboard[] PawnPairs = CreatePawnPairs();

        public static Bitboard SquareBB(Square square)
        {
            if (!Square.IsOk(square)) throw new InvalidOperationException("square_bb requiere una casilla jugable.");
            return 1UL << square;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MoreThanOne(Bitboard board) => (board & (board - 1)) != 0;

        public static Bitboard RankBB(Rank rank)
        {
            // RANK_NB provocaría un shift de 64 bits, indefinido en C++.
            if (rank == Rank.RANK_NB) throw new InvalidOperationException("rank_bb requiere una fila del tablero.");
            return Rank1BB << (8 * rank);
        }
        public static Bitboard RankBB(Square square) => RankBB(Rank.RankOf(square));
        public static Bitboard FileBB(File file) => FileABB << file;
        public static Bitboard FileBB(Square square) => FileBB(File.FileOf(square));

        public static Bitboard Shift(Bitboard board, Direction direction) =>
            direction == Direction.NORTH ? board << 8
            : direction == Direction.SOUTH ? board >> 8
            : direction == Direction.NORTH + Direction.NORTH ? board << 16
            : direction == Direction.SOUTH + Direction.SOUTH ? board >> 16
            : direction == Direction.EAST ? (board & ~FileHBB) << 1
            : direction == Direction.WEST ? (board & ~FileABB) >> 1
            : direction == Direction.NORTH_EAST ? (board & ~FileHBB) << 9
            : direction == Direction.NORTH_WEST ? (board & ~FileABB) << 7
            : direction == Direction.SOUTH_EAST ? (board & ~FileHBB) >> 7
            : direction == Direction.SOUTH_WEST ? (board & ~FileABB) >> 9 : 0;

        public static Bitboard PawnAttacksBB(Color color, Bitboard board) => color == Color.WHITE
            ? Shift(board, Direction.NORTH_WEST) | Shift(board, Direction.NORTH_EAST)
            : Shift(board, Direction.SOUTH_WEST) | Shift(board, Direction.SOUTH_EAST);
        public static Bitboard PawnSinglePushBB(Color color, Bitboard board) =>
            Shift(board, color == Color.WHITE ? Direction.NORTH : Direction.SOUTH);

        public static Bitboard PawnPairBB(Square square)
        {
            if (!Square.IsOk(square)) throw new InvalidOperationException("pawn_pair_bb requiere una casilla jugable.");
            return PawnPairs[square];
        }
        private static Bitboard[] CreatePawnPairs()
        {
            var result = new Bitboard[64];
            for (Square square = Square.SQ_A1; square <= Square.SQ_H8; ++square)
            {
                Bitboard file = FileBB(square);
                Bitboard files = file | Shift(file, Direction.EAST) | Shift(file, Direction.WEST);
                result[square] = files & ~(Rank1BB | Rank8BB) & ~SquareBB(square);
            }
            return result;
        }

        /// <summary>Portable unsigned bit count, independent of runtime intrinsics.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Popcount(Bitboard board)
        {
            ulong bits = board;
            bits -= (bits >> 1) & 0x5555555555555555UL;
            bits = (bits & 0x3333333333333333UL) + ((bits >> 2) & 0x3333333333333333UL);
            bits = (bits + (bits >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
            return (int)(unchecked(bits * 0x0101010101010101UL) >> 56);
        }
        public static int ConstexprPopcount(ulong bits) => Popcount(bits);
        public static int ConstexprPopcount(long bits) => Popcount(unchecked((ulong)bits));
        public static int ConstexprPopcount(uint bits) => Popcount(bits);
        public static int ConstexprPopcount(int bits) => Popcount(unchecked((uint)bits));
        public static int ConstexprPopcount(ushort bits) => Popcount(bits);
        public static int ConstexprPopcount(short bits) => Popcount(unchecked((ushort)bits));
        public static int ConstexprPopcount(byte bits) => Popcount(bits);
        public static int ConstexprPopcount(sbyte bits) => Popcount(unchecked((byte)bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Lsb(Bitboard board)
        {
            RequireNonzero(board);
            ulong bits = board;
            return LsbIndex64[unchecked(((bits ^ (bits - 1)) * 0x03F79D71B4CB0A89UL) >> 58)];
        }
        private static readonly int[] LsbIndex64 = new int[]
        {
            0,  47, 1,  56, 48, 27, 2,  60, 57, 49, 41, 37, 28, 16, 3,  61, 54, 58, 35, 52, 50, 42,
            21, 44, 38, 32, 29, 23, 17, 11, 4,  62, 46, 55, 26, 59, 40, 36, 15, 53, 34, 51, 20, 43,
            31, 22, 10, 45, 25, 39, 14, 33, 19, 30, 9,  24, 13, 18, 8,  12, 7,  6,  5,  63
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConstexprLsb(ulong bits)
        {
            if (bits == 0) throw new InvalidOperationException("constexpr_lsb requires a nonzero bitboard.");
            return LsbIndex64[unchecked(((bits ^ (bits - 1)) * 0x03F79D71B4CB0A89UL) >> 58)];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Msb(Bitboard board)
        {
            RequireNonzero(board);
            ulong bits = board;
            // Rellenar los bits inferiores convierte el bit más alto en una máscara
            // compatible con la misma tabla De Bruijn usada por constexpr_lsb.
            bits |= bits >> 1;
            bits |= bits >> 2;
            bits |= bits >> 4;
            bits |= bits >> 8;
            bits |= bits >> 16;
            bits |= bits >> 32;
            return LsbIndex64[unchecked((bits * 0x03F79D71B4CB0A89UL) >> 58)];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard LeastSignificantSquareBB(Bitboard board)
        {
            RequireNonzero(board);
            return board & -board;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square PopLsb(ref Bitboard board)
        {
            Square square = Lsb(board);
            board &= board - 1;
            return square;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RequireNonzero(Bitboard board)
        {
            if (board == 0) throw new InvalidOperationException("La extracción de un bit requiere un bitboard no vacío.");
        }

        public static string Pretty(Bitboard board)
        {
            const string border = "+---+---+---+---+---+---+---+---+\n";
            var text = new StringBuilder(border);
            for (Rank rank = Rank.RANK_8;; --rank)
            {
                for (File file = File.FILE_A; file <= File.FILE_H; ++file)
                    text.Append((board & Square.MakeSquare(file, rank)) != 0 ? "| X " : "|   ");
                text.Append("| ").Append(1 + rank).Append('\n').Append(border);
                if (rank == Rank.RANK_1) break;
            }
            return text.Append("  a   b   c   d   e   f   g   h\n").ToString();
        }
    }
}
