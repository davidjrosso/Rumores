# Balance del prototipo party

## Valores y efectos

Cada noticia empieza con pánico 15, importancia 20, credibilidad 85 y difusión a 1 vecino. Son datos configurables por noticia. Pánico, importancia y credibilidad se limitan a 0–100; difusión a 1–20. Los cambios se aplican antes de puntuar; cuenta el efecto real tras limitar valores.

| Acción | Pánico | Importancia | Credibilidad | Vecinos |
|---|---:|---:|---:|---:|
| Entrar en pánico | +18 | +3 | −4 | +1 |
| Exagerar | +8 | +18 | −8 | +2 |
| Mentir | +4 | +8 | −16 | +2 |
| Malinterpretar | +2 | +5 | −6 | +1 |
| Calmar | −18 | −4 | +14 | 0 |
| Difundir | 0 | +5 | −2 | +4 |

Además del efecto numérico, cada personalidad produce una transformación narrativa distinta. El ignorante cambia el protagonista; el mentiroso introduce una afirmación marcada como inventada. Importancia participa en la reacción del pueblo: con importancia ≥65 aparece una asamblea extraordinaria. La prioridad de reacciones es pánico ≥65, credibilidad ≤40, importancia ≥65, difusión ≥10 y reacción tranquila por defecto. Estas reacciones no dan puntos extra.

## Fórmula

**Puntos del turno = aporte (0/1) + variedad (0/1) + objetivo (0/1).** Cada persona tiene dos turnos: máximo seis puntos. No hay estadísticas ocultas ni bonus al terminar.

- **Aporte:** +1 si esa persona todavía no aplicó esa acción al protagonista actual en esa rama. Se compara `playerId + actionId + subject` con el historial de la rama. Cambiar solo la plantilla de redacción no crea un aporte nuevo. Un cambio de protagonista o una rama donde aún no hiciste ese aporte sí puede hacerlo. Los aportes de otros participantes no consumen tu oportunidad.
- **Variedad:** +1 si es la primera vez que esa persona utiliza la acción durante la ronda, independientemente de la rama.
- **Objetivo:** +1 si el cambio real cumple el objetivo común. Puede cumplirse en ambos turnos. Los valores saturados no simulan aumentos inexistentes.

El historial copiado al bifurcar ya cuenta como historia de ambas ramas. Dividir no da puntos ni turnos adicionales; se asigna la puntuación del evento una sola vez. Gana el total más alto, con empates compartidos.

## Objetivos

| Objetivo | Condición exacta | Decisión favorecida |
|---|---|---|
| Que cunda el pánico | Pánico final − inicial del turno ≥10 | Entrar en pánico mientras haya margen |
| Que alguien nos crea | Credibilidad final ≥80 y no menor que antes | Calmar y recuperar confianza |
| Que se entere el pueblo | Vecinos finales − iniciales del turno ≥2 | Difundir, exagerar o mentir mientras haya margen |

No hay una personalidad ganadora fija para todos los objetivos. Aun así, estos umbrales no demuestran por sí solos equidad o diversión: la historia, el orden y la saturación influyen.

## Observaciones de la simulación

[Informe reproducible](Docs/PARTY_VALIDATION.md): 432 rondas, 168 titulares finales distintos. Con ocho participantes hubo 132 empates en 216 recorridos; la escala corta hace que compartir victoria sea frecuente.

Se detectó y corrigió un sesgo inicial: medir novedad globalmente favorecía a los primeros asientos (120 frente a 24 apariciones como ganador en los extremos). Evaluarla por persona y rama redujo el rango a 72–84 en las secuencias de ocho participantes. Esto es evidencia limitada a esas secuencias, no una prueba de igualdad entre estrategias óptimas.

Persisten límites deliberados: vocabulario finito, saturación de atributos en rondas largas y objetivos que dependen del orden. Probar con grupos reales antes de añadir complejidad o más puntos. No se agregó un desempate arbitrario.
