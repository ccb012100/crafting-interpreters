# crafting-interpreters

[![.NET](https://github.com/ccb012100/crafting-interpreters/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ccb012100/crafting-interpreters/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/ccb012100/crafting-interpreters/actions/workflows/codeql.yml/badge.svg)](https://github.com/ccb012100/crafting-interpreters/actions/workflows/codeql.yml)

Working through [Crafting Interpreters](https://craftinginterpreters.com/) by Robert Nystrom.

Instead of `jlox` (`lox` in Java), I'm translating the code to [`cslox`](/cslox/) (`lox` in **C#**).

## Commit message format

For feature commits, the subject line follows the format:

> `<Chapter/Section number> <Chapter/Section name> [EC] (description)`

`EC` (Extra Credit) denotes extra features that aren't part of the core `Lox` feature set.
