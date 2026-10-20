# Fase 4 · UI, audio y pulido

20 de octubre de 2026. El usuario confirmó **mantener el modo sencillo**: no se reincorporaron puntos, ganadores ni ramas visibles. El contenido de Core y `PartyContent.json` permanece idéntico a `e370c81`.

## Cambios en la experiencia

- **Inicio:** marca editable, pueblo existente con un paseo visual suave, botones amplios y entrada por fundido. El modo de movimiento reducido detiene el paseo.
- **Grupo:** resumen de amigos/turnos, filas de mayor altura, apodos de una sola línea, avatares existentes y botón Quitar con etiqueta. Cambiar avatar mantiene la posición del listado; agregar lleva al nuevo participante. Los errores identifican apodos vacíos o repetidos.
- **Teclado:** cuando Unity informa el área del teclado, se eleva el borde inferior de la lista y se desplaza el campo enfocado para mantenerlo visible. Algunos teclados Android no informan esa área: ese caso requiere prueba en dispositivo; no se inventa una altura fija.
- **Turno:** jugador y avance siempre visibles. Rumor, pregunta y tres tarjetas están en una lista desplazable. Cada tarjeta muestra título, icono, explicación y vista previa exacta. El texto crece con su contenido, en lugar de reducirse para caber en una caja fija.
- **Selección:** marca textual «✓ Elegida», título destacado y nombre de la acción en la confirmación fija. La selección no se comunica solo por color. El botón conserva su ubicación al desplazar la lista.
- **Privacidad:** al entregar el teléfono o volver de una pausa durante la partida aparece una pantalla con el apodo, sin mostrar el chisme. «Estoy listo» revela el contenido; una pausa no consume turno ni borra la selección.
- **Protección:** la pantalla anterior se desactiva inmediatamente; el fundido no mezcla información de dos jugadores. CanvasGroup y callbacks bloquean entradas durante 0,18 s (0,06 s en modo reducido). Después se puede avanzar sin esperar las reacciones del personaje. Se mantienen versión de pantalla y token del dominio contra doble confirmación/avance.
- **Revelación:** texto completo desplazable, explicación del cambio, retrato y transmisión. El pie con el siguiente paso queda fijo.
- **Cierre:** original, final destacado, pregunta social y secuencia de aportes con autor y explicación. Confeti y sonido breve al finalizar; volver del historial no repite la fanfarria. Revancha, historial, cambio de grupo y menú tienen controles diferenciados.

## Legibilidad y traducción

El Canvas principal escala por ancho, con referencia de 720 unidades. Los textos de lectura usan 32–36 unidades (16–18 px a 360 px de ancho); ayudas secundarias usan 28. Los botones de pie ocupan el 10% del área de página, con separación y zonas seguras. El color de selección se oscureció para sostener el contraste del texto claro. Las listas de ayuda, opciones, revelación, historial y resultados calculan altura según texto.

`Resources/UiStrings.es-AR.json` centraliza **89 textos de interfaz** del modo actual y la galería. `UiStrings.T` usa claves estables y parámetros `{0}`, `{1}`. Los apodos se muestran sin interpretar etiquetas rich text. Noticias, títulos y explicaciones de las acciones ya están configurados en `PartyContent.json`: una futura traducción también debe traducir ese contenido. No se añadió selector de idioma ni se tradujo el modo Legacy.

## Audio real y procedencia

No se encontró generador de audio disponible ni audio previo en el repositorio. Se crearon **ocho WAV sintetizados con Python**, sin IA, grabaciones, voces ni muestras de terceros. Son recursos provisionales originales, con licencia **CC0-1.0** en `Resources/Audio/LICENSE.txt`. El script reproducible es `Tools/generate_audio.py`.

| Archivo | Duración | Uso |
|---|---:|---|
| Music.wav | 20 s, loop | Motivo instrumental ligero con notas pulsadas |
| Select.wav | 0,10 s | Botones y selección |
| Transmit.wav | 0,36 s | Transmisión del chisme |
| Surprise.wav | 0,34 s | Reacción tras la revelación |
| Panic.wav | 0,44 s | Reacción a Hacerse la cabeza |
| Reveal.wav | 0,64 s | Aparición del resultado final |
| Celebrate.wav | 0,72 s | Cierre de la ronda |
| Defeat.wav | 0,64 s | Desánimo en ArtPreview; no hay derrota en Main |

Los clips son mono PCM16 a 44,1 kHz, con entradas/salidas suaves y margen frente al clipping. Unity los importa sin compresión y `PartyAudio` los reproduce en dos fuentes 2D, una para música y otra para efectos. La fuente de efectos limita repetición y corta reacciones al cambiar de pantalla. La música se pausa al dejar la aplicación.

