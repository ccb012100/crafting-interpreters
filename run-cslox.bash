#!/usr/bin/env bash
set -Eeou pipefail

scriptdir=$(dirname -- "$(readlink -f -- "$0")")

echo '> dotnet run --project' "$scriptdir/cslox/cslox/mv cslox.csproj" "$@"
dotnet run --project "$scriptdir/cslox/cslox/cslox.csproj" "$@"
