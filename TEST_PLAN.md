# Plan y resultados de pruebas

Fecha: 2026-10-16. Entorno Linux x64; SDK .NET 8.0.426, runtime 8.0.32. El repositorio inicial solo contenía un README. Esta entrega contiene el prototipo implementado; la validación ejecutada tiene los límites que se detallan abajo.

## Corrección posterior: bloqueo al iniciar por mapa guardado

La captura del usuario confirmó que Main inicia en Unity 2022.3.62f1, pero la carga terminaba con `InvalidDataException: El mapa no coincide con la semilla y versión.` No implica que el resto del flujo Unity esté validado.

Se reprodujo ese mismo error en el servicio de almacenamiento extraído de LocalSave. Con el filtro de excepciones anterior, tres regresiones fallaron (mapa incompatible con respaldo, ambos mapas inválidos y documento sin run). Al capturar InvalidDataException de forma explícita, las tres pasan. Se añadió formato de archivo v2 para no depender de la serialización inline de un RunState nulo, manteniendo lectura de archivos anteriores y copias de originales rechazados.

Validación actual: **60 pruebas .NET ejecutadas y aprobadas**, 0 fallidas. Incluye las 49 anteriores y 11 casos nuevos de archivos reales: primer inicio, menú sin run tras tres reinicios, placeholder heredado, recuperación de respaldo, ambos archivos inválidos, JSON truncado, versiones futuras protegidas, semilla cero legítima, migración con progreso, run faltante y una run completa restaurada en cada transición. Evidencia: [Validation/save-recovery-tests.txt](Validation/save-recovery-tests.txt).

Validación estática actual: 41 GUIDs y 6 ensamblados; sintaxis C# 9 de 18 archivos sin errores. Los once casos nuevos están conectados al runner EditMode con **JsonUtility real** (59 casos EditMode totales). Su ejecución en Unity sigue pendiente en esta máquina; las pruebas de archivo ejecutadas usan System.Text.Json a través de la misma interfaz, sin sustituir la lógica de almacenamiento o validación.

## Evidencia de la entrega inicial

## Ejecutado y aprobado

Comando: `DOTNET_CLI_HOME=/workspace/tools/dotnet-home bash Tools/test.sh`.

**49 pruebas ejecutadas, 49 aprobadas, 0 fallidas, 0 omitidas.** Salida completa: [Validation/core-tests-2026-10-16.txt](Validation/core-tests-2026-10-16.txt). Se compiló y ejecutó el código real de `Scripts/Core` enlazado al runner .NET; no se probaron copias de su lógica.

| Área | Casos cubiertos |
|---|---|
| Personalidades | Cuatro transformaciones exactas, umbral estricto del miedoso, semántica con prioridad, diferencia entre invención y confusión, bonos |
| Estado del rumor | 500 transformaciones y límites 0–100; entrada inmutable; independencia de atributos, etiquetas, afirmaciones, historial y recorrido entre ramas |
| Simulación | BFS, orden ordinal, mismo resultado al invertir aristas, alcance único, origen incluido, historial explicativo |
| Red | Ciclos, relación inexistente, habitante inexistente, fusión, entrada al origen, exceso de salidas, presupuesto, desconexión, vacío y duplicados; eliminación recursiva y deshacer |
| Evaluación | Ceros de no alcanzados en colectivos, todas las condiciones obligatorias, estrellas y secundarias, soluciones de todos los encuentros y variantes |
| Generación | 1.000 semillas (−500 a 499), reproducibilidad, rutas de seis etapas y alternativas distintas con jefe accesible |
| Runs | 100 runs completas por lógica de dominio, eventos, recompensas, jefe, derrota, reintento y pérdida de paciencia, elección única, reinicio de bonos |
| Replay | Semilla y decisiones reconstruyen run con fracaso, reintento, eventos y victoria; detección de resultado alterado |
| Guardado de dominio | JSON con System.Text.Json, restauración de recompensas/resultados, continuación al jefe, rechazo de versiones/mapas/recorridos inválidos, reconstrucción de historial, no duplicar pérdida de paciencia |

La prueba de 100 runs usa soluciones de referencia del catálogo. No equivale a 100 partidas operadas mediante la interfaz ni a un estudio de balance.

Validación estática: **39 GUIDs, 6 ensamblados, escena, catálogo, fuente y versiones de paquetes comprobados** por `Tools/validate_project.py`. Los archivos `.meta` resuelven las referencias declaradas. Comprobación Roslyn con C# 9: **16 archivos, 0 errores de sintaxis**. Esto no comprueba resolución de tipos contra las DLL de Unity ni importación de assets.

`Tools/bootstrap-dotnet.sh` se ejecutó tanto contra la instalación existente como en una carpeta vacía; descarga HTTPS y SHA-512 del SDK comprobados. Scripts Bash revisados con `bash -n`. El runner preserva su código de salida.

## Hallazgos corregidos durante el desarrollo

