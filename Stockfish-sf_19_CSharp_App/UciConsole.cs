using System.Globalization;
using Stockfish_sf_19_CSharp_Library;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library.Search;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_App;
/// <summary>One input thread and a persistent search-worker pool. Whole output lines share a lock.</summary>
public sealed partial class UciConsole
{
    private readonly Engine engine;
    private readonly TextWriter output;
    private readonly SyncCout writes = new();
    private bool showWdl;
    private Task<SearchResult>? search;
    private readonly TextWriter error;
    public UciConsole(Engine engine, TextWriter output, TextWriter? error = null)
    {
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        this.output = engine.LogOutput(output ?? throw new ArgumentNullException(nameof(output)));
        this.error = error ?? output;
        InitSearchUpdateListeners();
    }

    private void InitSearchUpdateListeners()
    {
        engine.SetInfoListener(s =>
        {
            if (s != null)
                Info(s);
        });
        engine.SetOnDiagnostics(stats => writes.Write(error, stats.Print));
        engine.SetOnVerifyNetwork(Info);
        engine.SetOnInfoString(Info);
        engine.SetOnUpdateNoMoves(i => Line(Format(i)));
        engine.SetOnUpdateFull(i => Line(Format(i, showWdl)));
        engine.SetOnIteration(i => Line(Format(i)));
        engine.SetOnBestmove((b, p) => Line(FormatBestmove(b, p)));
    }

    public static string Format(InfoShort i) => $"info depth {i.Depth} score {UciFormatting.FormatScore(i.Score)}";
    public static string Format(InfoFull i, bool wdl) => FormattableString.Invariant($"info depth {i.Depth} seldepth {i.SelDepth} multipv {i.MultiPV.ToUInt64()} score {UciFormatting.FormatScore(i.Score)}") + (string.IsNullOrEmpty(i.Bound) ? "" : " " + i.Bound) + (wdl ? " wdl " + i.Wdl : "") + FormattableString.Invariant($" nodes {i.Nodes.ToUInt64()} nps {i.Nps.ToUInt64()} hashfull {i.Hashfull} tbhits {i.TbHits.ToUInt64()} time {i.TimeMs.ToUInt64()} pv {i.Pv}");
    public static string Format(InfoIteration i) => FormattableString.Invariant($"info depth {i.Depth} currmove {i.CurrentMove} currmovenumber {i.CurrentMoveNumber.ToUInt64()}");
    public static string FormatBestmove(string best, string ponder) => "bestmove " + best + (string.IsNullOrEmpty(ponder) ? "" : " ponder " + ponder);
    private void Line(string text)
    {
        writes.Write(output, writer => writer.WriteLine(text));
    }

    private void Info(string text)
    {
        writes.Write(output, writer =>
        {
            foreach (string line in MiscUtilities.Split(text, "\n"))
                if (!MiscUtilities.IsWhitespace(line)) writer.WriteLine("info string " + line);
        });
    }

    public int Run(TextReader input)
    {
        try
        {
            var logged = engine.LogInput(input);
            string? line;
            while ((line = logged.ReadLine()) != null)
            {
                if (search?.IsFaulted == true)
                    search.GetAwaiter().GetResult();
                if (!Execute(line))
                    break;
            }

            engine.Stop();
            engine.WaitForSearchFinished();
            return 0;
        }
        catch (Exception e)when (e is not OutOfMemoryException)
        {
            return Fail(e);
        }
    }

    private int Fail(Exception e)
    {
        // A failed output/log stream must not prevent stop/join of the workers.
        try { Info("CRITICAL ERROR: " + e.Message); }
        catch (Exception diagnosticFailure) when (diagnosticFailure is not OutOfMemoryException) { }
        engine.Stop();
        try { engine.WaitForSearchFinished(); }
        catch (Exception failure) when (failure is not OutOfMemoryException) { }
        search = null;
        return 1;
    }

    public int RunCommand(string command)
    {
        try
        {
            Execute(command);
            // argv execution waits naturally; EOF of an interactive session requests stop.
            engine.WaitForSearchFinished();
            return 0;
        }
        catch (Exception e) when (e is not OutOfMemoryException) { return Fail(e); }
    }

