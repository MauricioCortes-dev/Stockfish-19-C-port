using System.Diagnostics;
using System.Globalization;
using Stockfish_sf_19_CSharp_Library.Diagnostics;

namespace Stockfish_sf_19_CSharp_App;

/// <summary>Adaptador de consola para perft; no implementa el protocolo de un motor UCI.</summary>
public static class PerftConsole
{
    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        if (args.Length == 0 || (args.Length == 1 && args[0] == "--help"))
        {
            output.WriteLine("Uso: perft [--startpos | --fen \"FEN\"] [--depth N] [--chess960]");
            output.WriteLine("Ejemplo: perft --startpos --depth 4");
            return 0;
        }
        if (args[0] != "perft") return Invalid("Se esperaba el comando perft.", error);
        string fen = StartFen;
        int depth = 4;
        bool chess960 = false, seenPosition = false, seenDepth = false;
        for (int i = 1; i < args.Length; ++i)
        {
            string option = args[i];
            if (option == "--startpos" || option == "--fen")
            {
                if (seenPosition) return Invalid("Indica una sola posición mediante --startpos o --fen.", error);
                seenPosition = true;
                if (option == "--fen")
                {
                    if (++i == args.Length) return Invalid("Falta el FEN después de --fen.", error);
                    fen = args[i];
                }
            }
            else if (option == "--depth")
            {
                if (seenDepth) return Invalid("La profundidad se indicó más de una vez.", error);
                seenDepth = true;
                if (++i == args.Length || !int.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out depth))
                    return Invalid("La profundidad debe ser un entero de 32 bits.", error);
            }
            else if (option == "--chess960")
            {
                if (chess960) return Invalid("La opción --chess960 se indicó más de una vez.", error);
                chess960 = true;
            }
            else return Invalid("Opción desconocida: " + option, error);
        }
        var watch = Stopwatch.StartNew();
        PerftResult result = Perft.Run(fen, depth, chess960);
        watch.Stop();
        if (!result.Success)
        {
            error.WriteLine("FEN inválido: " + result.Error!.Message);
            return 1;
        }
        foreach (PerftEntry entry in result.Entries)
            output.WriteLine(UciFormatting.Move(entry.Move, chess960) + ": " + entry.Nodes.ToString(CultureInfo.InvariantCulture));
        output.WriteLine();
        output.WriteLine("Nodes searched: " + result.Nodes.ToString(CultureInfo.InvariantCulture));
        output.WriteLine("Elapsed: " + watch.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms");
        return 0;
    }
    private static int Invalid(string message, TextWriter error)
    {
        error.WriteLine(message);
        return 2;
    }
}
