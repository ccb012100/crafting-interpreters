using System.Text;

namespace cslox;

using static TokenType;

public static class Lox
{
    private static bool s_sHadError;

    public static void RunFile( string path )
    {
        if (path is "")
        {
            Console.WriteLine( "File parameter is empty; aborting..." );
            Environment.Exit( 66 );
        }

        byte[] bytes = File.ReadAllBytes( path );
        // TODO: get encoding of path
        string source = Encoding.Default.GetString( bytes );
        Console.WriteLine( $"source = {source}" );
        Run( source );

        if (s_sHadError)
        {
            Environment.Exit( 65 );
        }
    }

    private static void Run( string source )
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        Expr expression = parser.Parse();

        // Stop if there was a syntax error.
        if (s_sHadError)
        {
            return;
        }

        Console.WriteLine( new AstPrinter().Print( expression ) );
    }

    public static void RunPrompt()
    {
        while (Console.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty( line ))
            {
                break;
            }

            Run( line );
        }
    }

    public static void Error( int line, string message )
    {
        Report( line, "", message );
    }

    public static void Error( Token token, string message )
    {
        Report(
            token.Line,
            token.Type == EOF ? " at end" : " at '" + token.Lexeme + "'",
            message
        );
    }

    private static void Report( int line, string where, string message )
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine( $"[line {line}] Error{where}: {message}" );
        s_sHadError = true;
    }

    private class AstPrinter : Expr.IVisitor<string>
    {
        public string VisitBinaryExpr( Expr.Binary expr )
        {
            return Parenthesize(
                expr.Operator.Lexeme,
                expr.Left,
                expr.Right
            );
        }

        public string VisitGroupingExpr( Expr.Grouping expr )
        {
            return Parenthesize( "group", expr.Expression );
        }

        public string VisitLiteralExpr( Expr.Literal expr )
        {
            return expr.Value == null ? "nil" : expr.Value.ToString();
        }

        public string VisitUnaryExpr( Expr.Unary expr )
        {
            return Parenthesize( expr.Oper.Lexeme, expr.Right );
        }

        public string Print( Expr expr )
        {
            return expr.Accept( this );
        }

        private string Parenthesize( string name, params Expr[] exprs )
        {
            StringBuilder builder = new StringBuilder()
                .Append( '(' )
                .Append( name );

            foreach (Expr expr in exprs)
            {
                builder.Append( ' ' ).Append( expr.Accept( this ) );
            }

            return builder.Append( ')' ).ToString();
        }
    }
}
