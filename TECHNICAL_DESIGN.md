# Arquitectura del modo sencillo

## Reutilización e integración

Main mantiene su GUID y referencias a PartyApp, PartyCatalog y PartyActionCard. Se conservan menú, roster, Canvas/área segura, EventSystem, turnos, copias de historia, animación y revancha. AvatarGraphic y BoardGraphics conservan `[RequireComponent(typeof(CanvasRenderer))]`.

El cambio pedido retira del dominio party `PartyValues`, `PartyGoal`, puntuación, bonus y ganadores. `PartyContent.json` pasa a versión 2: noticias con `question` e `icebreaker`, cuatro acciones narrativas y plantillas. No hay guardados party que migrar. Los archivos y reglas del roguelike anterior siguen intactos y no se cargan desde Main.

## Dominio

`PartyGame` conserva Handoff → Choosing → Revealing → Finished y token `RoundNumber:Turn`. Validación de comandos antes de mutar, confirmación única, dos turnos por jugador y revancha solo al terminar. `PartyPlayer` contiene identidad, apodo y avatar; no puntuación.

`Choices()` ofrece tres estilos distintos de los cuatro disponibles, rotando con semilla y turno. `Preview()` transforma una copia sin consumir turno ni tocar la historia. `Confirm(token, actionId)` aplica esa misma transformación al rumor compartido. La API general `Confirm(token, actionId, branchId, split)` se conserva para evitar reescribir el sistema probado, pero el flujo Main nunca solicita divisiones. Sus copias profundas siguen cubiertas por pruebas.

`PartyTransform` expande plantillas deterministas. El malentendido sustituye el protagonista; el evento conserva autor, texto y motivo. La última versión se muestra en el turno siguiente; las anteriores permanecen en el historial. Las contracciones «de el»/«a el» se resuelven con límites de palabra para no alterar «desde el».

## Presentación

PartyApp muestra pregunta, rumor actual y tres tarjetas verticales. PartyActionCard recibe el texto exacto de Preview; no calcula nada. Solo el botón de confirmación altera la ronda. Pantallas antiguas se desactivan antes de destruirse, callbacks usan versión de pantalla y comandos usan token. El resultado muestra original/final y una pregunta social, con historial opcional.

La escena construye su UI al iniciar. No hacen falta nuevas conexiones ni assets. Se mantienen uGUI 1.0.0, Test Framework 1.1.33 y Unity 2022.3.62f1. La ronda vive en memoria; PlayerPrefs solo almacena `Rumores.Party.FastAnimations`.

## Verificación

Core compila sin UnityEngine mediante .NET 8.0.426. Las pruebas verifican opciones, vista previa fiel y sin efectos, turno único, independencia de historias y revancha. PlayMode recorre los botones con 2/8 jugadores y conserva la regresión del CanvasRenderer. Validación estática comprueba referencias, GUIDs y paquetes; Roslyn comprueba sintaxis C# 9.

Unity Editor no está instalado aquí. No se certifican importación, renderizado, API Unity ni Android a partir de esas comprobaciones. Ver TEST_PLAN.md.
