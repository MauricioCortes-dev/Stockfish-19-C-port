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
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Search
{
    /// <summary>SF19-000496/000494: value union for a mate, tablebase distance or normalized score.</summary>
    /// <remarks>Default is Mate{0}, matching the first std::variant alternative. Get and visitor
    /// arguments are value copies. The three record types remain mutable independently of Score.</remarks>
    public readonly partial struct Score
    {
        private enum Kind : byte { Mate, Tablebase, InternalUnits }
        private readonly Kind kind;
        private readonly int value;
        private readonly bool win;

        public Score(Value v, Position pos)
        {
            if (pos == null) throw new ArgumentNullException(nameof(pos));
            if (v <= -Value.VALUE_INFINITE || v >= Value.VALUE_INFINITE)
                throw new ArgumentOutOfRangeException(nameof(v), "Score requires a finite engine value.");
            win = false;
            if (!v.IsDecisive())
            {
                kind = Kind.InternalUnits;
                value = UciFormatting.ToCp(v, pos);
            }
            else if (Math.Abs((int)v) <= Value.VALUE_TB)
            {
                int distance = Value.VALUE_TB - Math.Abs((int)v);
                kind = Kind.Tablebase;
                win = v > 0;
                value = win ? distance : -distance;
            }
            else
            {
                int distance = Value.VALUE_MATE - Math.Abs((int)v);
                kind = Kind.Mate;
                value = v > 0 ? distance : -distance;
            }
        }

        /// <summary>Like holds_alternative; unsupported record types are rejected at runtime.</summary>
        public bool Is<T>() where T : struct
        {
            if (typeof(T) == typeof(Mate)) return kind == Kind.Mate;
            if (typeof(T) == typeof(Tablebase)) return kind == Kind.Tablebase;
            if (typeof(T) == typeof(InternalUnits)) return kind == Kind.InternalUnits;
            throw new ArgumentException("T must be Score.Mate, Score.Tablebase or Score.InternalUnits.");
        }

        public T Get<T>() where T : struct
        {
            if (!Is<T>()) throw new InvalidOperationException("The requested Score alternative is not active.");
            if (kind == Kind.Mate) return (T)(object)new Mate(value);
            if (kind == Kind.Tablebase) return (T)(object)new Tablebase(value, win);
            return (T)(object)new InternalUnits(value);
        }

        /// <summary>The three handlers replace the complete overloaded C++ visitor.</summary>
        public TResult Visit<TResult>(Func<Mate, TResult> mate, Func<Tablebase, TResult> tablebase,
                                      Func<InternalUnits, TResult> units)
        {
            if (mate == null) throw new ArgumentNullException(nameof(mate));
            if (tablebase == null) throw new ArgumentNullException(nameof(tablebase));
            if (units == null) throw new ArgumentNullException(nameof(units));
            if (kind == Kind.Mate) return mate(new Mate(value));
            if (kind == Kind.Tablebase) return tablebase(new Tablebase(value, win));
            return units(new InternalUnits(value));
        }

        public void Visit(Action<Mate> mate, Action<Tablebase> tablebase, Action<InternalUnits> units)
        {
            if (mate == null) throw new ArgumentNullException(nameof(mate));
            if (tablebase == null) throw new ArgumentNullException(nameof(tablebase));
            if (units == null) throw new ArgumentNullException(nameof(units));
            if (kind == Kind.Mate) mate(new Mate(value));
            else if (kind == Kind.Tablebase) tablebase(new Tablebase(value, win));
            else units(new InternalUnits(value));
        }
    }
}
