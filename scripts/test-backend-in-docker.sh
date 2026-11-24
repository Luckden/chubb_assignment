#!/usr/bin/env sh
# Runs the backend test suite inside the .NET SDK container.
# Use this where the host can't run the test DLLs (e.g. Windows Smart App Control blocks unsigned local builds).
# Testcontainers inside the container uses the host's Docker through the mounted socket.
set -eu

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
BACKEND="$ROOT/backend"
# Git Bash on Windows: give Docker a Windows path and stop MSYS rewriting container paths.
if command -v cygpath >/dev/null 2>&1; then BACKEND="$(cygpath -w "$BACKEND")"; export MSYS_NO_PATHCONV=1; fi

docker run --rm \
  -v "$BACKEND:/src:ro" \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -e TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  sh -c 'cp -r /src /work && cd /work && find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + && dotnet test "$@"' -- "$@"
