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
using System.Text;

namespace Stockfish_sf_19_CSharp_Library.Configuration
{
    /// <summary>Unique owner; options are copied on insertion. Configure only while the worker is idle.</summary>
    public sealed class OptionsMap
    {
        private readonly SortedDictionary<string, Option> values = new SortedDictionary<string, Option>(CaseInsensitiveLess.Instance);
        private readonly List<string> order = new List<string>();
        private Action<string?>? info;
        public void AddInfoListener(Action<string?> listener) => info = listener;
        internal void Notify(string message) => info?.Invoke(message);
        public Option this[string name] => values[name];
        public UIntPtr Count(string name) => values.ContainsKey(name) ? new UIntPtr(1u) : UIntPtr.Zero;
        public void Add(string name, Option value)
        {
            if (values.ContainsKey(name))
                throw new ArgumentException("Option already added: " + name, nameof(name));
            var copy = value.Copy();
            copy.Parent = this;
            values.Add(name, copy);
            order.Add(name);
        }

        /// <summary>Consumes the payload 'name ... value ...'; unknown names are returned to the adapter.</summary>
        public bool SetOption(string command)
        {
            string[] tokens = command.Split(new[] { ' ', '\t', '\n', '\r', '\v', '\f' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 1;
            var name = new List<string>();
            while (i < tokens.Length && tokens[i] != "value")
                name.Add(tokens[i++]);
            ++i;
            string key = string.Join(" ", name);
            if (!values.TryGetValue(key, out Option? option))
            {
                Notify("No such option: " + key);
                return false;
            }

            return option.Assign(string.Join(" ", tokens, Math.Min(i, tokens.Length), Math.Max(0, tokens.Length - i)));
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            foreach (string name in order)
            {
                Option o = values[name];
                s.Append("\noption name ").Append(name).Append(" type ").Append(o.Type);
                if (o.Type == "check" || o.Type == "combo")
                    s.Append(" default ").Append(o.DefaultValue);
                else if (o.Type == "string")
                    s.Append(" default ").Append(o.DefaultValue.Length == 0 ? "<empty>" : o.DefaultValue);
                else if (o.Type == "spin")
                    s.Append(" default ").Append(int.Parse(o.DefaultValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)).Append(" min ").Append(o.Min.ToString(CultureInfo.InvariantCulture)).Append(" max ").Append(o.Max.ToString(CultureInfo.InvariantCulture));
            }

            return s.ToString();
        }

        /// <summary>Search options only. Engine/resource callbacks are integrated in batch 4.</summary>
        public static OptionsMap CreateSearchDefaults()
        {
            var o = new OptionsMap();
            o.Add("Ponder", new Option(false));
            o.Add("MultiPV", new Option(1, 1, 256));
            o.Add("Skill Level", new Option(20, 0, 20));
            o.Add("Move Overhead", new Option(10, 0, 5000));
            o.Add("nodestime", new Option(0, 0, 10000));
            o.Add("UCI_LimitStrength", new Option(false));
            o.Add("UCI_Elo", new Option(1320, 1320, 3190));
            o.Add("UCI_ShowWDL", new Option(false));
            o.Add("SyzygyPath", new Option(""));
            o.Add("SyzygyProbeDepth", new Option(1, 1, 100));
            o.Add("Syzygy50MoveRule", new Option(true));
            o.Add("SyzygyProbeLimit", new Option(7, 0, 7));
            return o;
        }
    }
}
