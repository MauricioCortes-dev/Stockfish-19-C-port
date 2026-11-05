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
using System.Reflection;
using System.Runtime.InteropServices;

namespace Stockfish_sf_19_CSharp_Library.Diagnostics
{
    /// <summary>Version/attribution and truthful managed build/runtime information.</summary>
    public static class EngineInformation
    {
        public const string Version = "19";
        public static string EngineVersionInfo() => "Stockfish " + Version + " C#";
        public static string EngineInfo(bool toUci) => EngineVersionInfo() + (toUci ? "\nid author " : " by ") + "Stockfish developers / C# port";
        private static string Metadata(string key)
        {
            foreach (var a in typeof(EngineInformation).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
                if (a.Key == key) return a.Value ?? "unknown";
            return "unknown";
        }
        public static string CompilerInfo() =>
            "\nCompiled by                : .NET SDK " + Metadata("PortingSdkVersion") +
            "\nCompilation language       : C# " + Metadata("PortingLanguageVersion") + " / " + Metadata("PortingTargetFramework") +
            "\nCompilation architecture   : managed portable IL / " + (Infrastructure.Runtime.BuildConfiguration.Is64Bit ? "64bit" : "32bit") +
            "\nRequired x86 ISA           : none (POPCNT/BMI2 build flags disabled)" +
            "\nCompilation settings       : " + Metadata("PortingConfiguration") +
            "\nExecution profiles         : " + Infrastructure.Runtime.EngineRuntime.Initialize() +
            "\nRuntime hardware vectors   : " + System.Numerics.Vector.IsHardwareAccelerated +
            "\nPrefetch hints             : " + (Infrastructure.CachePrefetch.Enabled ? "runtime SSE" : "disabled / portable no-op") +
            "\nManaged runtime            : " + RuntimeInformation.FrameworkDescription +
            "\nProcess architecture       : " + RuntimeInformation.ProcessArchitecture +
            "\nOperating system           : " + RuntimeInformation.OSDescription + "\n";
    }
}
