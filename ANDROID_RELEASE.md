# Android: compilación de prueba y preparación de publicación

Estado al **20 de octubre de 2026**: preparación de código/configuración realizada; **no se generó APK ni AAB**. No se ejecutó Unity, emulador ni dispositivo físico en este entorno. No es una certificación de compatibilidad con Google Play. Ver [resultados reales](TEST_PLAN.md) y [auditoría](Docs/FASE_5_QA.md).

## Configuración del proyecto

| Opción | Valor / comportamiento |
|---|---|
| Editor fijado | Unity 2022.3.62f1 |
| Aplicación / compañía | Rumores / David J. Rosso |
| Identificador inicial | `com.davidjrosso.rumores`, configurable |
| Versión inicial / código | `0.1.0` / `1`; se conservan al configurar, no se incrementan solos |
| Escena incluida | `Assets/_Rumores/Scenes/Main.unity` |
| Orientación / UI | Retrato; referencia 720×1280, escala por ancho, `Screen.safeArea` |
| Plataforma | ARM64, IL2CPP, OpenGLES3, mínimo API 23 |
| Target API | Auto para la prueba inicial; **obligatoriamente explícito para el AAB** |
| Rendimiento | Límite de 60 FPS; pendiente de medir FPS, memoria, temperatura y consumo reales |
| Icono | `Assets/_Rumores/Art/AppIcon.png`, original generado con la herramienta de imágenes; fuente opaca 1254×1254, importación limitada a 1024, sin mipmaps ni compresión |
| Firma APK | Depuración, temporalmente; no usa la clave de publicación |
| Firma AAB | Clave personalizada elegida manualmente; el comando falla si falta |

El icono está asignado como icono predeterminado en Player Settings y el configurador vuelve a conectarlo. Falta comprobar sus variantes de launcher en un dispositivo; no se crearon capas adaptativas/monocromáticas específicas. **La imagen es un recurso real, no una captura de una APK funcionando.**

`Main` sigue siendo el modo sencillo: dos vueltas, historia compartida y comparación original/final. No tiene puntos ni ganador. La API interna de ramas y el legado se conservan, pero sus escenas no se incluyen en la compilación.

## 1. Preparar Unity y verificar el proyecto

