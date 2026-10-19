# Fase 3 · personajes, iconos y efectos integrados

La muestra del commit `ecb4e80` fue probada por el usuario: «el arte y los sprites funcionaron bien, podes continuar». Esa confirmación permite ampliar el conjunto. **No equivale a ejecutar las pruebas automatizadas ni valida por anticipado los cambios de esta entrega.** Unity sigue sin estar instalado en cloud.

## Inventario real

| Recurso | Implementación | Dónde se usa |
|---|---|---|
| Pueblo | PNG original generado con IA: plaza, kiosco, club, panadería y estación | Menú y galería; ilustración completa, no mapa interactivo |
| Miedoso | Vecino nervioso con mate y chaleco celeste | Avatar y retrato |
| Exagerado | Vecina de pelo colorado, brazos abiertos y cardigan coral | Avatar y retrato |
| Mentiroso | Vecino de ceja levantada, rulo y chaqueta lila | Avatar y retrato |
| Ignorante | Vecina de lentes, pelo gris, libro y cardigan verde | Avatar y retrato |
| Marca | Dos globos vectoriales y palabra RUMORES editable | Menú |
| Seis iconos originales | Gota, megáfono, globo, interrogación, bombilla y risa; geometría uGUI | Cabecera de cartas, insignias de retrato y galería |
| Siete movimientos | Espera, habla, sorpresa, pánico, risa, celebración y desánimo con escala, giro y desplazamiento | Los cuatro personajes; todos disponibles en galería |
| Transmisión / división | Uno o dos globos con trayectorias cortas | Transmisión al confirmar; división solo en galería |
| Celebración / desánimo | Confeti de colores en los bordes y trazos celestes descendentes | Confeti al finalizar la ronda; desánimo solo en galería |

Los cuatro personajes tienen sprites propios. Los ocho avatares de jugadores reutilizan esos cuatro retratos con las dos variantes de marco existentes; no son ocho personajes distintos. Las antiguas caras geométricas quedan como respaldo si se elimina una referencia.

Las imágenes se generaron mediante `image_gen.imagegen`, con el miedoso aprobado como referencia de estilo para los otros tres. No se copiaron personajes de juegos existentes. Los iconos y efectos son dibujos por código, **no imágenes generadas con IA**. No se crearon spritesheets: las animaciones transforman el sprite completo, sin cambiar su expresión facial ni hacer sincronización labial.

## Cómo probar esta entrega

1. Abrir con **Unity 2022.3.62f1**, esperar la importación y abrir `Assets/_Rumores/Scenes/ArtPreview.unity`. Usar Game View **720×1280** y Play.
2. Tocar cada uno de los **cuatro retratos** de la galería. Comprobar que cambia el personaje grande y que la cabeza del avatar pequeño se ve sin recortes extraños.
3. Probar **Espera, Hablar, Sorpresa, Pánico, Risa, Festejo, Desánimo, Transmitir y Dividir** con cada personaje. Festejo agrega confeti y Desánimo trazos celestes. Probar también **Reducir movimiento**.
4. Detener Play, abrir `Assets/_Rumores/Scenes/Main.unity` y jugar. En **Jugar**, tocar los avatares para recorrer los ocho marcos/retratos; agregar ocho personas y desplazar la lista.
5. Las cartas muestran su icono junto al título. **Ver otras 3** permite ver los seis estilos. Confirmar y pulsar **Pasar el celu** durante una animación: debe responder inmediatamente.
6. Terminar una ronda, observar el confeti y pulsar **¡Otra!** antes de que termine. La nueva ronda debe empezar sin restos visuales ni cambios de turnos.
7. Repetir a **360×800** y **720×1080**; comprobar textos largos, títulos de cartas, área segura y recorte de avatares. Console debe permanecer sin errores.
8. Ejecutar **Window → General → Test Runner → EditMode / PlayMode → Run All**. Guardar el XML y capturas si aparece un problema.

