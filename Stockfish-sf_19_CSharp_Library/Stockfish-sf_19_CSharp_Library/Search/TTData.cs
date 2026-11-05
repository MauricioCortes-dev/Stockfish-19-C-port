using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000688: copia local mutable de una entrada; no representa la tabla compartida.</summary>
    /// <remarks>C# permite default(TTData), aunque el constructor C++ sin argumentos está eliminado.</remarks>
    public struct TTData
    {
        public Move move;
        public Value value;
        public Value eval;
        public Depth depth;
        public Bound bound;
        public bool is_pv;

        public TTData(Move m, Value v, Value ev, Depth d, Bound b, bool pv)
        {
            move = m;
            value = v;
            eval = ev;
            depth = d;
            bound = b;
            is_pv = pv;
        }
    }
}
