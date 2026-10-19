# Validación simulada del party game

Generado con `dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md`. Usa las reglas C# reales; no ejecuta Unity ni evalúa diversión humana.

432 rondas: 36 semillas × 2 tamaños (2 y 8) × 6 secuencias. Cada secuencia rota las seis acciones, divide en el primer turno y alterna ramas A/B. Todos juegan exactamente dos veces.

| Participantes | Rondas | Empates | Puntos medios/persona | Ganadores por asiento (incluye empates) |
|---|---:|---:|---:|---|
| 2 | 216 | 96 | 4.53 | 168, 144 |
| 8 | 216 | 132 | 4.48 | 84, 84, 84, 72, 84, 84, 72, 72 |

Titulares finales distintos: 168. Ramas que terminan con pánico 100, credibilidad 0 o difusión 20: 144 (sobre 864 ramas).

Estos recorridos son pruebas estructurales, no jugadores óptimos. Los empates son esperables con 0–6 puntos y se muestran como victoria compartida. La novedad se evalúa por participante y rama: una contribución ajena no consume tu oportunidad. Los umbrales del objetivo siguen dependiendo del orden; validarlo con personas antes de ampliar el balance.

## Ejemplos reproducibles

### Semilla 0 · ¡Que cunda el pánico!

Original: La campana del pueblo sonó durante la siesta.

- T1 · A · Mentir: Tengo la exclusiva de la campana: el alcalde cobra entrada por cada campanada. (+2)
- T2 · B · Malinterpretar: ¿Cómo que no? La noticia era sobre la campaña electoral en el campanario. (+2)
- T3 · A · Entrar en pánico: No quiero alarmar, pero la campana podría despertar a todos los fantasmas. ¡Traigan un casco! (+3)
- T4 · B · Calmar: Antes de correr, preguntemos qué pasó con la campaña electoral en el campanario. (+2)

Resultado: Vecino 1 = 5; Vecino 2 = 4. Ganadores: Vecino 1.

### Semilla 5 · Que se entere el pueblo

Original: Apareció un zapato gigante en el puente.

- T1 · A · Mentir: Un testigo del puente jura que el zapatero ya le cobró alquiler. Todo por el zapato. (+3)
- T2 · B · Malinterpretar: Entendí perfecto: apareció un pato con cordones en el puente. (+2)
- T3 · A · Entrar en pánico: Cierren las ventanas del puente: el zapato podría anunciar la llegada de un gigante. (+2)
- T4 · B · Calmar: Respiren: sobre un pato con cordones en el puente todavía faltan pruebas. (+2)

Resultado: Vecino 1 = 5; Vecino 2 = 4. Ganadores: Vecino 1.

### Semilla 21 · Que alguien nos crea

Original: El gallo del panadero cantó a medianoche.

- T1 · A · Mentir: Tengo la exclusiva del gallo: firmó un contrato para cantar en la radio. (+2)
- T2 · B · Malinterpretar: ¿Cómo que no? La noticia era sobre un gallo de pan en la panadería. (+2)
- T3 · A · Entrar en pánico: No quiero alarmar, pero el gallo podría estar avisando de una invasión. ¡Traigan un casco! (+2)
- T4 · B · Calmar: Antes de correr, preguntemos qué pasó con un gallo de pan en la panadería. (+2)

Resultado: Vecino 1 = 4; Vecino 2 = 4. Ganadores: Vecino 1, Vecino 2.

## Observaciones

- Repetir tu transformación sobre el mismo protagonista y rama puede cambiar sus palabras, pero no recibe otro punto de aporte. La prueba de repetición lo exige.
- Las confusiones sustituyen al protagonista y quedan en las transformaciones siguientes. Las invenciones anteriores se conservan en el historial; el titular muestra la última transformación, no todas las frases concatenadas.
- El objetivo de pánico puede dejar de puntuar al acercarse a 100; difusión también satura en 20. Calmar y elegir otra rama permiten cambiar el contexto.
- El vocabulario es finito: 12 noticias, 3 plantillas por acción y 2 malentendidos por noticia. La variedad de redacción no equivale a una nueva regla.
- Pendientes: importación/renderizado y pruebas PlayMode en Unity, lectura y pulsación en pantallas pequeñas y prueba de diversión/equidad con personas.
