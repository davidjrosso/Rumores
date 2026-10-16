# Rumores · prototipo 0.1

Estrategia de redes para Android vertical, offline. En Villa Susurro una noticia modesta acaba convertida en un disparate. Una run tiene seis etapas y tres unidades de Paciencia. Objetivo de duración: 10–20 minutos; pendiente de prueba con jugadores.

## Bucle y decisiones
Inicio → nueva semilla o continuar → mapa → construir árbol → pulsos → resultado → mejora → mapa → jefe → resumen. Cada etapa ofrece dos desafíos distintos; en las etapas 2 y 4 se puede optar por un evento (recuperación o mejora) en lugar del desafío de esa ruta. Todas las rutas convergen en el jefe. El catálogo incluye cinco desafíos normales y uno final; la run visita seis encuentros, no necesariamente los cinco desafíos normales.

Un toque selecciona emisor; otro crea/elimina su conexión con el destinatario. Seleccionar un habitante también muestra sus reglas y, tras simular, su rumor. Deshacer elimina la última edición; borrar un enlace elimina su subárbol. Se puede reiniciar el tablero. Simular bloquea la construcción; la pausa y velocidad solo afectan a la presentación.

## Reglas cerradas
10 habitantes por tablero. Un enlace = 1 Susurro; presupuesto base según plantilla. Una entrada por habitante, ninguna al origen; dos salidas, salvo recompensa Pregonero que permite tres al exagerado. Solo relaciones dirigidas declaradas; todos los usados alcanzables; sin ciclos ni fusiones. El origen también transforma la noticia y cuenta en alcance. El simulador usa BFS, destinatarios ordenados por ID ordinal, copias profundas y números enteros. La evaluación sucede una sola vez, antes de animar. El resultado se guarda inmediatamente: cerrar la app no evita la pérdida de paciencia.

Fallar cuesta una paciencia. Con paciencia restante se puede reintentar el mismo encuentro (sin coste adicional hasta volver a fallar) o avanzar sin recompensa. El jefe exige victoria: un fallo permite reintentar o abandonar. Ganar entrega una mejora entre tres distintas. Cada mejora se elige una sola vez por pantalla; los bonos se acumulan hasta sus límites declarados. Nueva run reinicia todos los bonos. Victoria desbloquea un título cosmético; no hay estadísticas permanentes.

## Personalidades
Miedoso: Pánico +18; si queda >40, Importancia +5. Exagerado: Importancia +14, Pánico +10, Distorsión +12, Credibilidad −8. Mentiroso: Distorsión +18, Credibilidad −12, Importancia +5 y afirmación deliberadamente falsa. Ignorante: Distorsión +14 y reinterpretación por etiqueta, usando la primera regla compatible del catálogo. Orden: modificadores → umbral → afirmación/etiqueta → registro. Todos los atributos se limitan a 0–100. Ver BALANCE.md.

## Objetivos y estrellas
Todas las condiciones obligatorias se deben cumplir. Hay alcance, importancia colectiva, pánico colectivo, credibilidad mínima entre alcanzados, distorsión máxima, destinatario, profundidad y diferencia entre hojas. Colectivo = suma de valores de TODOS los receptores / 10; no alcanzados aportan cero. Alcance incluye origen. Victoria = una estrella; +1 si usa como máximo el límite eficiente del encuentro; +1 si satisface su condición secundaria. Un fallo = cero estrellas, con desglose por condición.

## Catálogo y solvencia
Plaza (alcance), Molino (pánico), Archivo (credibilidad), Feria (distorsión/destinatario), Estación (profundidad/importancia), Asamblea final (alcance/importancia/diferencia de hojas). Noticias: cabra, molino, alcalde, luces y gallina. Cada plantilla contiene soluciones testigo verificadas mediante el mismo validador y simulador que usa el juego. No se permutan personalidades sin validación. La semilla decide variantes de ruta y noticias compatibles; las decisiones quedan registradas.

## Presentación y alcance
Pueblo cenital con edificios, calles y personajes vectoriales originales generados con UI; siluetas, iniciales y nombres de perfiles acompañan al color. Flechas distinguen conexiones; pulso, salto de personaje y mensaje muestran cada recepción. Cámara ortográfica, Canvas escalable y zona segura. Sonidos sintetizados locales, controles de volumen y velocidad. Sin servicios online ni monetización.

Incluye tutorial contextual, guardado versionado, ajustes, mapa, eventos, recompensas y finales. Fuera de alcance: fusiones, rumores rivales, físicas, tienda, progresión estadística permanente. Perfiles futuros: crédulo (amplifica credibilidad), escéptico (limita pánico), chismoso (premia bifurcaciones), pregonero (difusión amplia). La recompensa Pregonero del MVP modifica el límite del exagerado; no agrega una quinta personalidad.
