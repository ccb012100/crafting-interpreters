#!/usr/bin/env bash
scriptdir=$(dirname -- "$(readlink -f -- "$0")")

"$scriptdir/run-cslox.bash" "$scriptdir/cslox/cslox/test-code/test.lox"
