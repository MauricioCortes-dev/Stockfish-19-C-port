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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Configuration
{
    /// <summary>Original tuning registry. Registration/init/updates require an idle engine.
    /// CLR delegates preserve live lvalues; each registry belongs to one options owner.</summary>
    public sealed class Tune
    {
        private abstract class EntryBase
        {
            public abstract void Init(Tune owner);
            public abstract void Read(Tune owner);
        }

        private sealed class Entry : EntryBase
        {
            private readonly string name;
            private readonly TuneParameter value;
            private readonly SetRange range;
            public Entry(string name, TuneParameter value, SetRange range)
            {
                this.name = name;
                this.value = value;
                this.range = range;
            }

            public override void Init(Tune owner) => owner.MakeOption(name, value.Value, range);
            public override void Read(Tune owner)
            {
                if (owner.options!.Count(name) != UIntPtr.Zero)
                    value.Value = owner.options[name].AsInt();
            }
        }

        private sealed class PostUpdate : EntryBase
        {
            private readonly Action action;
            public PostUpdate(Action action)
            {
                this.action = action;
            }

            public override void Init(Tune owner)
            {
            }

            public override void Read(Tune owner) => action();
        }

        private readonly List<EntryBase> entries = new List<EntryBase>();
        private readonly Dictionary<string, int> results = new Dictionary<string, int>(StringComparer.Ordinal);
        private OptionsMap? options;
        private Option? lastOption;
        private readonly TextWriter output;
        public bool UpdateOnLast { get; set; }

        public Tune(TextWriter? output = null)
        {
            this.output = output ?? TextWriter.Null;
            ReadResults();
        }

        /// <summary>The original read_results has an empty body; callers may supply compiled-in results explicitly.</summary>
        public void ReadResults(Action<IDictionary<string, int>>? initialize = null)
        {
            if (options != null)
                throw new InvalidOperationException("Results must precede initialization.");
            initialize?.Invoke(results);
        }

        public static string Next(ref string names, bool pop = true)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            var name = new StringBuilder();
            int balance = 0;
            do
            {
                int comma = names.IndexOf(',');
                string token = comma < 0 ? names : names.Substring(0, comma);
                if (pop)
                    names = comma < 0 ? "" : names.Substring(comma + 1);
                int start = 0;
                while (start < token.Length && MiscUtilities.IsAsciiWhitespace(token[start]))
                    ++start;
                int end = start;
                while (end < token.Length && !MiscUtilities.IsAsciiWhitespace(token[end]))
                    ++end;
                token = token.Substring(start, end - start);
                name.Append(token);
                foreach (char c in token)
                {
                    if (c == '(')
                        ++balance;
                    if (c == ')')
                        --balance;
                }

                if (balance != 0 && (!pop || comma < 0))
                    throw new FormatException("Unbalanced tuning name.");
            }
            while (balance != 0);
            return name.ToString();
        }

        /// <summary>Names use the original parenthesized TUNE expression. Arguments: live integers,
        /// int arrays (including rectangular/jagged), ranges or post-update actions.</summary>
        public void Add(string names, params object[] arguments)
        {
            if (options != null)
                throw new InvalidOperationException("Registration must precede initialization.");
            if (names == null || arguments == null)
                throw new ArgumentNullException();
            if (names.Length < 2 || names[0] != '(' || names[names.Length - 1] != ')')
                throw new FormatException("Expected parenthesized tuning names.");
            names = names.Substring(1, names.Length - 2);
            SetRange range = SetRange.Default;
            foreach (object value in arguments)
            {
                string name = Next(ref names);
                if (value is SetRange r)
                    range = r;
                else
                    Register(name, value, range);
            }
        }

        private void Register(string name, object value, SetRange range)
        {
            if (value is TuneParameter parameter)
            {
                entries.Add(new Entry(name, parameter, range));
                return;
            }

            if (value is Action action)
            {
                entries.Add(new PostUpdate(action));
                return;
            }

            if (value is Array array)
            {
                var indices = new int[array.Rank];
                void Visit(int dimension, string prefix)
                {
                    for (int i = array.GetLowerBound(dimension); i <= array.GetUpperBound(dimension); ++i)
                    {
                        indices[dimension] = i;
                        string next = prefix + "[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                        if (dimension + 1 < array.Rank)
                            Visit(dimension + 1, next);
                        else
                        {
                            int[] owned = (int[])indices.Clone();
                            object? item = array.GetValue(owned);
                            if (item is int)
                                Register(next, new TuneParameter(() => (int)array.GetValue(owned)!, x => array.SetValue(x, owned)), range);
                            else if (item is Value)
                                Register(next, new TuneParameter(() => (Value)array.GetValue(owned)!, x => array.SetValue((Value)x, owned)), range);
                            else if (item is Depth)
                                Register(next, new TuneParameter(() => (Depth)array.GetValue(owned)!, x => array.SetValue((Depth)x, owned)), range);
                            else if (item != null)
                                Register(next, item, range);
                            else
                                throw new ArgumentException("Null tuning entry.");
                        }
                    }
                }

                Visit(0, name);
                return;
            }

            throw new ArgumentException("Use a live TuneParameter, integer array, SetRange or Action.");
        }

        public void Init(OptionsMap target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (options != null)
                throw new InvalidOperationException("Registry already initialized.");
            options = target;
            foreach (var entry in entries)
                entry.Init(this);
            ReadOptions();
        }

        public void ReadOptions()
        {
            if (options == null)
                throw new InvalidOperationException("Registry not initialized.");
            foreach (var entry in entries)
                entry.Read(this);
        }

        private void MakeOption(string name, int value, SetRange range)
        {
            var limits = range.Invoke(value);
            if (limits.Min == limits.Max)
                return;
            if (results.TryGetValue(name, out int result))
                value = result;
            limits = range.Invoke(value);
            options!.Add(name, new Option(value, limits.Min, limits.Max, changed =>
            {
                if (!UpdateOnLast || ReferenceEquals(lastOption, changed))
                    ReadOptions();
                return null;
            }));
            lastOption = options[name];
            output.WriteLine(FormattableString.Invariant($"{name},{value},{limits.Min},{limits.Max},{(limits.Max - limits.Min) / 20.0:G6},0.0020"));
            output.Flush();
        }
    }
}
