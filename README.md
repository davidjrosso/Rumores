# Rumores · chismes para jugar con amigos

Un party game local de **2 a 8 personas** con un solo teléfono. Leés un chisme argentino, elegís cómo contarlo y pasás el celu. Al final comparan cómo empezó la historia y en qué terminó.

**La partida ahora es más simple:** una historia compartida, tres respuestas por turno y dos vueltas. Sin estadísticas, puntos, objetivos numéricos, ganadores automáticos ni selección de ramas.

## Abrir y jugar

1. Abrir la carpeta del proyecto desde Unity Hub con **Unity 2022.3.62f1**. Para jugar en el editor no hace falta Android Build Support.
2. Abrir `Assets/_Rumores/Scenes/Main.unity`, usar Game View vertical (por ejemplo 720×1280) y pulsar **Play**.
3. Tocar **Jugar**, escribir los apodos y elegir avatares. Pueden jugar entre 2 y 8 amigos; la lista se desplaza.
4. Pasar el teléfono a la persona indicada. Tocar **Estoy listo**, leer el rumor y su pregunta.
5. Elegir una de las **tres respuestas completas** y tocar **Contarlo así**. No hay respuesta correcta ni hay que hacer cuentas.
6. Leer la nueva versión en voz alta y tocar **Pasar el celu**. Cada persona juega dos veces.
7. Al final se muestra **Así empezó / Así lo dejaron**, una pregunta para charlar y el botón **¡Otra!**. **Ver el chisme completo** permite repasar lo que contó cada amigo.

**Cómo jugar** explica estos pasos; **Ajustes** permite acelerar la aparición de los textos. La ronda se conserva mientras la app siga abierta. Cerrar la app la termina; la preferencia de animaciones sí se guarda.

## Humor argentino

20 rumores originales y 40 preguntas: una para elegir qué contar y otra para conversar al final. Mate, bondi, SUBE, empanadas, kiosco, asado, alfajor, truco, facturas, fulbito, choripán, sifón, sierras de Córdoba, Mendoza, carpinchos, peña, fernet, tortas fritas, grupo del barrio y locro.

Las respuestas combinan cuatro maneras de contar: **Hacerse la cabeza**, **Hacerla enorme**, **Mandar fruta** y **Entender cualquiera**. Se ofrecen tres por turno y van rotando. Los textos usan datos y plantillas locales; no necesitan IA ni conexión. Los avatares son decorativos.

## Validación

**78 pruebas .NET aprobadas**: 18 del modo actual y 60 del legado. Incluyen 360 rondas simuladas con opciones reales, protección contra doble pulsación, vista previa sin efectos y revancha. Referencias estáticas y sintaxis C# 9 comprobadas.

**Unity no está instalado en el entorno cloud:** la importación, compilación completa, pruebas visuales y PlayMode siguen pendientes. Se conserva la corrección de `CanvasRenderer` que faltaba en los avatares. No se produjo arte definitivo ni se publicó Android.

```bash
bash Tools/test.sh
dotnet run --project Tools/SyntaxCheck/SyntaxCheck.csproj
dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md
```

En cloud se usa `/workspace/tools/dotnet/dotnet`, SDK 8.0.426, con `DOTNET_CLI_HOME=/workspace/tools/dotnet-home`. `Tools/bootstrap-dotnet.sh` instala esa versión con verificación SHA-512. No se necesitan servicios ni claves.

En Unity: **Window → General → Test Runner → EditMode / PlayMode**. También se pueden usar `Tools/unity.sh editmode` y `Tools/unity.sh playmode` con `UNITY_EDITOR` definido y licencia activa. Ver [TEST_PLAN.md](TEST_PLAN.md).

## Archivos principales

- `Assets/_Rumores/Resources/PartyContent.json`: rumores, preguntas y respuestas configurables; catálogo versión 2.
- `Assets/_Rumores/Scripts/Core/PartyContent.cs`, `PartyGame.cs`: narración, turnos, vista previa e historial.
- `Assets/_Rumores/Scripts/Presentation/PartyApp.cs`, `PartyActionCard.cs`: pantallas y tres tarjetas con respuestas completas.
- `Assets/_Rumores/Scenes/Main.unity`, `Resources/PartyCatalog.asset`, `Prefabs/PartyActionCard.prefab`: referencias integradas; no hace falta conectar nada manualmente.
- `Assets/_Rumores/Tests`, `Tests/PartyReport.cs`: pruebas y [ejemplos de partidas](Docs/PARTY_VALIDATION.md).

[GDD](GDD.md), [ritmo y variedad](BALANCE.md), [arquitectura](TECHNICAL_DESIGN.md). El antiguo roguelike sigue en `Legacy.unity` y `Docs/Legacy`; no se carga desde Main. La API de bifurcación se conserva internamente con pruebas, pero no aparece en la partida simplificada.
