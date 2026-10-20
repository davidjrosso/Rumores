# Fase 5 — auditoría y preparación Android

Fecha: **2026-10-20**. Base inspeccionada: `8c9f076`. Entorno Linux con .NET SDK 8.0.426 y Python; sin editor Unity/Hub, Android SDK/NDK, adb ni Gradle disponibles. Hay Java 21, que no sustituye la instalación Android de Unity.

## Alcance y hallazgos

Se mantuvieron sin cambios los scripts de gameplay/presentación, escenas, prefab y contenido. Se conserva el modo sencillo confirmado por el usuario: sin clasificación, ganador ni ramas en pantalla. No se agregó guardado de partidas.

| Área | Inspección / resultado | Límite |
|---|---|---|
| C# | 43 archivos con sintaxis C# 9 válida; reglas puras compiladas y probadas con .NET | No compilación de tipos/API Unity ni IL2CPP |
| Escenas/prefab/componentes | Main enlaza PartyApp, catálogo, arte y PartyActionCard; 92 GUID sin colisiones o referencias externas sin resolver, 6 assemblies; Main es la única escena de build | Importación real y ciclo de vida de componentes pendientes |
| CanvasRenderer | Se mantienen RequireComponent y pruebas de reconstrucción de avatares con máscaras | No se reprodujo el antiguo error en Unity aquí |
| Reglas/turnos | 23 contratos party: 2/8 participantes, dos vueltas, opciones/preview, tokens, seis estilos, historial, revancha e independencia interna de ramas | Pruebas de UI/táctiles requieren Unity |
| Puntuación | No aplica a Main por decisión previa. Se conservaron los 60 casos del legado, con sus objetivos/estrellas | No se reintrodujeron puntuaciones ni se afirma probar una clasificación inexistente |
| Guardado | Main mantiene ronda en memoria; tres preferencias separadas; almacenamiento del legado con recuperación/validación probado | PlayerPrefs y cierre real del proceso pendientes en Android |
| UI/audio | Safe area, escala por ancho, scroll, bloqueo por transición/versión/token, pausa privada y volúmenes independientes presentes; 89 textos y 8 WAV válidos | No se escuchó la app ni se renderizaron pantallas en este entorno |
| Recursos | Cinco ilustraciones con dimensiones/alfa correctos, nuevo icono opaco, metadatos de importación revisados, audio CC0 y licencia de fuente conservados | Launcher, compresión real, memoria y aspecto final pendientes |
| Paquetes | Manifest con versiones fijas; no plugins Android nativos añadidos | No hubo resolución UPM; falta lock generado por Unity |
| Conexiones | Búsqueda de APIs de red en scripts sin dependencias de gameplay; recursos locales y permiso Internet no forzado | Inspeccionar manifiesto final y jugar en modo avión |
| Android | Retrato, ARM64/IL2CPP, escena Main, nombre/ID, versión, icono y rutas APK/AAB preparados | No build, instalación, firma ni medición de rendimiento ejecutadas |

## Errores corregidos

1. **Falso éxito con APK vieja.** El wrapper anterior solo comprobaba si existía un archivo al terminar. Se reprodujo en una carpeta temporal usando un ejecutable que sale con código 0 sin crear archivos: aceptó un marcador anterior como salida válida. Ahora elimina la salida anterior, exige una nueva no vacía, conserva el código de error y elimina salidas parciales. Los marcadores de esta prueba no son APK y se borran al finalizar.
2. **Configuración que sobrescribía versión e identificador.** Por inspección, `ConfigureAndroid` fijaba siempre `0.1.0`, el ID por defecto y Target API Auto. Ahora conserva los valores existentes salvo overrides explícitos, y valida todos los overrides antes de aplicar ajustes. Siete pruebas .NET comprueban preservación, límites y entradas inválidas, incluidos saltos de línea.
3. **Firma personalizada desactivada al configurar.** Por inspección, el configurador siempre ponía `useCustomKeystore=false` y forzaba APK. Ahora configurar no toca esas opciones; la APK usa temporalmente firma de depuración y la restaura con `finally`. El nuevo AAB exige firma personalizada y API explícita. Su integración con Unity todavía no se ejecutó.
4. **Cambio de plataforma dentro de la compilación.** Se sustituyó por una precondición clara: seleccionar Android antes de ejecutar el menú, o `-buildTarget Android` en batch. Evita intentar continuar un método durante una posible recarga de scripts.

No se detectó un nuevo fallo reproducible en las reglas puras con los casos ejecutados. Esto no descarta fallos de Unity, dispositivo, layout, rendimiento o contenido humorístico.

## Recursos y pruebas agregados

- Icono `Assets/_Rumores/Art/AppIcon.png`, generado realmente con la herramienta de imágenes en esta fase; 1254×1254, opaco, globos de diálogo, sin texto incrustado. Integrado en Player Settings y en el configurador. No representa una captura de juego o build.
- `AndroidBuildConfig.cs`: validación de configuración de compilación sin dependencias Unity, separada de las reglas del juego.
- 7 casos .NET de configuración y 8 casos Python del wrapper (proceso ficticio, sin simular validación de APK).
- 3 casos Unity nuevos **sin ejecutar**: icono importado/asignado, pausa→menú→ajustes→continuación durante revelación, y nueva partida que descarta la ronda previa y callbacks viejos.
- [ANDROID_RELEASE.md](../ANDROID_RELEASE.md): instalación del editor/módulos, APK, prueba ARM64, firma manual AAB, fuentes oficiales a verificar y pasos previos a publicación.

## Resultados de esta ejecución

| Comando / comprobación | Resultado real |
|---|---|
| `DOTNET=/workspace/tools/dotnet/dotnet DOTNET_CLI_HOME=/workspace/tools/dotnet-home bash Tools/test.sh` | **90 .NET aprobadas, 0 fallidas**: 23 party + 60 legado + 7 configuración; validadores de referencias/PNG/WAV/textos aprobados; **8 Python aprobadas** |
| `dotnet run --project Tools/SyntaxCheck/SyntaxCheck.csproj` usando ese SDK | 43 archivos, 0 errores de sintaxis |
| `bash -n Tools/unity.sh Tools/test.sh` | Aprobado |
| `git diff --check` | Aprobado |
| Comparación de Core, Presentation, Resources, Scenes y Prefabs contra la base | Sin modificaciones de gameplay/presentación existentes |
| Unity EditMode / PlayMode | **No ejecutado** |
| APK / AAB / dispositivo / emulador | **No ejecutado; artefactos inexistentes** |
| Consulta HTTPS de documentación oficial Google/Unity | **Bloqueada por proxy HTTP 403**; requisitos actuales no verificados |

Las 1296 rondas y los controles de coherencia del contenido forman parte de los contratos party existentes, reejecutados ahora. Las cinco relaciones/tamaños de pantalla están cubiertos por tests de layout escritos para Unity, **todavía no ejecutados**.

## Pendientes para aceptar una versión Android

Completar la matriz de [TEST_PLAN.md](../TEST_PLAN.md), resolver importación/compilación Unity, revisar renderizado/audio/teclado/safe areas, medir el comportamiento en un teléfono y verificar la firma/manifiesto/librerías de una build real. Confirmar políticas actuales y parche del editor con las fuentes oficiales antes del AAB de publicación. La fase deja el camino de compilación preparado; **no entrega todavía una versión Android validada**.
