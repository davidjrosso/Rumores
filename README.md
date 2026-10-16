# Rumores

Prototipo de estrategia de redes y roguelike para Android, en Unity 2D y C#. Construí un árbol de chismes, observá cómo cada personalidad transforma la noticia y llegá a la asamblea antes de agotar la Paciencia del Pueblo.

**Estado:** núcleo implementado y ejecutado en .NET; proyecto Unity integrado en archivos, pendiente de abrir, probar y compilar en el editor. No hay APK generado ni validación en dispositivo. Ver [TEST_PLAN.md](TEST_PLAN.md) para la evidencia y los límites.

## Abrir y jugar

1. Instalar **Unity 2022.3.62f1 LTS** desde Unity Hub, con **Android Build Support**, SDK/NDK y OpenJDK. Activar una licencia válida mediante Unity Hub; no poner credenciales en el repositorio.
2. Agregar esta carpeta como proyecto. Permitir que Package Manager restaure UGUI 1.0.0 y Test Framework 1.1.33.
3. Abrir `Assets/_Rumores/Scenes/Main.unity`. En Game View, usar 720×1280 o una relación vertical de teléfono y pulsar Play.
4. Elegir **Nueva run**, conservar o escribir una semilla y comenzar. Seleccionar un nodo del mapa.
5. Tocar a Nora (• origen) y un destinatario por una relación punteada. Tras agregar un enlace queda seleccionado el destinatario para continuar una cadena. Para bifurcar, seleccionar de nuevo el emisor y otro destino. Tocar un enlace ya creado elimina esa rama y sus descendientes. Deshacer recupera la última edición de la sesión.
6. Cumplir los objetivos, pulsar **Simular**, revisar versiones y elegir una mejora. Los eventos permiten recuperar paciencia o ganar presupuesto. El jefe de la sexta etapa cierra la run.

Se guarda localmente tras cada cambio y antes de animar un resultado. Inicio → Continuar reabre el estado pendiente; cerrar durante una animación muestra directamente el resultado al volver. Fallar siempre resta una paciencia. Reintentar no resta otra hasta volver a fallar. Nueva run reemplaza la run anterior, conservando ajustes y título cosmético.

## Pruebas sin Unity

Requiere .NET SDK 8 y Python 3. No hay paquetes NuGet externos para el núcleo.

```bash
bash Tools/test.sh
```

En este entorno el SDK está instalado en `/workspace/tools/dotnet`. Para reproducir la instalación Linux x64, `bash Tools/bootstrap-dotnet.sh` descarga la versión fijada y comprueba su SHA-512 contra el valor del manifiesto oficial. La variable opcional `DOTNET` selecciona otro ejecutable. `Tools/test.sh` ejecuta pruebas C# reales y validación estática de referencias Unity, sin abrir el motor.

Comprobación adicional de sintaxis C# 9 (no valida tipos/API de Unity):

```bash
dotnet run --project Tools/SyntaxCheck/SyntaxCheck.csproj
```

## Pruebas en Unity

Usar **Window → General → Test Runner**, pestañas EditMode y PlayMode. EditMode envuelve los mismos casos puros; PlayMode carga Main, usa botones, comprueba guardado JsonUtility y recuperación. Ejecutarlas en un perfil de prueba: la prueba de flujo UI reemplaza la run local.

Por consola, con editor y licencia disponibles:

```bash
export UNITY_EDITOR=/ruta/a/Unity
bash Tools/unity.sh editmode
bash Tools/unity.sh playmode
```

El wrapper comprueba resultados XML del intento actual y rechaza ejecuciones vacías. Los logs quedan en `TestResults/` (ignorado por Git). Las pruebas PlayMode usan renderizado UGUI: si el runner sin gráficos no funciona en la máquina, ejecutarlas desde el Test Runner del editor.

## Android

Menú **Rumores → Configurar Android**, luego **Rumores → Compilar APK de desarrollo**. La escena Main ya está registrada. Alternativa:

```bash
export UNITY_EDITOR=/ruta/a/Unity
# Opcional: export RUMORES_APPLICATION_ID=com.tuestudio.rumores
bash Tools/unity.sh android
```

Salida: `Builds/Rumores-dev.apk`. Configuración: vertical, IL2CPP, ARM64, OpenGLES3, mínimo API 23 (Android 6.0), target API automático según SDK instalado, application ID `com.davidjrosso.rumores`, versión 0.1.0, firma de depuración. El mínimo es una decisión del prototipo; no implica cumplimiento de requisitos de Google Play. No requiere servicios online durante el juego.

Con un dispositivo ARM64 conectado y depuración USB habilitada, `adb install -r Builds/Rumores-dev.apk`. La generación del APK y este comando **no se ejecutaron** en la entrega actual.

## Contenido y arquitectura

- `Assets/_Rumores/Scripts/Core`: simulador, redes, objetivos, runs, recompensas, validación y replay, sin UnityEngine.
- `Assets/_Rumores/Scripts/Presentation`: interfaz UGUI, avatares/escenario procedural, pulsos, audio y guardado local.
- `Assets/_Rumores/Resources/Content.json`: personajes, noticias, reglas, relaciones, seis plantillas y mejoras, conectado a `RumoresCatalog.asset`.
- `Assets/_Rumores/Scripts/Editor`: validación y compilación Android.
- `Assets/_Rumores/Tests`: pruebas compartidas, EditMode y PlayMode; `Tests/` es el runner .NET.

Diseño: [GDD.md](GDD.md), [TECHNICAL_DESIGN.md](TECHNICAL_DESIGN.md), [BALANCE.md](BALANCE.md). Resultados y comprobaciones pendientes: [TEST_PLAN.md](TEST_PLAN.md).

Se usan archivos de escena y datos versionados; la UI, los avatares y los sonidos se generan al iniciar Main. No hacen falta prefabs descargados ni assets de terceros salvo DejaVu Sans, cuya licencia se incluye. Las partidas se guardan en `Application.persistentDataPath/rumores-save.json`, con respaldo `.bak`, formato de archivo 2 y datos de juego versión 1. Los guardados anteriores compatibles se migran al escribir. La semilla más `run.decisions` puede reproducirse con `RunReplay.Play` usando el mismo catálogo y versión.

## Si aparece «El mapa no coincide con la semilla y versión»

Actualizar el proyecto desde `main` incluye la corrección de carga. Salir de Play Mode y cerrar Unity antes de reemplazar archivos, o descargar el ZIP y abrir la carpeta nueva desde Unity Hub. Abrir Main y pulsar Play.

La carga ahora intenta el respaldo cuando un mapa es inválido. Si ambos archivos son incompatibles, vuelve al menú sin una run activa. Los archivos rechazados se conservan junto al guardado con sufijo `.invalid-<id>`. El nuevo formato distingue explícitamente «sin partida» de una run activa para evitar mapas vacíos creados por la serialización inline. Las versiones futuras permanecen protegidas contra escritura.

Esta corrección se reprodujo y verificó con pruebas del almacenamiento real en .NET; sus casos con JsonUtility están incluidos en EditMode y requieren ejecutarse en Unity.
