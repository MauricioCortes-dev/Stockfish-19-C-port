using System.Diagnostics;
using System.Globalization;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Search;

namespace Stockfish_sf_19_CSharp_App;

public sealed partial class UciConsole
{
    private void Diagnostic(string text) => writes.Write(error, writer => writer.Write(text));
    private void RestoreSearchListeners(bool clearStart)
    {
        try { engine.Stop(); engine.WaitForSearchFinished(); }
        finally { search = null; if (clearStart) engine.SetOnStart(null); InitSearchUpdateListeners(); }
    }

    private ulong RunBenchmarkSearch(string command)
    {
        LimitsType limits = UciCommands.ParseLimits(command[3..], MonotonicClock.Instance.Now);
        if (limits.Perft != 0)
        {
            var result = engine.Perft(limits.Perft);
            foreach (var entry in result.Entries)
                Line(UciFormatting.Move(entry.Move, engine.GetOptionValue("UCI_Chess960") == "true") + ": " + entry.Nodes.ToString(CultureInfo.InvariantCulture));
            Line("Nodes searched: " + result.Nodes.ToString(CultureInfo.InvariantCulture));
            return result.Nodes;
        }
        showWdl = engine.GetOptionValue("UCI_ShowWDL") == "true";
        search = engine.Go(limits);
        engine.WaitForSearchFinished();
        return 0; // Search node accounting comes from the last InfoFull, as in the original.
    }

    private void Bench(string arguments)
    {
        BenchmarkCommands commands = BenchmarkPlans.SetupBench(engine.Fen, arguments);
        ulong nodes = 0, nodesSearched = 0;
        int count = 1, total = commands.Count(c => c.StartsWith("go ", StringComparison.Ordinal) || c.StartsWith("eval", StringComparison.Ordinal));
        var elapsed = Stopwatch.StartNew();
        try
        {
            engine.SetOnUpdateFull(i => { nodesSearched = i.Nodes.ToUInt64(); Line(Format(i, showWdl)); });
            foreach (string command in commands)
            {
                if (command.StartsWith("go ") || command == "eval")
                {
                    Diagnostic($"\nPosition: {count++}/{total} ({engine.Fen})\n");
                    if (command == "eval") Execute(command);
                    else
                    {
                        ulong perft = RunBenchmarkSearch(command);
                        if (UciCommands.ParseLimits(command[3..], 0).Perft != 0) nodesSearched = perft;
                        nodes = unchecked(nodes + nodesSearched);
                        nodesSearched = 0;
                    }
                }
                else
                {
                    Execute(command);
                    if (command == "ucinewgame") elapsed.Restart();
                }
            }
            writes.Write(error, engine.DebugStats.Print);
            long milliseconds = elapsed.ElapsedMilliseconds + 1;
            Diagnostic(FormattableString.Invariant($"\n===========================\nTotal time (ms) : {milliseconds}\nNodes searched  : {nodes}\nNodes/second    : {unchecked(1000 * nodes) / (ulong)milliseconds}\n"));
        }
        finally { RestoreSearchListeners(false); }
    }

    private void SpeedTest(string arguments)
    {
        BenchmarkSetup setup = BenchmarkPlans.SetupBenchmark(arguments, message => Diagnostic(message + "\n"));
        int total = setup.commands.Count(c => c.StartsWith("go "));
        int count = 1, readings = 0;
        int[] totalHash = new int[2], maxHash = new int[2];
        ulong nodes = 0, nodesSearched = 0;
        long started = 0, totalTicks = 0;
        try
        {
            engine.SetOnUpdateFull(_ => { });
            engine.SetOnIteration(_ => { });
            engine.SetOnUpdateNoMoves(_ => { });
            engine.SetOnBestmove((_, __) => { });
            engine.SetOnVerifyNetwork(_ => { });
            Execute("setoption name Threads value " + setup.threads.ToString(CultureInfo.InvariantCulture));
            Execute("setoption name Hash value " + setup.ttSize.ToString(CultureInfo.InvariantCulture));
            Execute("setoption name UCI_Chess960 value false");
            foreach (string command in setup.commands)
            {
                if (command.StartsWith("go "))
                {
                    Diagnostic($"\rWarmup position {count++}/3");
                    RunBenchmarkSearch(command);
                }
                else Execute(command);
                if (count > 3) break;
            }
            Diagnostic("\n");
            count = 1;
            engine.SearchClear();
            engine.SetOnUpdateFull(i => nodesSearched = i.Nodes.ToUInt64());
            engine.SetOnStart(() => { started = Stopwatch.GetTimestamp(); nodesSearched = 0; });
            engine.SetOnBestmove((_, __) =>
            {
                totalTicks += Stopwatch.GetTimestamp() - started;
                nodes = unchecked(nodes + nodesSearched);
            });
            foreach (string command in setup.commands)
            {
                if (command.StartsWith("go "))
                {
                    Diagnostic($"\rPosition {count++}/{total}");
                    RunBenchmarkSearch(command);
                    ++readings;
                    for (int i = 0; i < 2; ++i)
                    {
                        int hash = engine.GetHashfull(i == 0 ? 0 : 999);
                        maxHash[i] = Math.Max(maxHash[i], hash);
                        totalHash[i] += hash;
                    }
                }
                else Execute(command);
            }
            writes.Write(error, engine.DebugStats.Print);
            long milliseconds = Math.Max(1, (long)(totalTicks * (1000.0 / Stopwatch.Frequency)));
            var resources = engine.ResourceInfo;
            Diagnostic("\n===========================\nVersion                    : " + EngineInformation.EngineVersionInfo() +
                EngineInformation.CompilerInfo() +
                "Large pages                : " + (resources.UsesLargePages ? "yes" : "no") +
                "\nUser invocation            : speedtest " + setup.originalInvocation +
                "\nFilled invocation          : speedtest " + setup.filledInvocation +
                "\nAvailable processors       : " + resources.ProcessorDescription +
                FormattableString.Invariant($"\nThread count               : {setup.threads}\nThread binding             : {resources.ThreadBinding}\nTT size [MiB]              : {setup.ttSize}\nHash max, avg [per mille]  : \n    single search          : {maxHash[0]}, {totalHash[0] / readings}\n    single game            : {maxHash[1]}, {totalHash[1] / readings}\nTotal nodes searched       : {nodes}\nTotal search time [s]      : {milliseconds / 1000.0:G6}\nNodes/second               : {unchecked(1000 * nodes) / (ulong)milliseconds}\n"));
        }
        finally { RestoreSearchListeners(true); }
    }
}
