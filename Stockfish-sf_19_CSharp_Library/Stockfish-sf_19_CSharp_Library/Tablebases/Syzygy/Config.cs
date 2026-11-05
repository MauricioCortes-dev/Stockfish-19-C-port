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

using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Tablebases.Syzygy
{
    /// <summary>Configuración mutable por valor del sondeo Syzygy (SF19-000624).</summary>
    /// <remarks>Default y new inicializan los cuatro campos a cero, como el agregado C++.</remarks>
    public struct Config
    {
        public int cardinality;
        public bool rootInTB;
        public bool useRule50;
        public Depth probeDepth;

        public Config(int cardinality, bool rootInTB, bool useRule50, Depth probeDepth)
        {
            this.cardinality = cardinality;
            this.rootInTB = rootInTB;
            this.useRule50 = useRule50;
            this.probeDepth = probeDepth;
        }
    }
}
