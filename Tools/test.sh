#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if [[ -n "${DOTNET:-}" ]]; then
  rumores_dotnet="$DOTNET"
elif command -v dotnet >/dev/null 2>&1; then
  rumores_dotnet="$(command -v dotnet)"
elif [[ -x /workspace/tools/dotnet/dotnet ]]; then
  rumores_dotnet=/workspace/tools/dotnet/dotnet
else
  echo 'Instalá .NET SDK 8 o definí DOTNET con la ruta a dotnet.' >&2
  exit 127
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1
"$rumores_dotnet" run --project Tests/Rumores.Tests.csproj --configuration Release
python3 Tools/validate_project.py
python3 Tools/validate_art.py
