﻿namespace tool;

internal static class Program
{
    private static void Main( string[] args )
    {
        if (args.Length is not 1)
        {
            Console.Error.WriteLine( "Usage: generate_ast <output directory>" );
            Environment.Exit( 64 );
        }

        string outputDir = args[0];

        AstGenerator.defineAst(
            outputDir,
            "Expr",
            [
                "Binary   : Expr left, Token oper, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token oper, Expr right"
            ]
        );
    }
}
