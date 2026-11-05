\# Stockfish 19 — C# Port



\[English](#english) | \[Español](#español)



\## English



C# port of the Stockfish 19 chess engine, organized as an independent library and a console application that exposes the engine through the UCI protocol.



The project aims to preserve the original engine’s behavior while prioritizing readability, maintainability, and integration into .NET applications.



This is an independent project, not an official Stockfish distribution.



\### Repository contents



\- \*\*C# library:\*\* board representation, move generation, search, NNUE evaluation, transposition tables, time management, and supporting services.

\- \*\*Console application:\*\* integration of the library with a UCI interface for terminal use and compatible chess graphical interfaces.



This repository contains source code. Compiled executables, NNUE networks, and Syzygy tablebases are not included.



\### Requirements



To build and run the application:



\- .NET SDK 10.

\- An NNUE network compatible with Stockfish 19.



The library targets:



\- C# 8.

\- .NET Standard 2.1.

\- Unsafe code support for specific memory operations.



The console application targets .NET 10.



Syzygy tablebases are optional.



\### Project structure



```text

Stockfish-sf\_19\_CSharp\_Library/

└── Stockfish-sf\_19\_CSharp\_Library/

&#x20;   └── Engine library source code



Stockfish-sf\_19\_CSharp\_App/

└── Console application and UCI interface

```



The library does not depend on the console application or Unity.



\### Building



From the repository root:



```bash

dotnet build Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release

```



Building the application also builds its referenced engine library.



To build only the library:



```bash

dotnet build Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library.csproj -c Release

```



Use \*\*Release\*\* builds for normal engine operation and performance comparisons.



\### NNUE network



Evaluation requires a network compatible with this engine version. Networks intended for other Stockfish versions may have incompatible formats or architectures.



Official networks are available through the Stockfish project:



\- https://stockfishchess.org/

\- https://tests.stockfishchess.org/nns



Configure the network path through the UCI `EvalFile` option:



```text

setoption name EvalFile value C:\\Networks\\compatible-network.nnue

```



The same option can be configured through a chess GUI that supports UCI engine settings.



\### Running from a terminal



```bash

dotnet run --project Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release

```



After the application starts, enter UCI commands. For example:



```text

uci

setoption name EvalFile value C:\\Networks\\compatible-network.nnue

setoption name Threads value 1

setoption name Hash value 32

setoption name Ponder value false

isready

position startpos

go depth 16

```



The engine reports search progress through `info` messages and returns its selected move through `bestmove`.



To stop a search:



```text

stop

```



To close the application:



```text

quit

```



\### Using a chess GUI



The console application can be used with UCI-compatible interfaces such as Arena or Cute Chess.



1\. Build or publish the application in Release mode.

2\. Add its executable as a UCI engine.

3\. Set `EvalFile` to the compatible network’s path.

4\. Configure `Threads`, `Hash`, and other options according to available resources.



To publish a self-contained Windows x64 application:



```bash

dotnet publish Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64

```



The executable will be located in `publish/win-x64`.



Keep the complete published directory together. Configure or distribute the NNUE network separately, subject to its license.



\### Integrating the library



Reference the library project from a compatible application:



```xml

<ItemGroup>

&#x20; <ProjectReference Include="../Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library.csproj" />

</ItemGroup>

```



The consuming project must support .NET Standard 2.1.



The included console application provides an integration example for engine configuration, search, and result handling.



Applications embedding the engine must manage:



\- Network loading and availability.

\- Search startup and cancellation.

\- Events and results produced by worker threads.

\- Resource reloading.

\- Deterministic disposal of engine resources.



Graphical applications must dispatch worker-thread events to their UI thread before updating interface controls.



\### Compatibility and Unity



The library uses C# 8/.NET Standard 2.1 and does not depend on Unity APIs.



These choices facilitate integration, but they do not establish compatibility with every runtime or platform. Unity Mono and IL2CPP require separate runtime validation, particularly for threading, memory, native services, and application lifecycle management.



Operating-system-dependent features may use functional alternatives according to the capabilities available on the host.



\### Performance and correspondence with C++



The goal is to preserve the original engine’s functionality and results. Performance may differ depending on the runtime, build configuration, processor architecture, and selected execution paths.



For meaningful comparisons, keep these conditions consistent:



\- Starting position and search completion criterion.

\- NNUE network.

\- Worker count.

\- Transposition-table size.

\- Ponder and Syzygy settings.

\- Execution environment and Release configuration.



Analyze nodes per second together with total nodes, evaluation, and principal variation. A speed difference alone does not establish a functional difference.



\### Original project and attribution



Stockfish is a free chess engine developed by its many contributors.



\- Original repository: https://github.com/official-stockfish/Stockfish

\- Official website: https://stockfishchess.org/



This port derives from Stockfish 19. Original copyright notices and attribution are retained.



\### License



Code derived from Stockfish is distributed under the \*\*GNU General Public License, version 3 or later — GPL-3.0-or-later\*\*.



See \[LICENSE](LICENSE) for the full license text.



Distribution of modified source code or binaries must comply with the license, including providing the corresponding source code where required.



NNUE networks and Syzygy tablebases are obtained separately and must be used and redistributed according to their respective licenses.



\---



\## Español



Port del motor de ajedrez Stockfish 19 a C#, organizado como una biblioteca independiente y una aplicación de consola que expone el motor mediante el protocolo UCI.



El proyecto busca conservar el comportamiento del motor original, dando prioridad a la legibilidad, la mantenibilidad y la integración en aplicaciones .NET.



Este es un proyecto independiente, no una distribución oficial de Stockfish.



\### Contenido del repositorio



\- \*\*Biblioteca C#:\*\* representación del tablero, generación de jugadas, búsqueda, evaluación NNUE, tablas de transposición, administración del tiempo y servicios asociados.

\- \*\*Aplicación de consola:\*\* integración de la biblioteca con una interfaz UCI para utilizar el motor desde terminal o desde interfaces gráficas de ajedrez compatibles.



Este repositorio contiene código fuente. No incluye ejecutables compilados, redes neuronales NNUE ni tablas de finales Syzygy.



\### Requisitos



Para compilar y ejecutar la aplicación:



\- .NET SDK 10.

\- Una red neuronal NNUE compatible con Stockfish 19.



La biblioteca utiliza:



\- C# 8.

\- .NET Standard 2.1.

\- Código `unsafe` para determinadas operaciones de memoria.



La aplicación de consola utiliza .NET 10.



Las tablas Syzygy son opcionales.



\### Organización del proyecto



```text

Stockfish-sf\_19\_CSharp\_Library/

└── Stockfish-sf\_19\_CSharp\_Library/

&#x20;   └── Código fuente de la biblioteca del motor



Stockfish-sf\_19\_CSharp\_App/

└── Aplicación de consola e interfaz UCI

```



La biblioteca no depende de la aplicación de consola ni de Unity.



\### Compilación



Desde la raíz del repositorio:



```bash

dotnet build Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release

```



La compilación de la aplicación también compila la biblioteca referenciada.



Para compilar únicamente la biblioteca:



```bash

dotnet build Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library.csproj -c Release

```



Utiliza compilaciones \*\*Release\*\* para ejecutar el motor y realizar comparaciones de rendimiento.



\### Red neuronal NNUE



La evaluación requiere una red compatible con esta versión del motor. Las redes de otras versiones de Stockfish pueden tener formatos o arquitecturas incompatibles.



Las redes oficiales pueden obtenerse desde los recursos del proyecto Stockfish:



\- https://stockfishchess.org/

\- https://tests.stockfishchess.org/nns



Configura la ruta de la red mediante la opción UCI `EvalFile`:



```text

setoption name EvalFile value C:\\Redes\\red-compatible.nnue

```



También puede configurarse desde una interfaz gráfica que permita editar las opciones UCI del motor.



\### Ejecución desde terminal



```bash

dotnet run --project Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release

```



Una vez iniciada la aplicación, introduce comandos UCI. Por ejemplo:



```text

uci

setoption name EvalFile value C:\\Redes\\red-compatible.nnue

setoption name Threads value 1

setoption name Hash value 32

setoption name Ponder value false

isready

position startpos

go depth 16

```



El motor comunica el progreso de la búsqueda mediante mensajes `info` y publica la jugada seleccionada mediante `bestmove`.



Para detener una búsqueda:



```text

stop

```



Para cerrar la aplicación:



```text

quit

```



\### Uso desde una interfaz gráfica de ajedrez



La aplicación puede utilizarse desde interfaces compatibles con UCI, como Arena o Cute Chess.



1\. Compila o publica la aplicación en Release.

2\. Añade el ejecutable como un motor UCI.

3\. Configura `EvalFile` con la ruta de la red compatible.

4\. Ajusta `Threads`, `Hash` y las demás opciones según los recursos disponibles.



Para publicar una aplicación Windows x64 que incluya el runtime:



```bash

dotnet publish Stockfish-sf\_19\_CSharp\_App/Stockfish-sf\_19\_CSharp\_App.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64

```



El ejecutable se encontrará en `publish/win-x64`.



Conserva el contenido completo de la carpeta publicada. Configura o distribuye la red NNUE por separado, respetando su licencia.



\### Integración de la biblioteca



Añade una referencia al proyecto de biblioteca desde una aplicación compatible:



```xml

<ItemGroup>

&#x20; <ProjectReference Include="../Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library/Stockfish-sf\_19\_CSharp\_Library.csproj" />

</ItemGroup>

```



El proyecto consumidor debe admitir .NET Standard 2.1.



La aplicación de consola incluida sirve como ejemplo de configuración del motor, ejecución de búsquedas y recepción de resultados.



Las aplicaciones que integren el motor deben gestionar:



\- La carga y disponibilidad de la red neuronal.

\- El inicio y la detención de búsquedas.

\- Los eventos y resultados emitidos desde trabajadores.

\- La recarga de recursos.

\- La liberación determinista de los recursos del motor.



En aplicaciones gráficas, los eventos recibidos desde trabajadores deben trasladarse al hilo de la interfaz antes de actualizar sus controles.



\### Compatibilidad y Unity



La biblioteca utiliza C# 8/.NET Standard 2.1 y no depende de APIs de Unity.



Estas decisiones facilitan la integración, pero no acreditan compatibilidad con todos los runtimes o plataformas. Unity Mono e IL2CPP requieren una validación propia, especialmente para hilos, memoria, servicios nativos y ciclo de vida de la aplicación.



Las funciones que dependen del sistema operativo pueden utilizar alternativas funcionales según las capacidades disponibles en el entorno.



\### Rendimiento y correspondencia con C++



El objetivo es conservar la funcionalidad y los resultados del motor original. El rendimiento puede diferir según el runtime, la configuración de compilación, la arquitectura del procesador y las rutas de ejecución seleccionadas.



Para realizar comparaciones útiles, mantén constantes:



\- La posición inicial y el criterio de finalización de la búsqueda.

\- La red neuronal.

\- El número de trabajadores.

\- El tamaño de la tabla de transposiciones.

\- Las opciones de ponder y Syzygy.

\- El entorno de ejecución y la configuración Release.



Analiza los nodos por segundo junto con los nodos totales, la evaluación y la variante principal. Una diferencia de velocidad, por sí sola, no demuestra una diferencia funcional.



\### Proyecto original y atribución



Stockfish es un motor de ajedrez libre desarrollado por sus numerosos colaboradores.



\- Repositorio original: https://github.com/official-stockfish/Stockfish

\- Sitio oficial: https://stockfishchess.org/



Este port deriva de Stockfish 19. Se conservan los avisos de copyright y las atribuciones correspondientes.



\### Licencia



El código derivado de Stockfish se distribuye bajo la \*\*GNU General Public License, versión 3 o posterior — GPL-3.0-or-later\*\*.



Consulta \[LICENSE](LICENSE) para conocer el texto completo.



La distribución de código modificado o de binarios debe cumplir las obligaciones de la licencia, incluida la disponibilidad del código fuente correspondiente cuando proceda.



Las redes neuronales NNUE y las tablas Syzygy se obtienen por separado y deben utilizarse y redistribuirse conforme a sus respectivas licencias.

