# Rumores · chismes para jugar con amigos

Un party game local de **2 a 8 personas** con un solo teléfono. Leés un chisme argentino, elegís cómo contarlo y pasás el celu. Al final comparan cómo empezó la historia y en qué terminó.

**La partida ahora es más simple:** una historia compartida, tres respuestas en pantalla, seis opciones disponibles por turno y dos vueltas. Sin estadísticas, puntos, objetivos numéricos, ganadores automáticos ni selección de ramas.

## Abrir y jugar

1. Abrir la carpeta del proyecto desde Unity Hub con **Unity 2022.3.62f1**. Para jugar en el editor no hace falta Android Build Support.
2. Abrir `Assets/_Rumores/Scenes/Main.unity`, usar Game View vertical (por ejemplo 720×1280) y pulsar **Play**.
3. Tocar **Jugar**, escribir los apodos y elegir avatares. Pueden jugar entre 2 y 8 amigos; la lista se desplaza.
4. Pasar el teléfono a la persona indicada. Tocar **Estoy listo**, leer el rumor y su pregunta.
5. Elegir una de las **tres respuestas completas** y tocar **Contarlo así**. **Ver otras 3** muestra las alternativas restantes sin gastar el turno ni cambiar el rumor. No hay respuesta correcta ni hay que hacer cuentas.
6. Leer la nueva versión en voz alta y tocar **Pasar el celu**. Cada persona juega dos veces.
7. Al final se muestra **Así empezó / Así lo dejaron**, una pregunta para charlar y el botón **¡Otra!**. **Ver el chisme completo** permite repasar lo que contó cada amigo.

**Cómo jugar** explica estos pasos; **Ajustes** permite acelerar la aparición de los textos. La ronda se conserva mientras la app siga abierta. Cerrar la app la termina; la preferencia de animaciones sí se guarda.

## Humor argentino

36 chismes originales, 72 preguntas y **648 continuaciones escritas para cada tema** (18 por chisme):

- **Cotidiano:** mate perdido, bondi, empanadas, asado, truco y facturas.
- **TikTok e internet:** receta con discusión de fondo, stream interrumpido por un tupper, tutorial de café, audio eterno, canje y filtro en videollamada.
- **Memes argentinos:** anulo mufa, Muchachos, Scaloneta, «fua, el Diego», saludos de vivos y «esto es cine».
- **Nostalgia:** MSN, Fotolog, videoclub, ciber, Family y teléfono fijo.
- **Vida actual:** menú QR, mensajes hechos con IA, delivery, carteles de precios, teletrabajo y grupos de vecinos.
- **Historia y costumbres:** actos del 25 de Mayo, Cabildo, locro, radio, patacones y sereno escolar.

Son escenas ficticias, no noticias reales ni un seguimiento de las tendencias del día. Hay seis maneras de continuar: **Hacerse la cabeza**, **Hacerla enorme**, **Mandar fruta**, **Entender cualquiera**, **Tirar una teoría** y **Hacer un meme**. Los avatares siguen siendo decorativos.

Cada carta mantiene el episodio original y el detalle elegido por el jugador anterior, y añade una continuación propia de esa historia. Confundir una palabra ya no convierte al protagonista en un objeto al azar. El texto visible conserva los dos aportes más recientes; **Lo anterior** guarda toda la secuencia. Las variantes no usadas de cada estilo tienen prioridad. Todo funciona con datos locales y sin IA en ejecución.

## Muestra artística · fase 3

El menú incorpora un pueblo ilustrado original y una marca de globos con texto editable. El primer avatar tiene un nuevo personaje miedoso con mate; hay movimientos breves de habla y reacción durante la ronda. Los demás personajes y la UI conservan recursos provisionales.

Para revisar las siete reacciones y los efectos de transmisión/división, abrir **`Assets/_Rumores/Scenes/ArtPreview.unity`** y pulsar Play. Para jugar, abrir **Main.unity**. La división y el desánimo de la muestra no cambian las reglas del party game.

**La fase 3 está pendiente de validación en Unity.** Se produjeron solo un pueblo y un personaje para respetar la prueba inicial antes de ampliar el arte. Ver la [guía visual](Docs/Art/DIRECCION_VISUAL.md) y las [instrucciones, inventario y pendientes](Docs/Art/PRUEBA_UNITY.md).

## Validación

**83 pruebas .NET aprobadas**: 23 del modo actual y 60 del legado. Incluyen 1296 rondas simuladas con opciones reales, protección contra doble pulsación, vista previa sin efectos y revancha. Referencias estáticas y sintaxis C# 9 comprobadas.

**Unity no está instalado en el entorno cloud:** la importación, compilación completa, pruebas visuales y PlayMode siguen pendientes. Se conserva la corrección de `CanvasRenderer` que faltaba en los avatares. La muestra tiene imágenes generadas con IA, controles estáticos de alfa/importación y nuevas pruebas de Unity aún no ejecutadas. No se publicó Android.

```bash
bash Tools/test.sh
dotnet run --project Tools/SyntaxCheck/SyntaxCheck.csproj
dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md
```

En cloud se usa `/workspace/tools/dotnet/dotnet`, SDK 8.0.426, con `DOTNET_CLI_HOME=/workspace/tools/dotnet-home`. `Tools/bootstrap-dotnet.sh` instala esa versión con verificación SHA-512. No se necesitan servicios ni claves.

En Unity: **Window → General → Test Runner → EditMode / PlayMode**. También se pueden usar `Tools/unity.sh editmode` y `Tools/unity.sh playmode` con `UNITY_EDITOR` definido y licencia activa. Ver [TEST_PLAN.md](TEST_PLAN.md).

## Archivos principales

- `Assets/_Rumores/Resources/PartyContent.json`: rumores, preguntas y respuestas configurables; catálogo versión 3.
- `Assets/_Rumores/Scripts/Core/PartyContent.cs`, `PartyGame.cs`: narración, turnos, vista previa e historial.
- `Assets/_Rumores/Scripts/Presentation/PartyApp.cs`, `PartyActionCard.cs`: pantallas y tres tarjetas con respuestas completas.
- `Assets/_Rumores/Scenes/Main.unity`, `Resources/PartyCatalog.asset`, `Prefabs/PartyActionCard.prefab`: referencias integradas; no hace falta conectar nada manualmente.
- `Assets/_Rumores/Tests`, `Tests/PartyReport.cs`: pruebas y [ejemplos de partidas](Docs/PARTY_VALIDATION.md).

[GDD](GDD.md), [ritmo y variedad](BALANCE.md), [arquitectura](TECHNICAL_DESIGN.md). El antiguo roguelike sigue en `Legacy.unity` y `Docs/Legacy`; no se carga desde Main. La API de bifurcación se conserva internamente con pruebas, pero no aparece en la partida simplificada.
