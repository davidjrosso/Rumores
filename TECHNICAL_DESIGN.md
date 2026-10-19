# Arquitectura del party game

## Integración Unity

`Assets/_Rumores/Scenes/Main.unity` mantiene su GUID y referencia a `PartyApp`, `Resources/PartyCatalog.asset` y `Prefabs/PartyActionCard.prefab`. El catálogo referencia `PartyCatalogAsset.cs` y `PartyContent.json`; el prefab referencia `PartyActionCard.cs`. Todos tienen `.meta` versionados. No se requiere un paso manual de generación o conexión.

`PartyApp.Start` carga/valida el catálogo, crea Camera, Canvas, escalado, área segura y EventSystem, y abre el menú. Las pantallas uGUI se generan en ejecución; en modo edición se ve el objeto de entrada con sus referencias. `PartyActionCard` instancia su vista desde un prefab reutilizable. Se reutilizan `Ui`, `AvatarGraphic` y la fuente DejaVu Sans con su licencia existente.

Las reglas viven en el assembly `Rumores.Core`, sin UnityEngine. La presentación depende de Core y `UnityEngine.UI`. No se cambiaron paquetes ni versión del motor.

## Dominio y estado

- `PartyContent.cs`: catálogo y validación, atributos, noticias/acciones/objetivos, transformación determinista y copias profundas.
- `PartyGame.cs`: roster normalizado, dos turnos por persona, fases, comandos, puntuación, ramas, ganadores y revancha.
- `PartyApp.cs`: pantallas, entrada, animación y ajuste básico. Nunca calcula puntos ni altera el historial directamente.

Transiciones: `Handoff → Ready → Choosing → Confirm → Revealing → Next → Handoff/Finished`. `Rematch` solo está permitido al terminar. Cada comando de turno exige `RoundNumber:Turn`; fase y token rechazan doble confirmación, doble avance y comandos de una ronda anterior. La presentación desactiva inmediatamente la pantalla vieja antes de destruirla al final del frame y protege callbacks con una versión de pantalla.

`Confirm` valida todo antes de modificar estado, transforma una copia y registra un evento con texto, motivo, valores antes/después y desglose. Si divide, copia profundamente la versión ya transformada a B. Puntos y turno se registran una sola vez. La animación ocurre después del comando y puede interrumpirse sin alterar las reglas.

Los modelos son objetos mutables para serialización local; la presentación debe tratarlos como lectura. Las copias impiden aliasing entre rumores, eventos y atributos de las ramas. Los participantes recibidos por el constructor también se copian.

## Determinismo y configuración

`PartyContent.json` contiene 12 noticias con protagonista, lugar, amenaza, consecuencia, invención, malentendidos y valores iniciales; 6 acciones con efectos y 3 plantillas; y 3 objetivos. Se admiten 4–6 acciones, con las cuatro personalidades requeridas. Nuevas noticias y variantes se añaden en datos. Cambiar mecánicas de objetivos requiere una implementación explícita.

La primera ronda toma una semilla aleatoria de Unity. A partir de ella, todo el dominio es determinista; el core no utiliza Unity.Random ni reloj. La noticia usa `uint(seed) % cantidadNoticias`; el objetivo usa `(uint(seed) / cantidadNoticias + uint(seed)) % cantidadObjetivos`. Esto permite que una misma noticia aparezca con distintos objetivos. La variante de texto usa semilla, turno e índice de acción. Revancha incrementa la semilla, cambia la noticia y reinicia la ronda.

Las plantillas expanden campos locales y contraen «de el»/«a el» a «del»/«al». El ignorante conserva su nueva interpretación como protagonista para acciones posteriores. El titular muestra la última transformación; la historia mantiene toda la secuencia. No hay IA ni servicios en ejecución.

## Persistencia y legado

La ronda nueva es solo memoria; cerrar la aplicación la pierde. El ajuste de animación usa `PlayerPrefs` bajo `Rumores.Party.FastAnimations`. No se accede al archivo `rumores-save.json` ni a mapas anteriores.

`Scenes/Legacy.unity` conserva el componente anterior y su catálogo. Los scripts, datos y pruebas del legado siguen presentes para evitar una reescritura innecesaria. Su documentación anterior está en `Docs/Legacy`. Legacy no está en la lista de escenas de compilación. Abrir ambas escenas de forma aditiva no forma parte del flujo soportado.

## Desarrollo y comprobaciones

.NET 8.0.426 ejecuta Core y pruebas compartidas. `Tools/test.sh` ejecuta la suite y `validate_project.py` verifica GUIDs, ensamblados, versiones y referencias Main→catálogo/prefab. Roslyn comprueba sintaxis C# 9. EditMode usa los mismos contratos con JsonUtility; PlayMode recorre botones reales con 2/8 participantes.

Ninguna comprobación estática sustituye importar y ejecutar Unity. El entorno cloud no tiene editor/licencia utilizable ni SDK Android; las pruebas del motor quedan pendientes. Los scripts Android anteriores se conservan, pero no se ejecutan ni publican en esta fase.
