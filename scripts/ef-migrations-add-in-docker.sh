#!/usr/bin/env sh
# Adds an EF migration inside the .NET SDK container, for hosts that block locally built DLLs (see
# test-backend-in-docker.sh). Usage: scripts/ef-migrations-add-in-docker.sh <MigrationName>
# The generated files and the regenerated OpenAPI document are copied back into backend/.
set -eu

NAME="$1"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
BACKEND="$ROOT/backend"
OUT="$(mktemp -d)"
if command -v cygpath >/dev/null 2>&1; then
  BACKEND_MOUNT="$(cygpath -w "$BACKEND")"; OUT_MOUNT="$(cygpath -w "$OUT")"; export MSYS_NO_PATHCONV=1
else
  BACKEND_MOUNT="$BACKEND"; OUT_MOUNT="$OUT"
fi

docker run --rm \
  -v "$BACKEND_MOUNT:/src:ro" \
  -v "$OUT_MOUNT:/out" \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  sh -c 'mkdir /work && cd /src && tar cf - --exclude=bin --exclude=obj . | tar xf - -C /work && cd /work \
    && dotnet tool restore >/dev/null && dotnet restore src/Claims.Api >/dev/null \
    && dotnet ef migrations add "$1" --project src/Claims.Api --output-dir Infrastructure/Persistence/Migrations \
    && cp -r src/Claims.Api/Infrastructure/Persistence/Migrations/. /out/ && cp openapi/claims-api.json /out/claims-api.json' -- "$NAME"

cp -r "$OUT"/*.cs "$BACKEND/src/Claims.Api/Infrastructure/Persistence/Migrations/"
cp "$OUT/claims-api.json" "$BACKEND/openapi/claims-api.json"
echo "Migration $NAME added."
