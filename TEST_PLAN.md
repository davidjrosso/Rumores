# Validación del modo sencillo argentino

## Ejecutado

- **78 pruebas .NET aprobadas, 0 fallidas:** 18 contratos del modo sencillo y los 60 casos del legado. Las pruebas de puntuación anteriores se reemplazaron porque esa regla fue retirada por pedido del usuario.
- Incluye **360 rondas**: 60 semillas × 2/8 amigos × 3 posiciones de respuesta. Una historia compartida, exactamente dos aportes por persona y final accesible. Además, 60 rondas dirigidas comprueban que cada vista previa coincide con su confirmación.
- 20 noticias con preguntas, frases breves, cuatro estilos diferentes y contracciones correctas. Tres opciones únicas por turno, rotación de los cuatro estilos, preview repetida sin mutaciones, semilla determinista incluso en extremos, confirmación/avance duplicados rechazados, apodos inválidos y revancha limpia.
- Se mantienen pruebas de copia profunda e independencia de la API interna de bifurcación, aunque Main ya no ofrece esa opción.
- Validación estática: **53 GUIDs, 6 assemblies**, escena, catálogo, prefab y paquetes.
- Sintaxis C# 9: **25 archivos de Assets, cero errores**. No comprueba los tipos de Unity.

El [informe narrativo](Docs/PARTY_VALIDATION.md) registra ejemplos reales de la simulación: mate, bondi, empanada y facturas. Hubo 360 finales diferentes y cero repeticiones textuales exactas dentro de una ronda en esa muestra; pueden repetirse ideas con otra redacción.

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

Unity Editor/Hub no está instalado. **No se ejecutaron EditMode ni PlayMode, ni la compilación/importación completa, renderizado o Android.**

- EditMode ejecuta los mismos contratos con JsonUtility.
- PlayMode abre Main con 2 y 8 personas, recorre ayuda/ajustes/roster, rechaza apodos inválidos, comprueba tres tarjetas y ausencia de botones de ramas, verifica el texto de preview, preserva selección al consultar historial, rechaza doble confirmación y alcanza cierre/revancha.
- `CustomGraphicsCreateTheirRequiredRenderer` verifica AvatarGraphic y BoardGraphics. `RosterAvatarsSurviveClippingRebuildAndReturnToMenu` abre y reconstruye la lista enmascarada tres veces, conservando la regresión del error CanvasRenderer.
- Las dos pruebas de almacenamiento Legacy usan carpetas temporales. El caso UI cambia la preferencia local de animación: usar perfil de prueba.

Con editor/licencia disponibles: Window → General → Test Runner → EditMode y PlayMode, o definir `UNITY_EDITOR` y usar `Tools/unity.sh editmode` / `playmode`. Exigir XML del intento actual y tests ejecutados.

## Revisión manual pendiente

1. Main → Play → Jugar. Lista de 2 y 8 amigos; editar apodos, cambiar avatar y desplazarse sin errores.
2. Leer pregunta y tres respuestas. Seleccionar, releer «Lo anterior», volver y confirmar: el texto debe coincidir. Leerlo en voz alta y pasar el celu.
3. Al terminar dos vueltas, comprobar original/final, pregunta social e historial. «¡Otra!» limpia historia, cambia noticia y conserva amigos.
4. Verificar que no aparezcan atributos, puntos, objetivos ni controles de ramas en ninguna pantalla del modo actual.
5. Revisar 720×1080, 720×1280 y teléfono estrecho, especialmente textos largos de tarjetas, teclado y área segura.
6. Probar humor, ritmo y repetición de ideas con personas. No hay estadísticas de diversión inventadas.

No se produjo arte definitivo ni se publicó Android.
