# Rumores: convenciones del party game

Objetivo actual: party game local Android, 2–8 jugadores en un teléfono, rondas cortas y revancha. Unity 2022.3.62f1, C#, uGUI. Sin roguelike, progreso permanente, servicios online ni monetización en el flujo nuevo.

- Usar el checkout existente; las tareas cloud ya están aisladas. No crear worktrees salvo petición expresa.
- Entrada actual: Assets/_Rumores/Scenes/Main.unity. El modo anterior se conserva en Legacy.unity y Git; no añadir sus sistemas a la ronda party.
- Reglas puras en Scripts/Core/Party*.cs, contenido argentino en Resources/PartyContent.json (versión 3). No depender de frames, animaciones ni UnityEngine para turnos e historia.
- Cada confirmación necesita el token del turno. Una vista previa o evento visual no puede consumir un turno ni repetir una acción.
- Todo jugador dispone de dos turnos. Main ofrece tres respuestas completas por página, seis estilos por turno y una historia compartida. «Ver otras 3» no consume turno. Cada continuación debe pertenecer a su noticia y conservar el aporte anterior; no sustituir protagonistas con mezclas arbitrarias. Sin atributos, puntos, objetivos numéricos, clasificación ni controles de ramas. No reintroducirlos sin pedido expreso. La API interna de ramas se conserva con copias y pruebas.
- Fase 3: el usuario confirmó que el arte y los sprites de la muestra ecb4e80 funcionaron bien y autorizó continuar. Cuatro personajes en RumoresArt.asset, iconos/efectos procedurales y galería ArtPreview.unity; guía Docs/Art/DIRECCION_VISUAL.md. La confirmación previa permite ampliar el arte, pero no equivale a ejecutar las pruebas Unity de cambios posteriores. Publicación Android sigue fuera de alcance.
- Ejecutar Tools/test.sh y comprobar sintaxis/asset wiring después de cambios pertinentes. Mantener las pruebas del legado mientras su código permanezca.
- Reportar por separado pruebas .NET, comprobaciones estáticas y ejecución Unity. No hay editor Unity ni SDK Android en este entorno; nunca describirlos como ejecutados sin evidencia.
- No cargar archivos de guardado roguelike en el party game. La ronda inicial vive en memoria; los ajustes básicos se guardan por separado.
- Actualizar GDD.md, BALANCE.md, TECHNICAL_DESIGN.md y TEST_PLAN.md al cambiar las reglas. Mantener la documentación del modo anterior en Docs/Legacy.
