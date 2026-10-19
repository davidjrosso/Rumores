# Validación de la fase 1

## Resultado ejecutado en esta entrega

- **81 pruebas C# .NET aprobadas, 0 fallidas:** 21 contratos party nuevos y 60 casos del legado conservados. El núcleo compila en .NET 8.0.426.
- Los casos party incluyen **432 rondas completas** (36 semillas × 2/8 participantes × 6 secuencias), además de casos dirigidos. No equivalen a 432 tests separados.
- **53 GUIDs y 6 assemblies** verificados estáticamente: Main, Legacy, catálogo party, prefab, fuente, paquetes fijados y escena de entrada.
- **25 archivos C# de Assets**, cero errores sintácticos con Roslyn C# 9. No verifica tipos/API de Unity.
- `git diff --check` sin errores al revisar la entrega.
- Intento de `Tools/unity.sh editmode`: detenido antes de ejecutar tests porque falta `UNITY_EDITOR`. Unity Editor/Hub no está instalado. **EditMode y PlayMode no ejecutados; compilación/importación completa, captura visual, APK y dispositivo no validados.**

## Cobertura del dominio

Catálogo/plantillas y contracciones; rondas de 2 y 8; orden y dos turnos por persona; diferencias numéricas y narrativas de cuatro personalidades; persistencia de la confusión; calmar/difundir; determinismo; prefijo e independencia de ramas; copias profundas; división sin puntos/turnos extra; rechazo de segunda división y división final; doble confirmación/avance y tokens obsoletos; comandos inválidos atómicos; apodos vacíos/duplicados/longitud y roster copiado; fórmula de puntos; contribución ajena sin penalización; variación cosmética sin premio repetido; tres objetivos; ganadores/empates; revancha sin estado anterior; límites numéricos y auditoría de puntos por evento.

Los 60 casos antiguos siguen pasando para el simulador, progresión y almacenamiento del modo Legacy. Esos sistemas no participan en Main.

## Comandos reproducibles

Desde la raíz del proyecto:

```bash
export DOTNET_CLI_HOME=/workspace/tools/dotnet-home
export DOTNET=/workspace/tools/dotnet/dotnet
bash Tools/bootstrap-dotnet.sh
bash Tools/test.sh
"$DOTNET" run --project Tools/SyntaxCheck/SyntaxCheck.csproj
"$DOTNET" run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md
```

En otra máquina usar el ejecutable .NET instalado allí. El [informe simulado](Docs/PARTY_VALIDATION.md) incluye puntuaciones, empates, titulares y tres historias reproducibles. Se corrigieron el sesgo de novedad global y contracciones incorrectas como «de el puente». Persisten saturación de atributos y frecuentes empates: son observaciones de balance, no fallos de estado.

## Pruebas Unity incluidas y pendientes

Abrir `Main.unity` con Unity 2022.3.62f1, **Window → General → Test Runner**:

- **EditMode:** contratos party y legado con catálogos cargados por JsonUtility.
- **PlayMode:** `TwoPlayersCompleteRoundThroughButtons` y `EightPlayersCompleteRoundThroughButtons`: menú, ayuda, ajuste, alta de jugadores, apodos vacíos/duplicados, avatar, turnos ordenados, confirmación desactivada sin selección, división, rama B, historial, doble callback de confirmar, resultados y revancha.
- PlayMode conserva dos comprobaciones de guardado del legado en directorios temporales. La prueba de ajustes puede cambiar la preferencia local de animación.

Con editor/licencia disponibles:

```bash
export UNITY_EDITOR=/ruta/al/ejecutable/Unity
bash Tools/unity.sh editmode
bash Tools/unity.sh playmode
```

Exigir un XML nuevo con casos ejecutados y resultado Passed. No afirmar éxito solo por un código cero del editor.

## Recorrido manual pendiente

1. Importar el proyecto y comprobar Console sin errores. Abrir Main y confirmar que aparecen Jugar, Cómo jugar y Ajustes.
2. Completar una ronda con dos personas y otra con ocho. Desplazar lista, editar nombres largos y comprobar límites. Intentar nombres vacíos/duplicados y corregirlos sin perder los otros participantes.
3. Pulsar rápidamente Confirmar y Siguiente; cada persona debe actuar exactamente dos veces. Abrir Historia y volver: la selección debe conservarse.
4. Dividir, modificar A y comprobar que B conserva su versión; modificar B y revisar ambas historias en resultados.
5. Comparar texto/atributos antes-después y sumar puntos. Comprobar ganador o empate compartido. Revancha debe empezar con cero puntos, una rama, otra noticia y los mismos apodos/avatares.
6. Revisar 720×1080, 720×1280 y una pantalla estrecha: tarjetas, botones, textos, listas desplazables, notch, teclado y nombres máximos. Probar fondo/retorno y cierre completo; este último pierde la ronda por diseño.
7. Jugar con personas para evaluar lectura, risas, ritmo y ventaja del orden. El simulador no prueba estas propiedades.

Arte definitivo y publicación Android quedan fuera del alcance actual. No hay errores conocidos en la compilación .NET ni en sintaxis/referencias estáticas; todavía no se puede certificar la compilación o ejecución Unity.
