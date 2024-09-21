namespace tool;

internal static class Program {
    private static void Main( string[ ] args ) {
        if ( args.Length is not 1 ) {
            Console.Error.WriteLine( "Usage: generate_ast <output directory>" );
            Environment.Exit( 64 );
        }

        string outputDir = args[0];

        AstGenerator.DefineAst(
            outputDir ,
            "Expr" ,
            [
                "Assign     : Token name, Expr value" ,
                "Binary     : Expr left, Token oper, Expr right" ,
                "Grouping   : Expr expression" ,
                "Literal    : object value" ,
                "Logical    : Expr left, Token operator, Expr right" ,
                "Unary      : Token oper, Expr right" ,
                "Variable   : Token name"
            ]
        );

        AstGenerator.DefineAst(
            outputDir ,
            "Stmt" ,
            [
                "Block          : List<Stmt> statements" ,
                "ExpressionStmt : Expr expression" ,
                "If             : Expr condition, Stmt thenBranch, Stmt elseBranch" ,
                "Print          : Expr expression" ,
                "Var            : Token name, Expr initializer" ,
                "While          : Expr condition, Stmt body"
            ]
        );
    }
}
