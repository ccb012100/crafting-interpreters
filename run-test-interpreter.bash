#!/usr/bin/env bash
set -Eeou pipefail

scriptdir=$(dirname -- "$(readlink -f -- "$0")")

"$scriptdir/run-cslox.bash" "$scriptdir/cslox/cslox/test-code/interpreter.lox"
