# Rumores · dirección visual v1

Muestra inicial para validar en Unity antes de producir el conjunto completo. Gameplay, noticias, turnos y opciones permanecen intactos.

## Identidad: un barrio que cuenta todo

Caricatura 2D cálida y dibujada, con siluetas simples y caras expresivas. Pueblo argentino inventado: plaza, kiosco, club, panadería, veredas y toldos. Los objetos cotidianos dan identidad; evitar saturar la imagen con símbolos nacionales o memes escritos. Sin personajes, composiciones ni interfaces copiados de otros juegos.

## Paleta

| Uso | Color |
|---|---|
| Tinta y contorno | `#243B46` |
| Papel y espacios de lectura | `#FFF3D5` |
| Verde petróleo / botones | `#297F78` |
| Coral / acentos y selección | `#ED7656` |
| Amarillo / sorpresa y alegría | `#F3C95B` |
| Celeste / ropa del miedoso | `#83C7DE` |
| Lila / ropa del mentiroso | `#AF96CD` |
| Verde claro / vegetación | `#AFCD87` |

Usar tinta sobre papel y papel sobre petróleo. Coral y amarillo son acentos, nunca el único indicador de selección. Dejar zonas tranquilas detrás de los textos.

## Personajes

Cuerpos de 2,5 cabezas, cabeza de aproximadamente 40% de la altura, pies grandes y manos legibles. Vista frontal de tres cuartos en retratos; dentro del pueblo, perspectiva cenital coherente con el escenario. No usar el retrato frontal como ficha de escala humana sobre el mapa.

- Miedoso: silueta de pera, hombros elevados, cejas arqueadas, mate sujeto con ambas manos; celeste.
- Exagerado: silueta triangular invertida, brazos abiertos, boca y cejas amplias; coral.
- Mentiroso: silueta inclinada y compacta, sonrisa lateral, ceja levantada; lila.
- Ignorante: silueta redondeada, cabeza ladeada, mirada distraída y lentes grandes; verde.

Expresiones definidas por cejas, ojos y boca; sin detalles faciales finos. Mantener contorno y accesorio reconocibles a 80–120 px. Avatares reutilizan retratos con marco y una variante de color del marco; los apodos permanecen como texto editable.

## Pueblo y perspectiva

Cenital ilustrada con una ligera inclinación uniforme: predominan techos y suelos, fachadas cortas orientadas hacia abajo, sin horizonte ni perspectiva con puntos de fuga. Plaza central despejada, calles simples y edificios de siluetas distintas. Luz desde arriba a la izquierda y sombras cortas hacia abajo a la derecha. No dibujar texto en carteles: el juego debe poder superponer etiquetas editables.

El escenario de la muestra es una ilustración de conjunto para el menú. La separación en edificios, props y personajes cenitales se hará después de validar la muestra. No afirmar que el fondo ya es un mapa interactivo.

## Escala e importación

Fuentes PNG en resolución alta; personajes RGBA con margen transparente de 8–12%, escenario opaco. Sprites 2D Single, sRGB, 100 píxeles por unidad, filtrado bilinear, wrap Clamp, sin mipmaps y sin compresión durante la validación. Pivote del personaje en centro inferior; fondo centrado. UI mantiene proporciones con `preserveAspect`.

Contorno exterior equivalente a 4–6 px en un retrato de 256 px; interiores de 2–3 px. Sombras planas de una sola intensidad, iluminación común, textura de papel discreta. Evitar gradientes fotográficos, iluminación dramática y microdetalle. No colorear el fondo transparente con un tablero de ajedrez.

## UI, iconos y movimiento

Marca: dos globos de conversación que se superponen, más la palabra RUMORES como texto editable. Iconos de 24–48 px con silueta clara: gota para susto, megáfono para exageración, burbuja con puntos para invención, interrogación para confusión, bombilla para teoría, carcajada para meme. No incrustar palabras ni números importantes en ilustraciones.

Movimientos cortos de squash/stretch y rotación moderada: espera 2 s; habla/transmisión 0,45 s; sorpresa 0,3 s; pánico 0,45 s; risa 0,45 s; celebración 0,65 s; desánimo 0,5 s. El siguiente botón siempre responde, incluso durante una animación. Cancelar/restaurar al cambiar de pantalla. Modo rápido reduce movimiento. No modificar el simulador desde una animación.

La ronda actual no tiene derrota ni bifurcación visible: preparar esas reacciones en la muestra visual sin inventar condiciones de derrota ni reactivar reglas retiradas.

## Puerta de validación

Primero un fondo y el miedoso, conectados a Main y a una pantalla de revisión dentro del juego. Comprobar importación, alfa, tamaño pequeño, recorte en lista, lectura, reacción y transición. Solo después ampliar a otros tres personajes, edificios separados, iconos finales y variantes de expresión.

En este entorno se pueden generar imágenes y ejecutar .NET, pero no hay Unity Editor instalado. La inspección de un PNG y la validación estática no sustituyen esta puerta. No declarar aprobada la dirección ni terminada la fase 3 sin una ejecución real del proyecto.
