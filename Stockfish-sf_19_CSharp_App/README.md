# Consola UCI y de comprobación de la biblioteca

La [tanda operativa](../Stockfish-sf_19/porting/Diagnostics/OperationalTools.TestResults.md)
incorpora `bench`, `speedtest`, `d`, `flip`, `compiler` y `Debug Log File`. Los
comandos se admiten en el bucle UCI y desde argumentos. Por ejemplo:

```bash
dotnet run --project Stockfish-sf_19_CSharp_App -- bench 1 1 3 current depth
dotnet run --project Stockfish-sf_19_CSharp_App -- speedtest 1 1 1
```

La red NNUE debe estar accesible mediante el nombre predeterminado en el directorio
de trabajo/binario, o configurarse `EvalFile` antes en una sesión interactiva.
`bench` admite TT/hilos/límite/default|current|archivo/tipo; `speedtest` admite
hilos/TT/segundos. Sus defaults conservan el original y pueden requerir mucha RAM.
Los informes describen la topología NUMA seleccionada y la afinidad efectiva; NNUE usa pesos mapeados de solo lectura compartidos entre procesos por dominio físico,
con fallback local e informes de estado/error reales. Ver [aceptación](../Stockfish-sf_19/porting/Infrastructure/Memory/SharedNnue.TestResults.md).
El registro es UTF-8 por motor; vacío o `<empty>` lo desactiva. Se conservan los
comandos independientes perft/eval con su gramática previa.
Aplicación net10.0 que referencia Stockfish-sf_19_CSharp_Library (netstandard2.1).
Ofrece el [perft escalar](../Stockfish-sf_19/porting/Diagnostics/Perft.md): árbol
de jugadas legales sin búsqueda ni red NNUE. Muestra cada jugada inicial y su
conteo, el total y el tiempo transcurrido en milisegundos.

Desde la raíz de la solución:

```powershell
dotnet run --project './Stockfish-sf_19_CSharp_App/Stockfish-sf_19_CSharp_App.csproj' -c Release -- perft --startpos --depth 4
dotnet run --project './Stockfish-sf_19_CSharp_App/Stockfish-sf_19_CSharp_App.csproj' -c Release -- perft --fen '4k3/8/8/8/8/8/8/6KR w H - 0 1' --depth 1 --chess960
```

En Rider, seleccionar la configuración del proyecto App y poner en **Program
arguments** `perft --startpos --depth 4`. El total esperado es **197281**.
Para una posición concreta usar `perft --fen "FEN completo" --depth 3` y añadir
`--chess960` cuando corresponda. El enroque Chess960 del ejemplo se imprime g1h1.

Sin argumentos se inicia UCI; con `--help` se muestra la ayuda de perft. `perft` sin opciones usa
posición inicial y profundidad 4. `--startpos` y `--fen` son excluyentes; cada
opción se admite una vez. La profundidad es un entero de 32 bits: igual que el
original, cero o negativos cuentan jugadas legales de la raíz, no una hoja.

Códigos de salida: 0 para éxito/ayuda; 1 para FEN rechazado; 2 para argumentos
incorrectos. Los errores se escriben en stderr. La aplicación ofrece además el bucle UCI esencial descrito más abajo. El tiempo es una observación de ejecución, no un
benchmark controlado. Las pruebas del adaptador y del proceso real están en
el proyecto de [Tests](../Stockfish-sf_19/porting/Diagnostics/Perft.Tests.md).


## Evaluación NNUE de FEN

El comando `eval` carga la red Stockfish 19 oficial y calcula la evaluación
estática, sin búsqueda. El cálculo vive en la biblioteca netstandard2.1.

```powershell
dotnet run --project './Stockfish-sf_19_CSharp_App/Stockfish-sf_19_CSharp_App.csproj' -c Release -- eval --network 'RUTA/nn-1a298aa575a0.nnue' --startpos
dotnet run --project './Stockfish-sf_19_CSharp_App/Stockfish-sf_19_CSharp_App.csproj' -c Release -- eval --network 'RUTA/nn-1a298aa575a0.nnue' --fen '4k3/8/8/8/8/8/P7/4K3 w - - 0 1' --trace
```

La salida normal incluye `info score cp ... wdl ...` y unidades internas. `--trace`
reproduce el diagnóstico original de los ocho buckets y la puntuación final de
blancas. `--optimism N` ajusta la evaluación normal; no se combina con `--trace`,
que conserva optimismo cero. `--chess960` interpreta los derechos de enroque.
`eval --help` muestra la ayuda. Sin --network busca el nombre oficial en el
directorio actual y junto a la aplicación. Los pesos no se incluyen en Git; .gitignore omite los archivos *.nnue.

Evaluate exige ausencia de jaque; trace indica que no hay evaluación si hay
jaque. Los fallos de red/FEN/evaluación devuelven 1 y los argumentos inválidos 2.
El comando independiente eval se conserva junto al bucle UCI y go. Ver
[contratos](../Stockfish-sf_19/porting/Evaluation/NNUE/NnueEvaluation.md) y
[aceptación](../Stockfish-sf_19/porting/Evaluation/NNUE/NnueEvaluation.TestResults.md).

## Motor UCI con trabajadores de búsqueda

Sin argumentos (o con `uci`) la aplicación recibe comandos por stdin y entrega
respuestas por stdout. Los pesos se configuran mediante EvalFile; no se incluyen
en Git ni se descargan automáticamente. Ejemplo interactivo, desde la raíz:

```powershell
dotnet run --project './Stockfish-sf_19_CSharp_App/Stockfish-sf_19_CSharp_App.csproj' -c Release
```

```text
uci
setoption name Hash value 16
setoption name Threads value 2
setoption name EvalFile value RUTA/nn-1a298aa575a0.nnue
isready
position startpos moves e2e4 e7e5
go depth 6
```

Esperar bestmove antes de preparar otra búsqueda; `stop` la interrumpe y `quit`
para y cierra. EOF también para y espera. `go infinite` requiere stop;
`go ponder depth 6` espera ponderhit/stop. Se admiten MultiPV, Skill Level,
UCI_LimitStrength/UCI_Elo, WDL, reloj/incrementos/movestogo, nodos, mate y searchmoves.
El protocolo sigue atendiendo isready durante la búsqueda. Los cambios de Hash,
red, posición y nueva partida detienen y esperan antes de modificar recursos.

Threads anuncia rango 1..max(1024,4*CPU nativas) y recrea el pool/reinicia TT al cambiar. El valor
por defecto sigue siendo uno. Se anuncian SyzygyPath, SyzygyProbeDepth, SyzygyProbeLimit y Syzygy50MoveRule;
NumaPolicy permite auto/system/hardware/none y listas de CPU; SHM y validación de plataformas conservan sus pendientes. Las tablas se proporcionan localmente y no se descargan desde el motor. La interfaz
operativo ya incluye bench/speedtest/compiler/d/flip y registro por instancia. Los errores críticos muestran info string y devuelven 1.
Go perft y eval/export_net están disponibles; los adaptadores previos perft y
`eval --network ...` conservan sus argumentos/códigos. Ver
[contratos](../Stockfish-sf_19/porting/Engine/Engine.md) y
[aceptación](../Stockfish-sf_19/porting/Engine/Engine.TestResults.md).