ArtPreview es una escena de revisión, fuera de Build Settings. Main sigue siendo la entrada jugable. La división y el desánimo no introducen ramas ni derrotas en el modo sencillo. El core, las noticias y los turnos no cambiaron respecto de `ecb4e80`.

## Preparación e integración

Los PNG se conservan en `Assets/_Rumores/Resources/Art/Preview` para mantener las referencias de la muestra aprobada. Los personajes son **1024×1536 RGBA**; el pueblo es **1536×1024 RGB**. Los `.meta` configuran Sprite Single, 100 PPU, sRGB, Bilinear, Clamp, sin mipmaps, máximo 2048 y sin compresión. Pivote inferior centrado para personajes y centrado para el fondo.

| Personaje | Píxeles completamente transparentes |
|---|---:|
| Miedoso | 58,36% |
| Exagerado | 52,57% |
| Mentiroso | 58,61% |
| Ignorante | 54,96% |

Las cuatro esquinas son transparentes en cada personaje. La UI preserva proporciones; el retrato pequeño se recorta dentro de un RectMask2D sin modificar el PNG. El pueblo, los retratos y los iconos permanecen separados. Los textos importantes son componentes Text editables.

- `Resources/RumoresArt.asset`: catálogo con cuatro sprites distintos; referenciado por Main y ArtPreview.
- `CharacterView.cs`: retratos, avatares e insignia de personalidad.
- `ActionIconGraphic.cs`: seis iconos vectoriales; `PartyActionCard.cs` elige el icono por ID de acción sin cambiar las opciones.
- `ReactionBurstGraphic.cs`: confeti y trazos de desánimo; no captura pulsaciones, usa tiempo no escalado y se limpia al desactivar la pantalla.
- `ArtPreview.cs`: galería y controles de revisión. `PartyApp.cs`: conecta el festejo al cierre de la ronda.

Para sustituir un personaje basta importar otro sprite con los mismos ajustes y asignarlo en el slot correspondiente de `RumoresArt.asset`. Mantener los IDs `miedoso`, `exagerado`, `mentiroso` e `ignorante`. No hay que modificar las reglas ni crear código por personaje. Revisar márgenes y el recorte del rostro antes de sustituirlo. La [guía visual](DIRECCION_VISUAL.md) sigue definiendo paleta, proporciones y perspectiva.

## Evidencia y límites

- **Confirmado por el usuario:** arte y sprites de la muestra anterior funcionaron correctamente.
- **Ejecutado en esta entrega:** 83 pruebas .NET, 0 fallos; referencias de 71 GUIDs y 6 assemblies; sintaxis C# 9 de 35 archivos sin errores; integridad PNG, resolución, alfa y metadatos de los cinco PNG; Core y noticias sin cambios; `git diff --check`.
- **Ampliado, sin ejecutar en cloud:** importación de los tres sprites nuevos; cuatro referencias únicas; selector de personajes; asociación de los seis iconos sin consumir turno; limpieza de efectos y reducción de movimiento; CanvasRenderer obligatorio para los nuevos gráficos. Se mantienen las rondas de dos/ocho participantes y la regresión de lista enmascarada.
- **Pendiente:** compilación/importación y revisión visual de esta ampliación dentro de Unity. La confirmación de la muestra anterior no sustituye ese recorrido.
- **Alcance visual:** movimientos procedurales sencillos y UI de lectura sobria; no hay rig facial, fotogramas de boca, edificios independientes ni mapa navegable. No son necesarios para las reglas actuales. No se preparó ni publicó Android.

SHA-256 de los sprites añadidos:

```text
f0799278db6e755dc1195508e9c1996437ae67a2b40d12c844de63324aa3e707  Exagerado.png
ae4e16723b03e4aeb4dadd4350fc96929983dc8f56766c549129e47fa290616b  Mentiroso.png
7284c5d0d97ba0b5952c15f8bb153e81809a15e216d72c11b9f71dafbc97da04  Ignorante.png
```
