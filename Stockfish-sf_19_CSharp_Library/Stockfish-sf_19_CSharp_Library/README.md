# Stockfish 19 Core

Motor independiente de UnityEngine, C# 8/.NET Standard 2.1,
GPL-3.0-or-later. Este directorio sigue siendo el proyecto .NET usado por la
consola Windows y las pruebas. El paquete Unity se genera desde estas mismas
fuentes, sin un segundo fork del motor ni DLL versionada.

Exportar/instalar junto con el adaptador:
https://github.com/MauricioCortes-dev/Stockfish-19-C-y-C-port/blob/main/Unity/README.md

La red NNUE se obtiene por separado. Este paquete declara unsafe y no depende de
UnityEngine ni paquetes NuGet externos. Sus consumidores Unity deben añadir una
referencia explícita a `Stockfish-sf_19_CSharp_Library` en su asmdef.
