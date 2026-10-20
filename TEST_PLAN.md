# Validación del modo sencillo argentino

## Ejecutado

- **83 pruebas .NET aprobadas, 0 fallidas:** 23 contratos del modo sencillo y los 60 casos del legado. Las pruebas de puntuación anteriores se reemplazaron porque esa regla fue retirada por pedido del usuario.
- Incluye **1296 rondas**: 108 semillas × 2/8 amigos × 6 posiciones de respuesta. Una historia compartida, exactamente dos aportes por persona y final accesible. Además, 108 rondas dirigidas comprueban que cada vista previa coincide con su confirmación.
- 36 noticias con preguntas, 648 continuaciones propias y seis estilos diferentes. Tres opciones por página, dos páginas disjuntas y rotación de los seis estilos, variantes nuevas al repetir una noticia en otro ciclo, preview repetida sin mutaciones, semilla determinista incluso en extremos, confirmación/avance duplicados rechazados, apodos inválidos y revancha limpia.
- **23.328 comprobaciones de enlaces**: 36 noticias × 18 detalles previos × 6 estilos × 6 semillas. Cada resultado debe conservar episodio y aporte anterior, usar una opción de la misma noticia y caber en 240 caracteres. Se prueban tres variantes sin repetir por estilo, agotamiento sin repetición inmediata y rechazo de opciones inválidas, duplicadas o demasiado largas. Esto no demuestra sentido común por sí solo.
- Se mantienen pruebas de copia profunda e independencia de la API interna de bifurcación, aunque Main ya no ofrece esa opción.
- Validación estática: **89 GUIDs, 6 assemblies**, escenas, catálogo, prefab, biblioteca de cuatro personajes y paquetes. Integridad PNG, dimensiones y alfa de personajes/pueblo. Ocho WAV y 89 textos de interfaz verificados con `Tools/validate_ux.py`.
- Sintaxis C# 9: **42 archivos de Assets, cero errores**. No comprueba los tipos de Unity.

El [informe narrativo](Docs/PARTY_VALIDATION.md) registra ejemplos reales de la simulación: mate, TikTok, stream, café, mufa, MSN, videoclub, QR y acto patrio. Hubo 1080 finales diferentes, 11.664 aportes enlazados y cero reutilizaciones de un detalle dentro de una ronda en esa muestra; pueden repetirse ideas con otra redacción.

## Repetir las comprobaciones

```bash
export DOTNET_CLI_HOME=/workspace/tools/dotnet-home
export DOTNET=/workspace/tools/dotnet/dotnet
bash Tools/test.sh
"$DOTNET" run --project Tools/SyntaxCheck/SyntaxCheck.csproj
"$DOTNET" run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md
```

En otra máquina, usar el SDK .NET 8 local. `Tools/bootstrap-dotnet.sh` reproduce la instalación cloud con SHA-512 verificado.

## Incluido, pendiente de Unity

Unity Editor/Hub no está instalado en cloud. **No se ejecutaron aquí EditMode ni PlayMode, compilación/importación completa, renderizado o Android.** El usuario confirmó el funcionamiento del arte y los sprites de la muestra anterior; la ampliación de personajes/iconos/efectos requiere su propia revisión.

- EditMode ejecuta los mismos contratos con JsonUtility.
- PlayMode abre Main con 2 y 8 personas, recorre ayuda/ajustes/roster, rechaza apodos inválidos, comprueba tres tarjetas y ausencia de botones de ramas, alterna «Ver otras 3» sin avanzar el turno y limpia la selección anterior, verifica el texto de preview, preserva selección al consultar historial, rechaza doble confirmación y alcanza cierre/revancha.
- `CustomGraphicsCreateTheirRequiredRenderer` verifica AvatarGraphic y BoardGraphics. `RosterAvatarsSurviveClippingRebuildAndReturnToMenu` abre y reconstruye la lista enmascarada tres veces, conservando la regresión del error CanvasRenderer.
- Las dos pruebas de almacenamiento Legacy usan carpetas temporales. El caso UI cambia la preferencia local de animación: usar perfil de prueba.
- Arte: importación de cinco PNG, cuatro personajes distintos, cambio de personaje en galería, siete movimientos, asociación de seis iconos a cartas, efectos que no capturan pulsaciones y se limpian al desactivar. Dependencias CanvasRenderer de SpeechBubbleGraphic, ActionIconGraphic y ReactionBurstGraphic. Avance/revancha después de la transición breve, sin esperar las animaciones largas.
- Fase 4: 17 casos nuevos de importación de audio, formato de textos, bloqueo durante transiciones, privacidad al pausar, doble pulsación, volúmenes independientes/persistencia, tarjetas largas y cinco tamaños de layout. Todos requieren Unity y siguen sin ejecutar aquí.

Con editor/licencia disponibles: Window → General → Test Runner → EditMode y PlayMode, o definir `UNITY_EDITOR` y usar `Tools/unity.sh editmode` / `playmode`. Exigir XML del intento actual y tests ejecutados.

## Revisión manual pendiente

1. Main → Play → Jugar. Lista de 2 y 8 amigos; editar apodos, cambiar avatar y desplazarse sin errores.
2. Leer pregunta y tres respuestas. Tocar «Ver otras 3» y comprobar alternativas distintas, sin consumir turno. Seleccionar, releer «Lo anterior», volver y confirmar: el texto debe coincidir. Leerlo en voz alta y pasar el celu.
3. Al terminar dos vueltas, comprobar original/final, pregunta social e historial. «¡Otra!» limpia historia, cambia noticia y conserva amigos.
4. Verificar que no aparezcan atributos, puntos, objetivos ni controles de ramas en ninguna pantalla del modo actual.
5. Revisar 720×1080, 720×1280 y teléfono estrecho, especialmente textos largos de tarjetas, teclado y área segura.
6. Probar humor, ritmo, continuidad del relato y repetición de ideas con personas. No hay estadísticas de diversión inventadas.

Para el recorrido de arte, usar [PRUEBA_UNITY.md](Docs/Art/PRUEBA_UNITY.md). Para UI, teclado, audio y resoluciones, usar [FASE_4_UI_AUDIO.md](Docs/FASE_4_UI_AUDIO.md). No se publicó Android.
