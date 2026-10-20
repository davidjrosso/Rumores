#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${UNITY_EDITOR:?Definí UNITY_EDITOR con la ruta al ejecutable Unity 2022.3.62f1. Requiere licencia activa.}"
if [[ ! -x "$UNITY_EDITOR" ]]; then
  echo 'UNITY_EDITOR no apunta a un ejecutable. Instalá Unity y sus módulos Android desde Unity Hub.' >&2
  exit 127
fi
mkdir -p TestResults
mode="${1:-editmode}"
case "$mode" in
  editmode|playmode)
    result="TestResults/$mode.xml"
    rm -f "$result"
    "$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform "$mode" -testResults "$PWD/$result" -logFile "$PWD/TestResults/$mode.log"
    python3 - "$result" <<'PY'
import sys,xml.etree.ElementTree as ET
r=ET.parse(sys.argv[1]).getroot()
total=int(r.attrib.get('total','0')); failed=int(r.attrib.get('failed','0'))
print(r.attrib)
if total==0 or failed or r.attrib.get('result')!='Passed':sys.exit(1)
PY
    ;;
  android|apk)
    mkdir -p Builds
    rm -f Builds/Rumores-dev.apk
    status=0
    "$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod Rumores.Editor.RumoresBuild.Android -logFile "$PWD/TestResults/android-build.log" || status=$?
    if [[ "$status" != 0 ]]; then rm -f Builds/Rumores-dev.apk; exit "$status"; fi
    test -s Builds/Rumores-dev.apk || { echo 'Unity no produjo una APK nueva.' >&2; exit 1; }
    ;;
  *) echo 'Uso: Tools/unity.sh editmode|playmode|android|apk. AAB firmado: menú Rumores en el editor (contraseñas solo en su UI).' >&2; exit 2 ;;
esac