**Ajustes** tiene sliders independientes para música y efectos; 0 silencia. Las preferencias se guardan al salir de ajustes, pausar o destruir la escena. Valores iniciales: música 22%, efectos 50%. La galería comparte esos ajustes y permite probar sorpresa, pánico, transmisión, celebración y desánimo. Las comprobaciones de señal no sustituyen una escucha con auriculares y altavoz del teléfono.

## Validación ejecutada en cloud

- `Tools/test.sh`: **83 pruebas .NET, 0 fallos**, incluidas las rondas simuladas y contratos anteriores; 89 GUIDs/6 assemblies; controles de cinco PNG y metadatos.
- `Tools/validate_ux.py`: 89 claves de interfaz, parámetros y referencias; ocho WAV con formato/duración, señal no vacía, picos sin saturación, offset DC bajo y extremos silenciosos. Picos observados entre 0,103 y 0,192, RMS entre 0,028 y 0,050.
- Análisis de sintaxis C# 9: **42 archivos, cero errores**. No compila las APIs de Unity.
- Core y noticias sin cambios; `git diff --check` sin errores.

**No ejecutado:** Unity Editor, importación real de audio, compilación contra Unity, EditMode/PlayMode, escucha en el motor, navegación renderizada ni revisión visual en dispositivos. El entorno no tiene Unity instalado. La confirmación del usuario sobre el arte previo no certifica estos cambios.

## Pruebas Unity preparadas

Se conservan los recorridos de dos/ocho participantes, regresión CanvasRenderer, historia, revancha y arte. Ahora esperan la transición breve antes de pulsar, y verifican por separado que una pulsación temprana sea rechazada.

Se agregaron **17 casos**: ocho importaciones de audio, formato localizado con apodos especiales, privacidad/pausa/doble pulsación, volúmenes independientes/persistencia, contenido largo con pie fijo, y cinco tamaños de layout: **360×640, 360×800, 720×1080, 720×1280 y 800×1280**, con margen de zona segura. Los casos de tamaño usan componentes reales de texto/layout en un Canvas de prueba; no son capturas ni una prueba completa del dispositivo.

## Recorrido manual pendiente

1. Abrir **Main.unity** en Unity 2022.3.62f1. Recorrer Jugar, Cómo jugar y Ajustes. Probar música y efectos por separado, silencio a 0 y movimiento reducido. Volver a abrir y comprobar persistencia.
2. Configurar dos y ocho amigos; editar el último apodo con teclado abierto, probar espacios, duplicados y 16 caracteres. Cambiar avatares y quitar/agregar personas sin perder el campo o la posición de la lista.
3. Jugar una ronda completa: desplazar las tres cartas, alternar opciones, ver la explicación, seleccionar, abrir Lo anterior, volver y confirmar. La versión revelada debe coincidir con la elegida.
4. Pulsar rápido y repetido durante las transiciones; nunca debe consumir dos turnos. Después del fundido, avanzar durante una reacción debe funcionar.
5. Pausar en mitad de una elección y regresar. No debe verse el chisme hasta Estoy listo; selección y número de turno deben conservarse.
6. Revisar original, final, autores y cambios en resultados. Probar Paso a paso, revancha inmediata, cambiar amigos y menú. No deben quedar sonidos o efectos de una pantalla anterior.
7. Repetir los tamaños indicados en Game View y simular un notch. Comprobar títulos largos, texto completo, áreas de toque y ausencia de superposición con el arte. Probar el teclado y el audio en un teléfono cuando corresponda.
8. Ejecutar **Window → General → Test Runner → EditMode y PlayMode → Run All**. Guardar XML del intento y errores completos de Console si hay fallos.

## Archivos principales

- `PartyApp.cs`, `PartyActionCard.cs`, `PartyLayout.cs`: pantallas, listas, tarjetas y controles.
- `PageTransition.cs`, `TownAmbience.cs`: transiciones y fondo animado.
- `PartyAudio.cs`, `Resources/Audio/*`: reproducción, preferencias y recursos sonoros.
- `UiStrings.cs`, `Resources/UiStrings.es-AR.json`, `ArtPreview.cs`: textos centralizados y galería con audio.
- `PolishTests.cs`, `PolishImportTests.cs`, `FlowTests.cs`, `ArtPresentationTests.cs`: pruebas del flujo y regresiones.
- `Tools/generate_audio.py`, `Tools/validate_ux.py`, `Tools/test.sh`: generación reproducible y controles.

La integración reutiliza Main, el prefab de cartas y el catálogo de arte. No requiere conectar referencias manualmente. No hay multijugador online, nuevas reglas ni preparación final de Android.
