# Rumores: convenciones del party game

Objetivo actual: party game local Android, 2–8 jugadores en un teléfono, rondas cortas y revancha. Unity 2022.3.62f1, C#, uGUI. Sin roguelike, progreso permanente, servicios online ni monetización en el flujo nuevo.

- Usar el checkout existente; las tareas cloud ya están aisladas. No crear worktrees salvo petición expresa.
- Entrada actual: Assets/_Rumores/Scenes/Main.unity. El modo anterior se conserva en Legacy.unity y Git; no añadir sus sistemas a la ronda party.
- Reglas puras en Scripts/Core/Party*.cs, contenido en Resources/PartyContent.json. No depender de frames, animaciones ni UnityEngine para turnos, puntos o ramas.
- Cada confirmación necesita el token del turno. Un evento visual no puede otorgar puntos ni repetir una acción.
- Todo jugador dispone de dos turnos. Las ramas comparten historia por copia, nunca referencias mutables. Empates: victoria compartida.
- Presentación provisional con recursos existentes y prefabs; posponer arte definitivo y publicación Android hasta que se soliciten.
- Ejecutar Tools/test.sh y comprobar sintaxis/asset wiring después de cambios pertinentes. Mantener las pruebas del legado mientras su código permanezca.
- Reportar por separado pruebas .NET, comprobaciones estáticas y ejecución Unity. No hay editor Unity ni SDK Android en este entorno; nunca describirlos como ejecutados sin evidencia.
- No cargar archivos de guardado roguelike en el party game. La ronda inicial vive en memoria; los ajustes básicos se guardan por separado.
- Actualizar GDD.md, BALANCE.md, TECHNICAL_DESIGN.md y TEST_PLAN.md al cambiar las reglas. Mantener la documentación del modo anterior en Docs/Legacy.
