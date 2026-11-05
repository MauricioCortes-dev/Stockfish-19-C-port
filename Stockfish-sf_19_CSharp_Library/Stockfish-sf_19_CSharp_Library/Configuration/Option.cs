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
using System.Globalization;
using System.Linq;

namespace Stockfish_sf_19_CSharp_Library.Configuration
{
    /// <summary>UCI option value and callback. Assign invokes the callback even for an unchanged accepted value.</summary>
    public sealed class Option
    {
        public string DefaultValue { get; private set; } = "";
        public string CurrentValue { get; private set; } = "";
        public string Type { get; }
        public int Min { get; }
        public int Max { get; }

        private readonly Func<Option, string?>? onChange;
        internal OptionsMap? Parent;
        public Option(Func<Option, string?>? change = null)
        {
            Type = "button";
            onChange = change;
        }

        public Option(bool value, Func<Option, string?>? change = null)
        {
            Type = "check";
            onChange = change;
            DefaultValue = CurrentValue = value ? "true" : "false";
        }

        public Option(string value, Func<Option, string?>? change = null)
        {
            Type = "string";
            onChange = change;
            DefaultValue = CurrentValue = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Option(int value, int min, int max, Func<Option, string?>? change = null)
        {
            Type = "spin";
            onChange = change;
            Min = min;
            Max = max;
            DefaultValue = CurrentValue = value.ToString(CultureInfo.InvariantCulture);
        }

        public Option(string values, string current, Func<Option, string?>? change = null)
        {
            Type = "combo";
            onChange = change;
            DefaultValue = values;
            CurrentValue = current;
        }

        private Option(Option source)
        {
            DefaultValue = source.DefaultValue;
            CurrentValue = source.CurrentValue;
            Type = source.Type;
            Min = source.Min;
            Max = source.Max;
            onChange = source.onChange;
        }

        internal Option Copy() => new Option(this);
        public int AsInt() => Type == "check" ? (CurrentValue == "true" ? 1 : 0) : Type == "spin" ? int.Parse(CurrentValue, CultureInfo.InvariantCulture) : throw new InvalidOperationException("Expected check or spin.");
        public string AsString() => Type == "string" ? CurrentValue : throw new InvalidOperationException("Expected string.");
        public bool EqualsChoice(string value) => Type == "combo" ? CaseInsensitiveLess.Instance.Compare(CurrentValue, value) == 0 : throw new InvalidOperationException("Expected combo.");
        internal static bool ValueInRange(string v, int min, int max)
        {
            // strtoll accepts leading ASCII whitespace/sign; rejects trailing whitespace, junk and overflow.
            int i = 0;
            while (i < v.Length && " \t\n\r\v\f".IndexOf(v[i]) >= 0)
                ++i;
            if (i < v.Length && (v[i] == '+' || v[i] == '-'))
                ++i;
            int digits = i;
            while (i < v.Length && v[i] >= '0' && v[i] <= '9')
                ++i;
            return i == v.Length && i > digits && long.TryParse(v, NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long n) && n >= min && n <= max;
        }

        public bool Assign(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if ((Type != "button" && Type != "string" && value.Length == 0) || (Type == "check" && value != "true" && value != "false") || (Type == "spin" && !ValueInRange(value, Min, Max)))
                return false;
            if (Type == "combo" && (value == "var" || !DefaultValue.Split(new[] { ' ', '\t', '\n', '\r', '\v', '\f' }, StringSplitOptions.RemoveEmptyEntries).Any(t => CaseInsensitiveLess.Instance.Compare(t, value) == 0)))
                return false;
            if (Type == "string")
                CurrentValue = value == "<empty>" ? "" : value;
            else if (Type != "button")
                CurrentValue = value;
            string? info = onChange?.Invoke(this);
            if (info != null)
                Parent?.Notify(info);
            return true;
        }
    }
}
