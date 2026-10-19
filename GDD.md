# Rumores · prototipo party

## Objetivo y alcance

De dos a ocho participantes transforman una noticia absurda en un único dispositivo. El humor surge de las versiones y de las contradicciones visibles en el historial. Unity 2022.3.62f1, C#, vertical, sin conexión. Esta fase usa placeholders; no incluye progreso persistente, inventarios, monedas, mejoras ni publicación Android.

## Recorrido

Menú → participantes → entrega del teléfono → elegir acción/rama → confirmar → leer cambio y puntos → siguiente participante → resultados → revancha. Cada persona juega dos veces en el orden de registro. Las acciones son públicas; la entrega del teléfono identifica claramente quién actúa.

Hay ocho avatares provisionales (cuatro caras con dos colores) independientes de las reglas. Todos pueden elegir las cuatro personalidades en cada turno. No se asignan ventajas permanentes por avatar.

## Acciones

- **Entrar en pánico / miedoso:** convierte una señal en amenaza; sube especialmente el pánico.
- **Exagerar / exagerado:** convierte el incidente en consecuencias enormes; sube especialmente la importancia.
- **Mentir / mentiroso:** inventa un detalle ligado a la noticia y lo marca como invención; pierde credibilidad.
- **Malinterpretar / ignorante:** sustituye al protagonista por una confusión absurda; ese cambio persiste en los textos posteriores.
- **Calmar:** pide pruebas, reduce la alarma y recupera credibilidad.
- **Difundir:** lleva el rumor a cuatro vecinos más.

La tarjeta resume el efecto y, al seleccionarla, aparece una explicación adicional antes de confirmar. El resultado muestra texto, razón, atributos antes/después y desglose de puntos. Las plantillas son locales y deterministas.

## Ramas y narración

La versión inicial circula por A, la plaza. Una confirmación puede enviar una copia al mercado, creando B. Ambas conservan el prefijo de historia hasta esa confirmación. Desde entonces cada acción afecta solo a la rama seleccionada. Máximo dos ramas, sin fusiones, y debe quedar al menos otro turno para poder crear B.

El titular es la última transformación. El historial conserva todas las invenciones, confusiones y rectificaciones anteriores; la presentación no pretende concatenarlas en una frase cada vez más larga. En resultados se ve el original junto a las versiones finales y sus historias.

## Ronda y victoria

Un objetivo común pide aumentar el pánico, conservar credibilidad o alcanzar vecinos nuevos. La fórmula otorga hasta tres puntos por turno: aporte propio nuevo en la rama, acción no usada por esa persona y objetivo cumplido. Ver [BALANCE.md](BALANCE.md). Gana la puntuación mayor; los empates son victorias compartidas.

La revancha conserva apodos/avatares, cambia la noticia y reinicia todos los puntos, acciones usadas, historial y ramas. La ronda vive en memoria. Solo se guarda la preferencia de animación; no hay progreso persistente.

## Contenido y siguiente validación

Doce noticias, tres variantes por acción y dos malentendidos por noticia, configurados en `PartyContent.json`. La frase no necesita conexión ni generación remota. Se prioriza probar con personas el ritmo, la comprensión del objetivo y la frecuencia de empates. El arte definitivo y la publicación Android quedan fuera de esta fase.
