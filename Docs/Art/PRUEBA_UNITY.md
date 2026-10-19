# Fase 3 · muestra integrada, pendiente de validación en Unity

Fecha: 2026-10-19. Esta entrega prepara la **prueba representativa previa a la producción completa**. No declara terminada la fase 3: el entorno cloud no tiene Unity Editor y todavía no se comprobó la apariencia dentro del motor.

## Qué existe

| Recurso | Implementación real | Estado |
|---|---|---|
| Dirección visual | [Paleta, siluetas, perspectiva, escala y movimiento](DIRECCION_VISUAL.md) | Definida; pendiente de validar |
| Pueblo | PNG original generado con `image_gen.imagegen`, plaza, kiosco, club y estación | Muestra; fondo completo, no edificios separados |
| Miedoso | PNG original generado con la misma herramienta, vecino con mate, alfa real | Muestra; un solo dibujo, sin fotogramas faciales |
| Otros tres personajes | Avatares geométricos anteriores | Provisionales; sprites nuevos pendientes |
| Avatares | Recorte de presentación del miedoso; los restantes conservan sus caras anteriores | Mixtos; comprobar lectura a tamaño pequeño |
| Marca | Dos globos vectoriales originales y título editable RUMORES | Marca provisional; no imagen de letras generada por IA |
| Espera, habla, sorpresa, pánico, risa, celebración y desánimo | `CharacterMotion`: escala, giro y desplazamiento con tiempo independiente de `timeScale` | Código integrado; ejecución Unity pendiente |
| Transmisión / división | Globos breves que recorren una trayectoria, dos destinos diferenciados al dividir | Transmisión conectada a la ronda; división solo en escena de revisión |
| Celebración / derrota | Movimientos del retrato | Festejo al terminar; desánimo solo en revisión. No hay ganador ni derrota en las reglas actuales |
| Iconos de acción y personalidad, confeti y otros adornos | Se conservan títulos y avatares existentes | Conjunto final pendiente |
| Menú, tarjetas y paneles | UI existente con fondo nuevo, marca y retratos conectados | Presentación parcial; textos siguen siendo editables |

No se generaron fotogramas de animación: los movimientos usan el mismo sprite y también funcionan sobre el avatar geométrico de respaldo. No se añadieron reglas, puntuación, controles de ramas, noticias ni servicios online. Core y `PartyContent.json` permanecen idénticos a `cf2236a`.

## Abrir la muestra y la partida

1. Abrir el proyecto con **Unity 2022.3.62f1** y esperar la importación.
2. Abrir `Assets/_Rumores/Scenes/ArtPreview.unity`, Game View **720×1280**, pulsar Play. Esta escena es de revisión; no se agregó a Build Settings.
3. Verificar pueblo, personaje completo, avatar recortado y avatar provisional. Probar **Espera, Hablar, Sorpresa, Pánico, Risa, Festejo, Desánimo, Transmitir y Dividir**. Probar **Reducir movimiento**. El botón no cambia las preferencias de la partida.
4. Detener Play y abrir `Assets/_Rumores/Scenes/Main.unity`. El fondo y la marca aparecen en el menú; **Jugar** abre la lista con el nuevo avatar. El primer avatar es el miedoso; cambiarlo y volver a él para comprobar ambas representaciones.
5. Jugar una ronda de dos amigos; observar el personaje al pasar el teléfono, el habla y la reacción al confirmar, la transmisión y el festejo final. Tocar **Pasar el celu** inmediatamente durante la animación: debe responder. Repetir con ocho participantes, desplazar la lista y pulsar **¡Otra!**.
6. Repetir a **360×800** y **720×1080**. Confirmar que los textos y botones no quedan tapados, que el retrato no invade otra fila y que no aparece `MissingComponentException` ni otro error en Console.
7. Ejecutar **Window → General → Test Runner → EditMode → Run All** y después **PlayMode → Run All**. Guardar resultados y capturas de menú, lista, turno y reacciones antes de ampliar la producción.

La condición de avance es ver la muestra correctamente **dentro de Unity**, como pide la fase. Un PNG visto fuera del motor, un GUID resuelto o una prueba .NET no cumplen esa condición. Si algo falla, registrar resolución, captura, error completo de Console y reacción utilizada.

