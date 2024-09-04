using System.Text;

namespace cslox;

using static TokenType;

public static class Lox
{
    private static bool s_sHadError;

    public static void runFile( string path )
    {
        if (path is "")
        {
            Console.WriteLine( "File parameter is empty; aborting..." );
            Environment.Exit( 66 );
        }

        byte[] bytes = File.ReadAllBytes( path );
        // TODO: get encoding of path
        string source = Encoding.Default.GetString( bytes );
        run( source );

        if (s_sHadError)
        {
            Environment.Exit( 65 );
        }
    }

    private static void run( string source )
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.scanTokens();
        Parser parser = new(tokens);
        Expr expression = parser.parse();

        // Stop if there was a syntax error.
        if (s_sHadError)
        {
            return;
        }

        Console.WriteLine( new AstPrinter().print( expression ) );
    }

    public static void runPrompt()
    {
        while (Console.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty( line ))
            {
                break;
            }

            run( line );
        }
    }

    public static void error( int line, string message )
    {
        report( line, "", message );
    }

    public static void error( Token token, string message )
    {
        report(
            token.Line,
            token.Type == EOF ? " at end" : " at '" + token.Lexeme + "'",
            message
        );
    }

    private static void report( int line, string where, string message )
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine( $"[line {line}] Error{where}: {message}" );
        s_sHadError = true;
    }

    private class AstPrinter : Expr.IVisitor<string>
    {
        public string visitBinaryExpr( Expr.Binary expr )
        {
            return parenthesize(
                expr.Operator.Lexeme,
                expr.Left,
                expr.Right
            );
        }

        public string visitGroupingExpr( Expr.Grouping expr )
        {
            return parenthesize( "group", expr.Expression );
        }

        public string visitLiteralExpr( Expr.Literal expr )
        {
            return expr.Value == null ? "nil" : expr.Value.ToString();
        }

        public string visitUnaryExpr( Expr.Unary expr )
        {
            return parenthesize( expr.Oper.Lexeme, expr.Right );
        }

        public string print( Expr expr )
        {
            return expr.accept( this );
        }

        private string parenthesize( string name, params Expr[] exprs )
        {
            StringBuilder builder = new StringBuilder()
                .Append( '(' )
                .Append( name );

            foreach (Expr expr in exprs)
            {
                builder.Append( ' ' ).Append( expr.accept( this ) );
            }

            return builder.Append( ')' ).ToString();
        }
    }
}