- La primera ruta testigo de Asamblea no satisfacía conjuntamente importancia y contraste. Se cambió por una bifurcación vía Beto y Tito; los umbrales de victoria se mantuvieron.
- La comprobación de guardados podía lanzar una excepción al encontrar un encuentro alterado en el mapa. Ahora rechaza mapas incoherentes antes de acceder a sus encuentros, valida el recorrido y reconstruye el resultado con el simulador.
- La cifra esperada en una prueba de agregación era incorrecta: 25 + 39 dividido entre 10 da 6,4. Se corrigió la expectativa.

No quedan fallos conocidos en la suite ejecutada. Esto no demuestra ausencia de defectos en Unity.

## Implementado, todavía no ejecutado en Unity

- Main, arranque, pantalla de inicio, semilla, mapa ramificado, tablero táctil, edición, fichas, pulsos, pausa, velocidad y salto.
- Resultados, elección de recompensas, eventos, derrota/victoria, título cosmético, ajustes y tutorial contextual.
- Avatares, edificios, conexiones con flechas, salto visual y sonido sintetizado.
- Guardado con JsonUtility, escritura temporal/reemplazo, respaldo y bloqueo de escritura de versiones futuras.
- Configuración y comando de compilación Android.
- EditMode: 48 casos compartidos; PlayMode: tres pruebas de flujo UI, guardado/respaldo y preservación de versión futura. **No ejecutadas**: no se atribuyen pases a estos runners.

## Bloqueos de esta máquina

No se encontró Unity Editor, Unity Hub, Android SDK/NDK, `adb` ni un dispositivo Android. No se observó una variable de licencia Unity configurada. No se ejecutó Unity, no se hizo una compilación Android y no se generó un APK.

Las consultas HTTPS a `packages.unity.com` y `services.api.unity.com` respondieron HTTP 403 desde este entorno. Se proponen en la configuración del entorno esos destinos y los hosts de descarga `download.packages.unity.com` y `download.unity3d.com`; guardar un borrador no demuestra que la conectividad haya cambiado.

Se descartó una comprobación auxiliar contra DLLs de una versión diferente de Unity: la validación de firma NuGet no pudo consultar revocación. No se utilizaron esas DLLs ni se deshabilitó verificación. No son una dependencia del proyecto.

## Pasos de aceptación pendientes

1. Instalar y activar Unity 2022.3.62f1 con Android Build Support, SDK/NDK y OpenJDK. Abrir el proyecto, restaurar paquetes y verificar importación sin errores. No actualizar versiones silenciosamente.
2. Ejecutar `Tools/unity.sh editmode` y `Tools/unity.sh playmode` con `UNITY_EDITOR` configurado, o usar Test Runner. Confirmar casos ejecutados y XML del intento actual; corregir errores antes de compilar.
3. Jugar una run manual desde Main. Crear y eliminar bifurcaciones, producir errores de red, fallar/reintentar, elegir recompensas, entrar a ambos eventos y derrotar al jefe. Comparar objetivo y resultado con las fichas.
4. Cerrar/reabrir en construcción, resultado, recompensa y durante pulsos. Comprobar que la paciencia no se devuelve. Probar un archivo dañado y respaldo. Ejecutar la prueba UI sobre una partida de prueba porque reemplaza la run local.
5. Revisar pantalla 720×1280, 1080×1920 y 1080×2400, notch y zona segura, tamaños táctiles, scroll, legibilidad, teclado numérico y botón Atrás. Ajustar espaciado si hay recortes; la adaptación está implementada pero no observada.
6. Ejecutar `Tools/unity.sh android`; instalar el APK en un Android ARM64. Probar toque real, suspensión/reanudación, audio silenciado, guardado persistente y ejecución en modo avión.
7. Medir con Profiler CPU/GPU, GC, memoria, objetos y draw calls en el dispositivo. El tablero tiene 10 avatares y 15 relaciones potenciales por plantilla; no se presentan medidas de rendimiento que no se hayan tomado.
8. Playtesting humano de duración, dificultad, comprensión, diversión y repetición. Los 10–20 minutos son una meta de diseño, no un resultado medido.

## Matriz de aceptación

| Criterio | Estado demostrado |
|---|---|
| Redes ramificadas, personalidades, determinismo, objetivos | Implementados y aprobados en dominio C# |
| Mapa, recompensas, paciencia, run completa, victoria/derrota | Implementados y aprobados en dominio C# |
| Semilla + decisiones reproducibles | Implementado y probado con replay |
| Arranque offline, interfaz cómoda y presentación móvil | Código y escena integrados; ejecución y uso real pendientes |
| Persistencia del dominio | Roundtrip probado en .NET; JsonUtility y sistema de archivos del juego pendientes |
| Compilación sin errores | Núcleo .NET compilado; sintaxis de scripts comprobada; compilación Unity pendiente |
| Android | Configuración y procedimiento incluidos; APK y dispositivo pendientes |
