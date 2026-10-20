# Arquitectura del modo sencillo

## Reutilización e integración

Main mantiene su GUID y referencias a PartyApp, PartyCatalog y PartyActionCard. Se conservan menú, roster, Canvas/área segura, EventSystem, turnos, copias de historia, animación y revancha. AvatarGraphic y BoardGraphics conservan `[RequireComponent(typeof(CanvasRenderer))]`.

El cambio pedido retira del dominio party `PartyValues`, `PartyGoal`, puntuación, bonus y ganadores. `PartyContent.json` usa versión 3: noticias con `category`, `anchor`, `question`, `icebreaker` y `options`; seis acciones narrativas. Cada opción tiene ID, acción y texto propio del episodio. No hay guardados party que migrar. Los archivos y reglas del roguelike anterior siguen intactos y no se cargan desde Main.

## Dominio

`PartyGame` conserva Handoff → Choosing → Revealing → Finished y token `RoundNumber:Turn`. Validación de comandos antes de mutar, confirmación única, dos turnos por jugador y revancha solo al terminar. `PartyPlayer` contiene identidad, apodo y avatar; no puntuación.

`Choices(page)` ofrece tres estilos distintos de los seis disponibles, rotando con semilla y turno. Las páginas 0 y 1 son disjuntas y cubren las seis acciones; otros índices se rechazan. `Preview()` transforma una copia sin consumir turno ni tocar la historia. `Confirm(token, actionId)` aplica esa misma transformación al rumor compartido. La API general `Confirm(token, actionId, branchId, split)` se conserva para evitar reescribir el sistema probado, pero el flujo Main nunca solicita divisiones. Sus copias profundas siguen cubiertas por pruebas.

`PartyTransform` filtra las continuaciones de esa noticia y acción. Prefiere IDs que no están en el historial de la rama. Si se agotan, permite reutilizarlos salvo el inmediatamente anterior. La selección es determinista con ciclo de noticias (`uint(seed) / cantidadNoticias`), turno e índice de acción. Usar el ciclo evita que una noticia que reaparece después de recorrer el catálogo tenga siempre las mismas variantes. El orden de las seis opciones también incorpora ese ciclo. El evento guarda `detailId`, `detail` y `parentDetailId`, además de autor, texto y motivo.

El texto se compone de `anchor + detalle anterior + nuevo detalle`. No se concatenan recursivamente los titulares completos: mantienen longitud acotada y las frases anteriores siguen en el historial. No se reemplazan sujetos por palabras al azar ni se cruzan continuaciones entre noticias. La confusión es un detalle escrito para ese episodio. Las copias profundas incluyen los nuevos campos del evento.

El validador exige seis estilos, al menos tres variantes por estilo/noticia, IDs de opciones únicos, acciones existentes, textos distintos dentro de la noticia y un máximo de 240 caracteres para el peor par de detalles más su contexto. Rechaza placeholders sin resolver. Estas comprobaciones verifican estructura y longitud; la coherencia semántica requiere revisión editorial.

## Presentación

PartyApp muestra pregunta, rumor actual y tres tarjetas verticales en un ScrollRect de altura natural. PartyActionCard recibe el texto exacto de Preview y la explicación del catálogo; no calcula reglas. «Ver otras 3» cambia la página y limpia la selección; volver del historial conserva página y selección. Solo la confirmación aplica una transformación. Pantallas antiguas se desactivan antes de destruirse, callbacks usan versión de pantalla y comandos usan token. PageTransition agrega un bloqueo de 0,18 s (0,06 s con movimiento reducido). El resultado muestra original/final, pregunta social e historial de autores y cambios.

La escena construye su UI al iniciar y reutiliza sus referencias. Se mantienen uGUI 1.0.0, Test Framework 1.1.33 y Unity 2022.3.62f1. La ronda vive en memoria. PlayerPrefs almacena `Rumores.Party.FastAnimations`, `Rumores.Party.MusicVolume` y `Rumores.Party.EffectsVolume`. PartyAudio carga ocho clips locales; no muta el dominio. UiStrings carga la tabla es-AR y el contenido narrativo sigue en PartyContent.json. Una pausa oculta la vista activa de partida sin avanzar el estado del modelo. Ver Docs/FASE_4_UI_AUDIO.md.

## Verificación

Core compila sin UnityEngine mediante .NET 8.0.426. Las pruebas verifican opciones, vista previa fiel y sin efectos, turno único, independencia de historias y revancha. PlayMode recorre los botones con 2/8 jugadores y conserva la regresión del CanvasRenderer. Validación estática comprueba referencias, GUIDs y paquetes; Roslyn comprueba sintaxis C# 9.

Unity Editor no está instalado aquí. No se certifican importación, renderizado, API Unity ni Android a partir de esas comprobaciones. Ver TEST_PLAN.md.
