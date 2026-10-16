#!/usr/bin/env bash
set -euo pipefail
rumores_root="${RUMORES_DOTNET_ROOT:-/workspace/tools/dotnet}"
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/workspace/tools/dotnet-home}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
if [[ ! -x "$rumores_root/dotnet" ]] || [[ "$("$rumores_root/dotnet" --version)" != "8.0.426" ]]; then
  if [[ "$(uname -s)" != Linux || "$(uname -m)" != x86_64 ]]; then
    echo 'Este instalador fija Linux x64. Instalá .NET SDK 8 para tu plataforma.' >&2
    exit 1
  fi
  rumores_download="$(mktemp -d)"
  trap 'rm -rf "$rumores_download"' EXIT
  curl --proto '=https' --tlsv1.2 --fail --silent --show-error --location 'https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.426/dotnet-sdk-8.0.426-linux-x64.tar.gz' -o "$rumores_download/sdk.tar.gz"
  (cd "$rumores_download" && printf '%s  sdk.tar.gz\n' '8141d01bdc53f604aed3bb661b604707a64209f8a947cd81bf95b4f518951593d77864069191f54ea5e84362a409da6b0f69074bf893b283d7e0cf93e2bf20a8' | sha512sum --check --status)
  mkdir -p "$rumores_root"
  tar -xzf "$rumores_download/sdk.tar.gz" -C "$rumores_root"
fi
"$rumores_root/dotnet" --version
