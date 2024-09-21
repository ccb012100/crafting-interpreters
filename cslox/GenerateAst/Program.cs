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
                "Assign   : Token name, Expr value" ,
                "Binary   : Expr left, Token oper, Expr right" ,
                "Grouping : Expr expression" ,
                "Literal  : object value" ,
                "Logical  : Expr left, Token operator, Expr right",
                "Unary    : Token oper, Expr right" ,
                "Variable : Token name"
            ]
        );

        AstGenerator.DefineAst(
            outputDir ,
            "Stmt" ,
            [
                "BlockStatement      : List<Stmt> statements" ,
                "ExpressionStatement : Expr expression" ,
                "IfStatement         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "PrintStatement      : Expr expression" ,
                "Var                 : Token name, Expr initializer"
            ]
        );
    }
}
