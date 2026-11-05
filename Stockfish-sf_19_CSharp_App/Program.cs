using System.Text;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Infrastructure;
using Stockfish_sf_19_CSharp_Library;

namespace Stockfish_sf_19_CSharp_App;

public static class Program
{
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        Stockfish_sf_19_CSharp_Library.Infrastructure.Runtime.EngineRuntime.Initialize();
        Console.InputEncoding = new UTF8Encoding(false);
        Console.OutputEncoding = new UTF8Encoding(false);
        var cli = new CommandLine(typeof(Program).Assembly.Location, args);
        // Keep the existing standalone perft/eval argument grammars.
        if (args.Length != 0 && args[0] == "perft") return PerftConsole.Run(args, Console.Out, Console.Error);
        if (args.Length != 0 && args[0] == "eval") return NnueEvaluationConsole.Run(args, Console.Out, Console.Error);
        Console.WriteLine(EngineInformation.EngineInfo(false));
        if (args.Length == 0 || (args.Length == 1 && args[0] == "uci"))
            return UciConsole.Run(Console.In, Console.Out, Console.Error);
        using var engine = new Engine();
        return new UciConsole(engine, Console.Out, Console.Error).RunCommand(cli.Command + " ");
    }
}
