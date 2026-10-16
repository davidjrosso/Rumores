#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${UNITY_EDITOR:?Definí UNITY_EDITOR con la ruta al ejecutable Unity 2022.3.62f1. Requiere licencia activa.}"
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
  android)
    "$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod Rumores.Editor.RumoresBuild.Android -logFile "$PWD/TestResults/android-build.log"
    test -s Builds/Rumores-dev.apk
    ;;
  *) echo 'Uso: Tools/unity.sh editmode|playmode|android' >&2; exit 2 ;;
esac
