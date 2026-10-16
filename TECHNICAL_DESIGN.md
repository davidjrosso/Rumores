# Diseño técnico

## Motor y dependencias
Proyecto nuevo: Unity 2022.3.62f1 LTS, pipeline integrado 2D, UGUI 1.0.0, Test Framework 1.1.33. C# compatible con Unity; núcleo sin UnityEngine. Se eligió una versión fija porque no existía proyecto. El SDK .NET 8 solo ejecuta las pruebas externas y no es una dependencia del juego.

## Capas
- Core: modelos serializables, catálogo, reglas de personalidades, validador de árboles, simulador BFS, objetivos, generador PRNG xorshift32, máquina de estados y DTO de guardado.
- Presentation: MonoBehaviour de flujo + UI, geometría de conexiones y avatares originales, sonidos locales; transforma eventos calculados en pulsos. No calcula reglas.
- Editor: menú de validación y construcción Android; escena principal serializada y registrada en EditorBuildSettings.
- Datos: TextAsset JSON referenciado por RumoresCatalog (ScriptableObject). IDs estables; posiciones explícitas. Personalidades, semántica, noticias, relaciones, objetivos, soluciones y recompensas editables en datos.
- Pruebas: suite pura compartida por consola .NET y EditMode; pruebas PlayMode para inicio, UI y guardado.

## Determinismo y separación
Los resultados no dependen del frame, cultura, reloj ni orden de listas de conexiones. Cada recepción crea su propia copia de Stats, etiquetas, afirmaciones, historial y recorrido. Los eventos conservan anterior/posterior y destinatarios. La red se valida antes de simular; un GameSession rechaza modificaciones fuera de Build. Simulation se presenta después del cálculo; resultado y paciencia ya están confirmados en el guardado.

El mapa tiene seis capas; dos nodos por capa salvo jefe. Cada nodo enlaza ambos nodos de la siguiente capa. Las alternativas tienen distinto contenido y algunas son eventos. PRNG explícito y versión de reglas 1; no usar System.Random para contenido. El log de decisiones registra nodo, red ordenada, resultado, eventos, reintentos y recompensa. RunReplay.Play ejecuta esas decisiones mediante GameSession y verifica el registro resultante. La reproducción requiere catálogo/versión, semilla y decisiones iguales. Las pantallas pendientes se restauran desde la fase del guardado.

## Persistencia
SaveData v1 incluye run, etapa, nodo, fase, conexiones, bonos, resultados, decisiones, ajustes y título desbloqueado. JsonUtility y archivo en Application.persistentDataPath. Escritura temporal, reemplazo con copia de respaldo, lectura con validación semántica de mapa, recorrido y resultados; el historial se reconstruye con el simulador; archivo dañado se recupera desde respaldo o se inicia estado seguro con mensaje. Una versión futura desconocida se preserva y bloquea la escritura. Se guarda después de cada edición y transición, en pausa y al salir. La animación no se persiste: al restaurar un resultado se muestra directamente. Reiniciar app en una pantalla de recompensa conserva la elección pendiente.

## Android
Application ID inicial com.davidjrosso.rumores, modificable en Player Settings o RUMORES_APPLICATION_ID al compilar. Portrait, minSdkVersion 23 (Android 6.0), target automático con SDK instalado; IL2CPP, ARM64, OpenGLES3, permiso de Internet no forzado. Estos son valores elegidos para el prototipo, no una afirmación de cumplimiento de Google Play. APK de desarrollo con firma de depuración. Compilación release/AAB, firma de distribución y política vigente de tienda quedan fuera de esta entrega.

## Recursos y rendimiento
10 avatares, menos de 30 conexiones por tablero. Simulación finita O(V+E), ejecutada al pulsar Simular. Reconstrucción de pantallas solo en interacción/transición, pulsos visuales con tiempos no escalados. UI en 720×1280 lógico; controles de al menos 64 unidades. Arte procedural sustituible sin modificar Core. AudioClip breve generado una vez. Perfilado real de CPU/GPU/memoria y validación táctil requieren Unity/dispositivo y se documentan en TEST_PLAN.md.
