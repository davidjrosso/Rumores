# Rumores

Party game local para **2–8 personas**, en **Unity 2022.3.62f1 y C#**. Pasen el teléfono, transformen una noticia absurda, creen dos versiones y descubran quién aportó más al chisme. Rondas de dos turnos por participante; avatares y pueblo provisionales.

La fase 1 está implementada e integrada en archivos. **81 pruebas .NET aprobadas; ejecución visual y compilación completa en Unity pendientes**, porque este entorno no tiene el editor. No hay APK ni validación en teléfono. [Evidencia y pendientes](TEST_PLAN.md).

## Abrir y completar una ronda

1. Descargar el proyecto actualizado o actualizar el checkout. En Unity Hub, **Add → Add project from disk**, seleccionar la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`. Usar **Unity 2022.3.62f1** con licencia activa. Para jugar en el editor no hace falta instalar Android Build Support.
2. Esperar la importación y abrir **`Assets/_Rumores/Scenes/Main.unity`**. Está registrada como escena de entrada. Usar Game View vertical, por ejemplo **720×1280**, y pulsar **Play**.
3. Pulsar **Jugar**. Hay dos participantes con apodos iniciales; editar apodos, tocar los avatares para cambiarlos y usar **Agregar participante** hasta ocho. La lista se desplaza. Los apodos deben ser distintos, no vacíos y de hasta 16 caracteres.
4. Pulsar **Comenzar ronda**. Entregar el teléfono a la persona indicada y pulsar **Estoy listo**.
5. Leer el objetivo y el rumor. Elegir una de las seis acciones y pulsar **Confirmar acción**. Antes de confirmar se puede activar **Dividir** una vez por ronda; no se permite en el último turno.
6. Leer qué cambió, los puntos y su explicación. Pulsar **Siguiente participante**. Si hay dos ramas, elegir **A · La plaza** o **B · El mercado** antes de actuar. **Historia** muestra el original y los cambios de ambas.
7. Tras 4 turnos con dos personas, o 16 con ocho, pulsar **Ver resultados**. Se muestran original, versiones finales, historial, puntuación, ganador o empate. **Nueva ronda** conserva participantes y avatares, limpia puntos/ramas y cambia la noticia.

**Cómo jugar** explica las reglas. **Ajustes** permite animaciones rápidas y recuerda esa preferencia. Las rondas viven en memoria: se puede volver desde el menú mientras la aplicación esté abierta; cerrarla termina la ronda. No se cargan los guardados del modo anterior.

## Qué incluye

- Menú, registro de participantes, ocho avatares provisionales y validación de apodos.
- Entrega del teléfono, elección/confirmación separadas, protección contra doble confirmación y avance duplicado.
- 12 noticias locales, cuatro personalidades y las acciones complementarias calmar y difundir. Ningún servicio de IA en ejecución.
- Una bifurcación por ronda; copia profunda del historial y evolución independiente de las ramas.
- Tres objetivos de ronda, puntuación explicada por turno, empates compartidos y revancha.
- Escena `Main`, catálogo serializado y prefab `PartyActionCard` conectados. La UI se construye al iniciar la escena; no hay que arrastrar scripts ni enlazar botones a mano.

## Validación en la nube

Requiere .NET SDK 8 y Python 3:

```bash
bash Tools/test.sh
dotnet run --project Tools/SyntaxCheck/SyntaxCheck.csproj
```

En este entorno: `DOTNET=/workspace/tools/dotnet/dotnet` y `DOTNET_CLI_HOME=/workspace/tools/dotnet-home`. `Tools/test.sh` encuentra automáticamente ese SDK. `bash Tools/bootstrap-dotnet.sh` reproduce la instalación fijada 8.0.426 con verificación SHA-512. No hacen falta servicios, claves ni paquetes NuGet externos para las reglas.

El primer comando compila y ejecuta las reglas, incluyendo los 60 casos del modo anterior, y comprueba referencias de assets. El segundo solo comprueba sintaxis C# 9; **no sustituye la compilación/importación de Unity**.

Informe reproducible de 432 rondas y ejemplos narrativos:

```bash
dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md
```

## Validación en Unity pendiente

En **Window → General → Test Runner**, ejecutar **EditMode** y **PlayMode**. Los casos PlayMode usan botones reales con dos y ocho participantes, apodos inválidos, las cuatro personalidades, división, resultados y revancha. Las pruebas de guardado del legado usan directorios temporales. Una prueba cambia el ajuste de animaciones; usar un perfil de pruebas.

También se pueden ejecutar con un editor instalado y activado:

```bash
export UNITY_EDITOR=/ruta/al/ejecutable/Unity
bash Tools/unity.sh editmode
bash Tools/unity.sh playmode
```

El wrapper rechaza resultados XML vacíos o fallidos y escribe logs en `TestResults/`. Si la máquina no admite UGUI sin gráficos, ejecutar PlayMode desde el editor. Revisar además pantallas 720×1080, 720×1280 y teléfonos estrechos, teclado, notch y scroll. No se ejecutó producción artística ni publicación Android en esta fase.

## Archivos principales

| Ubicación | Responsabilidad |
|---|---|
| `Scripts/Core/PartyContent.cs`, `PartyGame.cs` | Datos, transformaciones, turnos, ramas y puntos; sin UnityEngine |
| `Resources/PartyContent.json`, `PartyCatalog.asset` | Noticias, plantillas, efectos y objetivos configurables |
| `Scripts/Presentation/PartyApp.cs` | Menú, roster, ronda, historial y resultados |
| `Scripts/Presentation/PartyCatalogAsset.cs`, `PartyActionCard.cs` | Carga del catálogo y vista del prefab |
| `Prefabs/PartyActionCard.prefab`, `Scenes/Main.unity` | Prefab reutilizado y referencias de entrada |
| `Tests/Shared/PartyChecks.cs`, `Tests/EditMode/PartyTests.cs`, `Tests/PlayMode/FlowTests.cs` | Contratos de reglas y flujo en Unity |
| `Tests/Program.cs`, `Tests/PartyReport.cs`, `Tools/validate_project.py` | Runner .NET, informe y verificación estática |

Las rutas de Unity son relativas a `Assets/_Rumores/`. Diseño: [GDD.md](GDD.md), [BALANCE.md](BALANCE.md), [TECHNICAL_DESIGN.md](TECHNICAL_DESIGN.md). La versión roguelike anterior permanece en `Scenes/Legacy.unity`, sus datos/scripts originales y [Docs/Legacy](Docs/Legacy); no participa en el flujo nuevo. El error antiguo «El mapa no coincide con la semilla y versión» no pertenece al nuevo modo, que no carga mapas ni partidas roguelike.
