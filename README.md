# crafting-interpreters

[![.NET](https://github.com/ccb012100/crafting-interpreters/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ccb012100/crafting-interpreters/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/ccb012100/crafting-interpreters/actions/workflows/codeql.yml/badge.svg)](https://github.com/ccb012100/crafting-interpreters/actions/workflows/codeql.yml)

Working through [Crafting Interpreters](https://craftinginterpreters.com/) by Robert Nystrom.

Instead of `jlox` (`lox` in Java), I'm translating the code to [`cslox`](/cslox/) (`lox` in **C#**).

## Commit message format

For feature commits, the subject line follows the format:

> `<Chapter/Section number> <Chapter/Section name> [EC] (description)`

`EC` (Extra Credit) denotes extra features that aren't part of the core `Lox` feature set.

### `cslox`

This includes the "extra credit" grammar such as `ternary` and `comma` that I've implemented on top of the "core" **Lox** implementation from the book.

```console
┌──────────────────────────────────────────────────────────────────────────┐
│                       Expression Grammar                                 │
├──────────────────────────────────────────────────────────────────────────┤
│    expression   →   ternary ;                                            │
│    ternary      →   comma ( "?" comma ":" comma )*;                      │
│    comma        →   "(" equality ( "," equality )* ;                     │
│    equality     →   comparison ( ( "!=" | "==" ) comparison )*;          │
│    comparison   →   term ( ( ">" | ">=" | "<" | "<=" ) term )* ;         │
│    term         →   factor ( ( "-" | "+" ) factor )*;                    │
│    factor       →   unary ( ( "/" | "*" ) unary )* ;                     │
│    unary        →   ( "!" | "-" ) unary                                  │
│                 |   primary ;                                            │
│    primary      →   NUMBER | STRING | "true" | "false" | "nil"           │
│                 |   "(" expression ")" ;                                 │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│    Grammar notation    |   Code representation                           │
├──────────────────────────────────────────────────────────────────────────┤
│    Terminal            |   Code to match and consume a token             │
│    Non-terminal        |   Call to that rule’s function                  │
│    |                   |   if or switch statement                        │
│    * or +              |   while or for loop                             │
│    ?                   |   if statement                                  │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│    Lox Type            |   C# representation                             │
├──────────────────────────────────────────────────────────────────────────┤
│    Any Lox value       |   object                                        │
│    nil                 |   null                                          │
│    Boolean             |   boolean                                       │
│    number              |   Double                                        │
│    string              |   string                                        │
└──────────────────────────────────────────────────────────────────────────┘
```
