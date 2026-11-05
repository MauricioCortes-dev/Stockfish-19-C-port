using System.Globalization;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Diagnostics;
using Stockfish_sf_19_CSharp_Library.Evaluation;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;
using Stockfish_sf_19_CSharp_Library.Search;

namespace Stockfish_sf_19_CSharp_App;
public static class NnueEvaluationConsole
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        if (args.Length == 0 || args[0] != "eval")
            return Invalid("Se esperaba el comando eval.", error);
        if (args.Length == 2 && args[1] == "--help")
        {
            output.WriteLine("Uso: eval [--network ARCHIVO.nnue] [--startpos | --fen \"FEN\"] [--chess960] [--optimism N] [--trace]");
            return 0;
        }

        string fen = PerftConsole.StartFen, path = EvalFile.DefaultName;
        bool chess960 = false, trace = false, seenPosition = false;
        int optimism = 0;
        var seen = new HashSet<string>();
        for (int i = 1; i < args.Length; ++i)
        {
            string option = args[i];
            if (!seen.Add(option))
                return Invalid("Opción repetida: " + option, error);
            switch (option)
            {
                case "--startpos":
                case "--fen":
                    if (seenPosition)
                        return Invalid("Indica una sola posición.", error);
                    seenPosition = true;
                    if (option == "--fen")
                    {
                        if (++i == args.Length)
                            return Invalid("Falta el FEN.", error);
                        fen = args[i];
                    }

                    break;
                case "--network":
                    if (++i == args.Length)
                        return Invalid("Falta el archivo de red.", error);
                    path = args[i];
                    break;
                case "--optimism":
                    if (++i == args.Length || !int.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out optimism))
                        return Invalid("Optimism debe ser un entero de 32 bits.", error);
                    break;
                case "--chess960":
                    chess960 = true;
                    break;
                case "--trace":
                    trace = true;
                    break;
                default:
                    return Invalid("Opción desconocida: " + option, error);
            }
        }

        if (trace && seen.Contains("--optimism"))
            return Invalid("--trace utiliza el optimismo cero del diagnóstico original; omite --optimism.", error);
        var pos = new Position();
        var failure = pos.Set(fen, chess960, new StateInfo());
        if (failure != null)
        {
            error.WriteLine("FEN inválido: " + failure.Message);
            return 1;
        }

        if (pos.Checkers() != 0 && !trace)
        {
            error.WriteLine("La evaluación estática requiere una posición sin jaque.");
            return 1;
        }

        try
        {
            var network = new Network();
            var evalFile = new EvalFile();
            network.Load(AppContext.BaseDirectory, path, evalFile);
            network.Verify(null, evalFile, path);
            if (trace)
                output.Write(Evaluator.Trace(pos, network));
            else
            {
                var stack = new AccumulatorStack();
                var caches = new AccumulatorCaches(network);
                var value = Evaluator.Evaluate(network, pos, stack, caches, optimism);
                output.WriteLine("info score " + UciFormatting.FormatScore(new Score(value, pos)) + " wdl " + UciFormatting.Wdl(value, pos));
                output.WriteLine("Evaluation (internal units): " + ((int)value).ToString(CultureInfo.InvariantCulture));
            }

            return 0;
        }
        catch (Exception ex)when (ex is IOException or UnauthorizedAccessException or NetworkVerificationException or OverflowException)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int Invalid(string message, TextWriter error)
    {
        error.WriteLine(message);
        return 2;
    }
}
