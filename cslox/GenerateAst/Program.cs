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
                "Assign      : Token name, Expr value" ,
                "Binary      : Expr left, Token oper, Expr right" ,
                "Call        : Expr callee, Token paren, List<Expr> arguments" ,
                "Get         : Expr object, Token name" ,
                "Grouping    : Expr expression" ,
                "Literal     : object value" ,
                "Logical     : Expr left, Token operator, Expr right" ,
                "Conditional : Expr condition, Expr thenBranch, Expr elseBranch" ,
                "Set         : Expr object, Token name, Expr value" ,
                "Super       : Token keyword, Token method" ,
                "This        : Token keyword" ,
                "Unary       : Token oper, Expr right" ,
                "Variable    : Token name" ,
                "Function    : List<Token> parameters, List<Stmt> body"
            ]
        );

        AstGenerator.DefineAst(
            outputDir ,
            "Stmt" ,
            [
                "Block          : List<Stmt> statements" ,
                "Break          : " ,
                "Class          : Token name, Expr.Variable superclass, List<Expr> traits, List<Stmt.Function> methods, List<Stmt.Function> classMethods" ,
                "ExpressionStmt : Expr expression" ,
                "Function       : Token name, Expr.Function function" ,
                "If             : Expr condition, Stmt thenBranch, Stmt elseBranch" ,
                "Print          : Expr expression" ,
                "Return         : Token keyword, Expr value" ,
                "Trait          : Token name, List<Expr> traits, List<Stmt.Function> methods" ,
                "Var            : Token name, Expr initializer" ,
                "While          : Expr condition, Stmt body"
            ]
        );
    }
}