1. En Unity Hub, instalar **2022.3.62f1** con **Android Build Support**, **Android SDK & NDK Tools** y **OpenJDK** correspondientes a esa instalación. Activar Unity mediante el procedimiento normal de tu licencia. No introducir credenciales en scripts ni en este repositorio.
2. Añadir la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`. Esperar la importación y resolución de paquetes. `manifest.json` fija uGUI 1.0.0 y Test Framework 1.1.33. No hay `packages-lock.json` generado por Unity todavía: revisar y conservar el que produzca una resolución exitosa; no fabricarlo manualmente.
3. Abrir `Main.unity`, ejecutar **Rumores → Validar contenido** y comprobar que Console no tenga errores. Ejecutar **Window → General → Test Runner → EditMode / PlayMode**. Después recorrer una partida con 2 y otra con 8 personas.
4. Abrir **File → Build Settings → Android → Switch Platform** y esperar a que termine. El compilador del menú exige Android ya activo para evitar una recarga de scripts a mitad del método.
5. En **Edit → Preferences → External Tools**, usar el SDK, NDK y JDK instalados con ese editor. El Java 21 del entorno cloud por sí solo no permite compilar Unity Android; no reemplazar arbitrariamente las herramientas de Hub.
6. Ejecutar **Rumores → Configurar Android**. Revisar **Player Settings → Android**: identificador, versión, Bundle Version Code, retrato, IL2CPP/ARM64, icono y API. La configuración conserva identificador/versión/API y firma elegidos; no reinicia la versión ni desactiva la clave personalizada.

Antes de publicar, revisar también los avisos de seguridad y soporte del editor. Se conserva la versión existente para no migrar el proyecto sin validación; **no se verificó que este parche sea adecuado para una publicación actual**. Si hace falta actualizarlo, hacerlo en un cambio separado, con copia del proyecto y repetición completa de importación, tests, APK y pruebas de dispositivo.

## 2. Generar la APK de prueba

Con Android ya seleccionado, usar **Rumores → Compilar APK de desarrollo**.

Salida esperada: **`Builds/Rumores-dev.apk`**, relativa al proyecto. El comando elimina la salida anterior, exige un BuildReport exitoso y un archivo no vacío, y elimina una salida parcial si falla. La firma de depuración y el modo APK se usan durante la compilación; luego se restauran las opciones anteriores de firma/formato. La APK de desarrollo no es el artefacto para publicar.

Alternativa desde Bash, con editor instalado y licencia activa:

```bash
export UNITY_EDITOR="/ruta/real/al/ejecutable/Unity"
bash Tools/unity.sh editmode
bash Tools/unity.sh playmode
bash Tools/unity.sh android
```

También se admite `apk`. Los logs y XML quedan en `TestResults/`, ignorado por Git. El script rechaza resultados XML antiguos, cero pruebas y APK antiguas/vacías. Su prueba automatizada usa un ejecutable ficticio en una carpeta temporal: **no genera una APK real ni valida el compilador de Unity**.

Si falla, leer `TestResults/android-build.log` y la Console. Resolver el primer error de importación, C#, módulo o Gradle y volver a compilar. No aceptar una APK de otro intento como prueba de éxito. No se incluye una descarga de APK en esta entrega porque el archivo no existe.

## 3. Instalar y probar

Usar un teléfono **ARM64**, Android API 23 o superior; esto describe el mínimo configurado, no todos los dispositivos comprobados. Un emulador x86 no coincide con el ABI elegido.

Activar depuración USB solo en un equipo de confianza, autorizar el dispositivo y ejecutar con el `adb` del SDK:

```bash
adb devices
adb install -r Builds/Rumores-dev.apk
```

Abrir **Rumores** desde el launcher. Si Android informa firma incompatible con una instalación previa, no desinstalar a ciegas: desinstalar borra sus datos locales. Decidir si se puede descartar esa instalación o usar otro identificador de prueba. Mantener el identificador final estable una vez publicada la app.

Registrar en [TEST_PLAN.md](TEST_PLAN.md): modelo, Android/API, versión/código, commit, resolución, navegación por gestos/botones, resultado y log. Probar:

- Recorrido completo con 2 y 8 participantes, teclado en el último apodo, corrección de duplicados/vacíos y cambio de avatar.
- Tres opciones, otras tres, historial, confirmación doble, avance doble, segundo turno de cada persona, resultados, revancha y menú/continuación.
- Inicio/Home del sistema y reanudación durante elección y revelación: ocultar contenido con la pantalla de entrega; conservar turno y aporte.
- 16:9, 20:9 y pantalla/tablet más ancha; notch y barras del sistema; leer textos largos y pulsar controles sin solapamientos. Ver matriz de tamaños en TEST_PLAN.
- Música y efectos independientes, silencio, movimiento reducido y persistencia de esas preferencias al cerrar/abrir.
- **Modo avión desde antes de abrir:** menú, ronda, audio, resultados y revancha. El análisis del código no encontró servicios remotos obligatorios, pero esta comprobación todavía no se ejecutó.
- Reinicio del proceso: la ronda se pierde por diseño; ajustes permanecen. Android puede terminar el proceso en segundo plano, por lo que no hay garantía de reanudar una ronda después de ese cierre.
- Icono con distintos launchers, audio real, batería y memoria con Profiler/Android Logcat. Conservar ANR/crashes si aparecen, sin publicar apodos u otros datos personales en logs compartidos.

## 4. Verificar requisitos actuales de Google Play

**Bloqueo de consulta:** el 2026-10-20 se intentó acceder por HTTPS a las siguientes páginas oficiales. El proxy respondió `Tunnel connection failed: 403 Forbidden`; no se obtuvo el contenido. Las URLs son fuentes a revisar, **no citas de políticas verificadas**. No se fija aquí un Target API “vigente” ni un número obligatorio de testers/días.

| Fuente oficial intentada | Qué verificar antes de release |
|---|---|
| [Target API de Google Play](https://support.google.com/googleplay/android-developer/answer/11926878) | API exigida para nuevas apps/actualizaciones y fechas/plazos aplicables |
| [Tamaños de página Android](https://developer.android.com/guide/practices/page-sizes) | Requisitos de 16 KB, bibliotecas nativas IL2CPP, herramientas de comprobación y pruebas aplicables |
| [Compatibilidad Android de Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/android-requirements-and-compatibility.html) | SDK/NDK/JDK y compatibilidad del editor con la API requerida |
| [Build Settings Android de Unity 2022.3](https://docs.unity3d.com/2022.3/Documentation/Manual/android-build-settings.html) | Opciones de APK/AAB y firma de esa versión |
| [Firma de apps Android](https://developer.android.com/studio/publish/app-signing) | Diferencia entre upload key, app signing key y Play App Signing |
| [Requisitos de prueba de cuentas personales](https://support.google.com/googleplay/android-developer/answer/14151465) | Si se aplican a tu cuenta y qué acceso a producción exige Console |
| [Aviso de seguridad de Unity](https://unity.com/security/sept-2025-01) | Parches afectados/corregidos y recomendaciones actuales; no se verificó el estado de 2022.3.62f1 |

Abrir las fuentes desde un navegador con acceso y registrar fecha y requisito en esta tabla antes de producir un candidato a publicación. Contrastar con las advertencias concretas de Play Console para esa app/cuenta. Si hay incompatibilidad del editor o sus librerías nativas, actualizar al parche recomendado y repetir QA; cambiar solo el número de Target API **no demuestra** compatibilidad ni alineación de 16 KB.

## 5. Crear un AAB firmado, sin guardar secretos en Git

Este procedimiento queda preparado; **no se ejecutó**:

1. Completar las verificaciones oficiales anteriores. Instalar la plataforma SDK requerida usando las herramientas compatibles con Unity y seleccionar un **Target API explícito** en Player Settings, no `Automatic`.
2. Confirmar el identificador definitivo y elegir `Version` y `Bundle Version Code`. El código debe superar los ya usados para la app según lo que muestre Console. El configurador no lo incrementa automáticamente.
3. En **Player Settings → Android → Publishing Settings → Keystore Manager**, crear o seleccionar la **upload key** que corresponda al alta/actualización. Guardar el keystore **fuera de la carpeta del proyecto**, con copia de seguridad cifrada y acceso restringido. No perder el alias ni sustituir una clave de una app existente sin seguir el procedimiento oficial.
4. Activar `Custom Keystore`, elegir el archivo con ruta absoluta y el alias, e introducir las contraseñas **solo en los campos del editor**, usando un gestor de contraseñas. Nunca pegarlas en chat, argumentos de terminal, variables versionadas o documentos. El código no solicita, genera, imprime ni escribe contraseñas.
5. Ejecutar **Rumores → Compilar AAB firmado**. Se exige configuración de firma completa y Target API explícito. Se genera **`Builds/Rumores-release.aab`** con `BuildOptions.None`, sin la opción Development. Se restauran las preferencias anteriores de formato/firma al terminar o fallar. La existencia de un AAB no implica aprobación de Google Play.
6. Verificar la firma con las herramientas del JDK/SDK compatibles (por ejemplo, `jarsigner -verify -verbose -certs Builds/Rumores-release.aab`) y el certificado de carga esperado. Revisar manifiesto final: package, versión/código, min/target API, ABI y permisos realmente incluidos. Comprobar bibliotecas nativas/alineación y 16 KB siguiendo la guía oficial vigente.
7. Probar el bundle mediante la herramienta oficial correspondiente o, manualmente, el canal de pruebas autorizado de Play Console. Un AAB no se instala con `adb install` como una APK. Probar también la entrega firmada de Play, porque no es idéntica a la APK de depuración.

Los overrides opcionales del configurador son **datos no secretos**: `RUMORES_APPLICATION_ID`, `RUMORES_VERSION`, `RUMORES_VERSION_CODE`, `RUMORES_TARGET_API`. Configurarlos antes de iniciar Unity si se usan. En su ausencia se respetan Player Settings. Un valor inválido falla antes de modificar esos ajustes. No hay ruta CLI para suministrar contraseñas de firma; usar el menú del editor.

`Builds/`, logs, `.env*`, `*.keystore`, `*.jks`, `*.p12`, `*.pfx`, `*.pem` y `*.key` están ignorados. Esto es una defensa adicional: no meter claves dentro del proyecto ni confiar en extensiones para proteger un secreto.

## 6. Pasos manuales antes de publicar

- Resolver importación/compilación Unity y completar pruebas de dispositivo y del AAB firmado. Registrar evidencias y corregir errores pendientes.
- Verificar API, compatibilidad del editor/16 KB, firma y requisitos de acceso a producción de **tu cuenta**, sin asumir plazos o excepciones.
- Revisar la ficha, categoría, audiencia, clasificación de contenido y declaraciones de seguridad de datos según el comportamiento real del binario y las reglas que muestre Console. El proyecto no agrega analítica, anuncios ni conexiones de gameplay; comprobar permisos del manifiesto final y las opciones/servicios del editor antes de declarar datos recolectados.
- Revisar política de privacidad y demás declaraciones que correspondan. No afirmar cumplimiento legal a partir de esta auditoría técnica.
- Crear capturas reales de la build y recursos exigidos por la ficha. El icono de launcher no sustituye automáticamente todos los recursos de Play. Conservar atribuciones/licencias de fuentes y audio; documentar la procedencia de las ilustraciones.
- Subir manualmente al canal de prueba elegido, revisar informes automáticos y probar instalaciones/actualizaciones. La publicación en producción requiere una decisión posterior del usuario.

No se accedió a cuentas, no se subió ningún artefacto y no se publicó la aplicación.
