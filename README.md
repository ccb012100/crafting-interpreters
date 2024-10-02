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

This includes the "extra credit" grammar such as `ternary` and `comma` that I've implemented on top of the "core" **Lox** implementation from the
book.

```console
╭────────────────────────────────────────────────────────────────────────────╮
│                        EXPRESSION GRAMMAR                                  │░
├────────────────────────────────────────────────────────────────────────────┤░
│    program        →   declaration* EOF ;                                   │░
│                                                                            │░
│    declaration    →   classDecl                                            │░
│                   |   funDecl ;                                            │░
│                   |   varDecl ;                                            │░
│                   |   statement ;                                          │░
│                                                                            │░
│    classDecl      →   "class" IDENTIFIER ( "<" IDENTIFIER)?                │░
│                       "{" function "}" ;                                   │░
│    funDecl        →   "fun" function ;                                     │░
│    varDecl        →   "var" IDENTIFIER ( "=" expression )? ";" ;           │░
│    varDecl        →   "var" IDENTIFIER ( "=" expression )? ";" ;           │░
│                                                                            │░
│    statement      →   exprStmt                                             │░
│                   |   forStmt                                              │░
│                   |   ifStmt                                               │░
│                   |   printStmt                                            │░
│                   |   returnStmt                                           │░
│                   |   whileStmt                                            │░
│                   |   block ;                                              │░
│                                                                            │░
│    breakStmt      →   "break" ";" ;                                        │░
│                   |   statement ;                                          │░
│    exprStmt       →   expression ";" ;                                     │░
│    forStmt        →   "for" "(" ( varDecl | exprStmt | ";" )               │░
│                       expression? ";"                                      │░
│                       expression? ")" statement ;                          │░
│    ifStmt         →   "if" "(" expression ")" breakStmt                    │░
│                       ( "else" breakStmt )? ;                              │░
│    printStmt      →   "print" expression ";" ;                             │░
│    returnStmt     →   "return" expression? ";" ;                           │░
│    whileStmt      →   "while" "(" expression ")" breakStmt ;               │░
│    block          →   "{" declaration* "}" ;                               │░
│                                                                            │░
│    expression     →   lambda ;                                             │░
│    comma          →   "(" assignment ( "," assignment )* ;                 │░
│                                                                            │░
│    assignment     →   ( call "." )? IDENTIFIER "=" assignment              │░
│                   |   conditional ;                                        │░
│                                                                            │░
│    conditional    →   logic_or ( "?" expression ":" conditional )*;        │░
│    logic_or       →   logic_and ( "or" logic_and )* ;                      │░
│    logic_and      →   equality ( "and" equality )* ;                       │░
│    equality       →   comparison ( ( "!=" | "==" ) comparison )* ;         │░
│    comparison     →   term ( ( ">" | ">=" | "<" | "<=" ) term )* ;         │░
│    term           →   factor ( ( "-" | "+" ) factor )* ;                   │░
│    factor         →   unary ( ( "/" | "*" ) unary )* ;                     │░
│                                                                            │░
│    unary          →   ( "!" | "-" ) unary | lambda ;                       │░
│                   |   lambda ;                                             │░
│    call           →   primary ( "(" arguments? ")" | "." IDENTIFIER )* ;   │░
│    primary        →   NUMBER | STRING | "true" | "false" | "nil"           │░
│                   |   "(" expression ")" ;                                 │░
│                   |   lambda ;                                             │░
│                   |   IDENTIFIER ;                                         │░
│                                                                            │░
│    lambda         →   "fun" "(" parameters? ")" block ;                    │░
│    function       →   IDENTIFIER "(" parameters? ")" block ;               │░
│    parameters     →   IDENTIFIER ( "," IDENTIFIER )* ;                     │░
│    arguments      →   expression ( "," expression )* ;                     │░
╰────────────────────────────────────────────────────────────────────────────╯░
 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░

╭────────────────────────────────────────────────────────────────────────────╮
│                          LEXICAL GRAMMAR                                   │░
├────────────────────────────────────────────────────────────────────────────┤░
│    NUMBER        →   DIGIT+ ("." DIGIT+ )? ;                               │░
│    STRING        →   "\"" <any char except "\"">* "\"" ;                   │░
│    IDENTIFIER    →   ALPHA ( ALPHA | DIGIT )* ;                            │░
│    ALPHA         →   "a" ... "z" | "A" ... "Z" | "_" ;                     │░
│    DIGIT         →   "0" ... "9" ;                                         │░
╰────────────────────────────────────────────────────────────────────────────╯░
 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
 
╔════════════════════════╦═══════════════════════════════════════════════════╗
║    Grammar notation    ║   Code representation                             ║░
╠━━━━━━━━━━━━━━━━━━━━━━━━╬━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╣░
║    Terminal            ║   Code to match and consume a token               ║░
║    Non-terminal        ║   Call to that rule’s function                    ║░
║    |                   ║   if or switch statement                          ║░
║    * or +              ║   while or for loop                               ║░
║    ?                   ║   if statement                                    ║░
╚════════════════════════╩═══════════════════════════════════════════════════╝░
 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░

╔════════════════════════╦═══════════════════════════════════════════════════╗
║    Lox Type            ║   C# representation                               ║░
╠━━━━━━━━━━━━━━━━━━━━━━━━╬━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╣░
║    Any Lox value       ║   object                                          ║░
║    nil                 ║   null                                            ║░
║    Boolean             ║   boolean                                         ║░
║    number              ║   Double                                          ║░
║    string              ║   string                                          ║░
╚════════════════════════╩═══════════════════════════════════════════════════╝░
 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
```