    public bool Execute(string command)
    {
        string[] tokens = UciCommands.Tokens(command);
        if (tokens.Length == 0 || tokens[0].StartsWith('#'))
            return true;
        string verb = tokens[0];
        string payload = string.Join(" ", tokens.Skip(1));
        switch (verb)
        {
            case "quit":
                engine.Stop();
                return false;
            case "stop":
                engine.Stop();
                break;
            case "ponderhit":
                engine.PonderHit();
                break;
            case "isready":
                Line("readyok");
                break;
            case "uci":
                foreach (string info in EngineInformation.EngineInfo(true).Split('\n')) Line(info.StartsWith("id ") ? info : "id name " + info);
                foreach (string option in engine.OptionDescriptions.Split('\n'))
                    if (option.Length != 0)
                        Line(option);
                Line("uciok");
                break;
            case "setoption":
                if (!engine.SetOption(payload))
                    Info("Option rejected: " + payload);
                break;
            case "ucinewgame":
                engine.SearchClear();
                break;
            case "position":
                Position(tokens);
                break;
            case "go":
                LimitsType limits = UciCommands.ParseLimits(payload, MonotonicClock.Instance.Now);
                showWdl = engine.GetOptionValue("UCI_ShowWDL") == "true";
                if (limits.Perft != 0)
                {
                    var result = engine.Perft(limits.Perft);
                    foreach (var entry in result.Entries)
                        Line(UciFormatting.Move(entry.Move, engine.GetOptionValue("UCI_Chess960") == "true") + ": " + entry.Nodes.ToString(CultureInfo.InvariantCulture));
                    Line("Nodes searched: " + result.Nodes.ToString(CultureInfo.InvariantCulture));
                }
                else
                    search = engine.Go(limits, Info);
                break;
            case "bench":
                Bench(payload);
                break;
            case BenchmarkPlans.SpeedTestCommand:
                SpeedTest(payload);
                break;
            case "d":
                Line(engine.Visualize());
                break;
            case "flip":
                var flipError = engine.Flip();
                if (flipError != null) throw new FormatException(flipError.Message);
                break;
            case "compiler":
                Line(EngineInformation.CompilerInfo());
                break;
            case "eval":
                Info(engine.TraceEval());
                break;
            case "export_net":
                engine.SaveNetwork(tokens.Length > 1 ? payload : null);
                break;
            case "license":
            case "--license":
            case "help":
            case "--help":
                Line("\nStockfish is a powerful chess engine for playing and analyzing." +
                    "\nIt is released as free software licensed under the GNU GPLv3 License." +
                    "\nStockfish is normally used with a graphical user interface (GUI) and implements" +
                    "\nthe Universal Chess Interface (UCI) protocol to communicate with a GUI, an API, etc." +
                    "\nFor any further information, visit https://github.com/official-stockfish/Stockfish#readme" +
                    "\nor read the corresponding README.md and Copying.txt files distributed along with this program.\n");
                break;
            default:
                Line("Unknown command: '" + command + "'. Type help for more information.");
                break;
        }

        return true;
    }

    private void Position(string[] tokens)
    {
        if (tokens.Length < 2)
            throw new FormatException("Missing position.");
        string fen;
        int i;
        if (tokens[1] == "startpos")
        {
            fen = Engine.StartFen;
            i = 2;
        }
        else if (tokens[1] == "fen")
        {
            i = 2;
            while (i < tokens.Length && tokens[i] != "moves")
                ++i;
            fen = string.Join(" ", tokens, 2, i - 2);
        }
        else
            throw new FormatException("Expected startpos or fen.");
        if (i < tokens.Length && tokens[i++] != "moves")
            throw new FormatException("Expected moves.");
        var error = engine.SetPosition(fen, tokens.Skip(i));
        if (error != null)
            throw new FormatException(error.Message);
    }

    public static int Run(TextReader input, TextWriter output, TextWriter? error = null)
    {
        using var engine = new Engine();
        return new UciConsole(engine, output, error).Run(input);
    }
}
