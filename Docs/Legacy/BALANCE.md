# Balance inicial · reglas 1

Todos los números configurables están en `Assets/_Rumores/Resources/Content.json`, referenciado por el ScriptableObject `RumoresCatalog.asset`. Editar una regla exige volver a ejecutar las pruebas de solvencia. Los valores se aplican al receptor, incluido el origen, antes de copiar hacia sus hijos.

| Personalidad | Importancia | Pánico | Credibilidad | Distorsión | Regla adicional |
|---|---:|---:|---:|---:|---|
| Miedoso | 0 | +18 | 0 | 0 | +5 Importancia si Pánico resultante >40 |
| Exagerado | +14 | +10 | −8 | +12 | Frase amplificadora |
| Mentiroso | +5 | 0 | −12 | +18 | Afirmación marcada como invención |
| Ignorante | 0 | 0 | 0 | +14 | Primera reinterpretación compatible según orden de catálogo |

`clamp(v) = max(0, min(100, v))`, aplicado después de cada transformación numérica. Un rumor parte de (25, 15, 95, 0). Noticias distintas comparten estos atributos para preservar solvencia; las etiquetas cambian el texto de las confusiones. La prioridad semántica actual es animal → máquina → autoridad → misterio. Al confundir se elimina la etiqueta de entrada y se añade la resultante sin duplicados. Mentir no modifica etiquetas. No se consume aleatoriedad al transformar.

## Agregados y condiciones

- Alcance: cantidad de receptores únicos, incluido el origen. Fracción alcanzada = alcance / habitantes.
- Importancia/pánico colectivos: suma de cada valor final recibido / total de habitantes del tablero (10). No se redondea para evaluar; la UI muestra un decimal.
- Credibilidad: mínimo de los receptores. Quien no recibe no interviene en este mínimo.
- Distorsión: máximo entre los receptores, debe ser ≤ umbral.
- Destinatario: presencia del ID indicado.
- Profundidad: máximo número de habitantes en un recorrido, incluido el origen.
- Contraste: máximo pánico menos mínimo pánico entre hojas. Con menos de dos hojas, cero.

Victoria requiere todas las condiciones. Una estrella por victoria, otra por usar como máximo `efficientEdges`, otra por cumplir todas las secundarias. Fallo: cero estrellas.

| Plantilla | Presupuesto | Límite eficiente | Objetivos obligatorios | Solución testigo |
|---|---:|---:|---|---|
| Plaza | 6 | 5 | Alcance ≥5 | a→b, a→c, b→e, c→f |
| Molino | 7 | 6 | Alcance ≥6, Pánico colectivo ≥25 | a→b, a→d, b→e, d→h, e→i, h→j |
| Archivo | 6 | 4 | Alcance ≥5, Credibilidad mínima ≥85 | a→b, a→c, b→e, c→f |
| Feria | 7 | 5 | Llegar a i, Distorsión ≤35, Alcance ≥5 | a→b, a→c, b→e, e→i |
| Estación | 7 | 5 | Profundidad ≥4, Importancia colectiva ≥19 | a→b, a→c, b→e, e→i, c→g |
| Asamblea | 8 | 7 | Alcance ≥8, Importancia colectiva ≥29, Contraste ≥18 | a→b, a→d, b→e, b→f, d→g, d→h, e→i |

IDs: a Nora, b Beto, c Lola, d Tito, e Rita, f Paco, g Vera, h Milo, i Cora, j Dani. Los perfiles están declarados por encuentro: Archivo reemplaza varios perfiles para favorecer difusión creíble.

## Mejoras

| Mejora | Efecto por elección | Máximo de elecciones |
|---|---|---:|
| Hilo de confianza | +1 presupuesto en todos los encuentros posteriores | 5 |
| Drama de bolsillo | +6 Importancia del exagerado | 3 |
| Coartada impecable | Reduce en 6 la pérdida de Credibilidad del mentiroso | 2 |
| Pregonero por un día | +1 salida permitida del exagerado (máximo total 3) | 1 |
| Ronda de mates | +1 Paciencia, tope 3 | 5 |

Las ofertas contienen hasta tres IDs distintos entre los que no agotaron sus acumulaciones. La configuración actual siempre permite tres en los encuentros con recompensa de una run de seis etapas. En los eventos se elige entre +1 Paciencia (tope 3) y +1 Susurro; solo hay dos eventos posibles por run. No se arrastran mejoras entre runs.

## Estado de balance

La solvencia de todas las variantes del catálogo y 100 runs completas está verificada por pruebas. La duración de 10–20 minutos, claridad de la interfaz, dificultad percibida, diversión y suficiencia de la variedad requieren playtesting humano. Las soluciones testigo prueban existencia, no una dificultad equilibrada ni todas las combinaciones posibles. El contenido es deliberadamente pequeño; algunas rutas pueden repetir plantillas.
