using System;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.MoveGeneration
{
    /// <summary>SF19-000211/000212: jugada y puntuación con copias mutables por valor.</summary>
    public struct ExtMove : IEquatable<ExtMove>
    {
        private Move move;
        public int value;

        public ExtMove(Move move, int value) { this.move = move; this.value = value; }

        /// <summary>Equivale a operator=(Move): conserva value. C# no permite sobrecargar =.</summary>
        public void Assign(Move replacement) => move = replacement;

        public ushort Raw() => move.Raw();
        public Square FromSq() => move.FromSq();
        public Square ToSq() => move.ToSq();
        public MoveType TypeOf() => move.TypeOf();
        public PieceType PromotionType() => move.PromotionType();
        public bool IsOk() => move.IsOk();

        // Slicing por valor de la base original; nunca comparte una jugada mutable.
        public static implicit operator Move(ExtMove extended) => extended.move;
        public static explicit operator bool(ExtMove extended) => (bool)extended.move;
        public static bool operator true(ExtMove extended) => (bool)extended.move;
        public static bool operator false(ExtMove extended) => !(bool)extended.move;
        public static bool operator !(ExtMove extended) => !extended.move;

        public static bool operator <(ExtMove left, ExtMove right) => left.value < right.value;
        // C# exige declarar > junto a <. Equivale a right < left del original.
        public static bool operator >(ExtMove left, ExtMove right) => right < left;

        // Las comparaciones heredadas de Move ignoran la puntuación.
        public static bool operator ==(ExtMove left, ExtMove right) => left.move == right.move;
        public static bool operator !=(ExtMove left, ExtMove right) => left.move != right.move;
        public static bool operator ==(ExtMove left, Move right) => left.move == right;
        public static bool operator !=(ExtMove left, Move right) => left.move != right;
        public static bool operator ==(Move left, ExtMove right) => left == right.move;
        public static bool operator !=(Move left, ExtMove right) => left != right.move;
        public bool Equals(ExtMove other) => this == other;
        public override bool Equals(object? obj) => obj is ExtMove other && Equals(other);
        public override int GetHashCode() => move.GetHashCode();
    }
}
