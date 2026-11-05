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

namespace Stockfish_sf_19_CSharp_Library.Infrastructure
{
    /// <summary>Inline storage of 8 values; storage adaptation, not an additional engine domain.</summary>
    public struct ValueStorage8<T> : IValueListStorage<T>
    {
        private T v0, v1, v2, v3, v4, v5, v6, v7;
        public int Capacity => 8;
        public T this[int index]
        {
            get
            {
                if ((uint)index >= 8U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index)
                {
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                    case 3: return v3;
                    case 4: return v4;
                    case 5: return v5;
                    case 6: return v6;
                    default: return v7;
                }
            }
            set
            {
                if ((uint)index >= 8U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index)
                {
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                    case 3: v3 = value; break;
                    case 4: v4 = value; break;
                    case 5: v5 = value; break;
                    case 6: v6 = value; break;
                    default: v7 = value; break;
                }
            }
        }
    }
    /// <summary>Inline storage of 32 values; storage adaptation, not an additional engine domain.</summary>
    public struct ValueStorage32<T> : IValueListStorage<T>
    {
        private ValueStorage8<T> v0, v1, v2, v3;
        public int Capacity => 32;
        public T this[int index]
        {
            get
            {
                if ((uint)index >= 32U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 8)
                {
                    case 0: return v0[index % 8];
                    case 1: return v1[index % 8];
                    case 2: return v2[index % 8];
                    default: return v3[index % 8];
                }
            }
            set
            {
                if ((uint)index >= 32U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 8)
                {
                    case 0: v0[index % 8] = value; break;
                    case 1: v1[index % 8] = value; break;
                    case 2: v2[index % 8] = value; break;
                    default: v3[index % 8] = value; break;
                }
            }
        }
    }
    /// <summary>Inline storage of 96 values; storage adaptation, not an additional engine domain.</summary>
    public struct ValueStorage96<T> : IValueListStorage<T>
    {
        private ValueStorage32<T> v0, v1, v2;
        public int Capacity => 96;
        public T this[int index]
        {
            get
            {
                if ((uint)index >= 96U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 32)
                {
                    case 0: return v0[index % 32];
                    case 1: return v1[index % 32];
                    default: return v2[index % 32];
                }
            }
            set
            {
                if ((uint)index >= 96U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 32)
                {
                    case 0: v0[index % 32] = value; break;
                    case 1: v1[index % 32] = value; break;
                    default: v2[index % 32] = value; break;
                }
            }
        }
    }
    /// <summary>Inline storage of 256 values; storage adaptation, not an additional engine domain.</summary>
    public struct ValueStorage256<T> : IValueListStorage<T>
    {
        private ValueStorage32<T> v0, v1, v2, v3, v4, v5, v6, v7;
        public int Capacity => 256;
        public T this[int index]
        {
            get
            {
                if ((uint)index >= 256U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 32)
                {
                    case 0: return v0[index % 32];
                    case 1: return v1[index % 32];
                    case 2: return v2[index % 32];
                    case 3: return v3[index % 32];
                    case 4: return v4[index % 32];
                    case 5: return v5[index % 32];
                    case 6: return v6[index % 32];
                    default: return v7[index % 32];
                }
            }
            set
            {
                if ((uint)index >= 256U) throw new ArgumentOutOfRangeException(nameof(index));
                switch (index / 32)
                {
                    case 0: v0[index % 32] = value; break;
                    case 1: v1[index % 32] = value; break;
                    case 2: v2[index % 32] = value; break;
                    case 3: v3[index % 32] = value; break;
                    case 4: v4[index % 32] = value; break;
                    case 5: v5[index % 32] = value; break;
                    case 6: v6[index % 32] = value; break;
                    default: v7[index % 32] = value; break;
                }
            }
        }
    }
}