## Importación preparada

| PNG en `Assets/_Rumores/Resources/Art/Preview` | Fuente | Pivote normalizado | Alfa |
|---|---|---|---|
| `Miedoso.png` | 1024×1536 RGBA | (0,5; 0) | 58,36% de píxeles totalmente transparentes; cuatro esquinas transparentes |
| `Pueblo.png` | 1536×1024 RGB | (0,5; 0,5) | Opaco |

Los `.meta` configuran **Sprite / Single**, 100 PPU, sRGB, Bilinear, Clamp, sin mipmaps, sin compresión y máximo 2048. `Image.preserveAspect` evita deformarlos. El avatar pequeño usa recorte de UI del mismo original: no duplica ni modifica el PNG. Los 100 PPU y pivotes definen los sprites; en UI el tamaño visible lo determina el RectTransform.

La biblioteca `Assets/_Rumores/Resources/RumoresArt.asset` está referenciada explícitamente por Main y ArtPreview. Sus entradas sin sprite usan el avatar anterior. `SpeechBubbleGraphic` declara `CanvasRenderer` obligatorio; las imágenes y textos decorativos no capturan pulsaciones.

Los originales se generaron con IA en esta sesión, no se tomaron de otros juegos. El personaje se pidió como vecino nervioso con mate, chaleco celeste, contornos azul oscuro y fondo transparente. El pueblo se pidió cenital inclinado, con plaza argentina, kiosco, panadería, club y estación, sin texto ni personajes. Se conservó la primera versión del personaje: una segunda prueba de limpieza no se incorporó. Los PNG finales son copias sin edición externa de las salidas de la herramienta.

SHA-256:

```text
da75e6d1bb42fbb6a85aece5db895c5fa6f74bd38c5bbda8136e176f979866a7  Miedoso.png
c1c0889bebd17ca8a9845d74df92605e502bdbc3d1870c2b719c2a61552c575e  Pueblo.png
```

## Añadir los otros personajes después de la prueba

1. Crear sprites siguiendo la guía, con margen suficiente para movimientos y contorno coherente. Revisar especialmente la densidad de detalles del pueblo y la lectura del rostro pequeño: la muestra aún tiene más detalle y sombreado que la dirección objetivo.
2. Importar cada PNG como Sprite Single con los ajustes anteriores. Asignarlo en el Inspector de `RumoresArt.asset` a `exagerado`, `mentiroso` o `ignorante`. No cambiar los IDs.
3. Probar el avatar pequeño y el retrato completo; revisar escala y margen antes de continuar. Las pantallas y los movimientos reutilizan la nueva referencia sin modificar código ni reglas.
4. Para introducir una personalidad realmente nueva harían falta decisiones de gameplay; completar los cuatro slots existentes no las requiere.

## Validación efectuada y pendientes

- **Ejecutado:** `Tools/test.sh`: 83 pruebas .NET aprobadas, 0 fallos; referencias de 66 GUIDs y 6 assemblies; integridad PNG, dimensiones, alfa y metadatos. La suite incluye las 1296 combinaciones de rondas ya existentes.
- **Ejecutado:** análisis de sintaxis C# 9 de 33 archivos sin errores; `git diff --check`; comprobación de que Core y las noticias no cambiaron.
- **Añadido, no ejecutado:** ocho casos nuevos de Unity: dos de importación, biblioteca, movimientos y restauración, transmisión/división, avance y revancha durante movimientos, escena de revisión y dependencia CanvasRenderer del nuevo gráfico. Se adaptó la regresión del listado para cubrir sprites y avatares provisionales.
- **No ejecutado:** importación real, compilación contra APIs Unity, EditMode, PlayMode y revisión visual en Game View. No hay capturas del motor ni build Android.
- **Pendiente tras aprobar la muestra:** tres sprites de personajes, expresiones por fotogramas si aportan valor, iconos finales, efectos de celebración/derrota más elaborados, props independientes y acabado de toda la UI.

Las reacciones y conexiones existen en código, pero aún no se pueden certificar como correctas en ejecución. No hay preparación final ni publicación Android en esta entrega.
