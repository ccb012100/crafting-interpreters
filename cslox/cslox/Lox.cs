using System.Text;
using cslox.Analyzers;

namespace cslox;

internal static class Lox
{
    private static readonly Interpreter s_interpreter = new();
    private static bool s_hadError;
    private static bool s_hadRuntimeError;

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
        Console.WriteLine( $"*source* {source}" );
        Run( source );

        if (s_hadError)
        {
            Environment.Exit( 65 );
        }

        if (s_hadRuntimeError)
        {
            Environment.Exit( 70 );
        }
    }

    private static void Run( string source )
    {
        Scanner scanner = new( source );
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new( tokens );
        Expr expression = parser.Parse();

        // Stop if there was a syntax error.
        if (s_hadError)
        {
            return;
        }

        s_interpreter.Interpret( expression );
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

    internal static void RuntimeError( RuntimeError error )
    {
        Console.WriteLine( $"{error.Message}\n[line {error.Token.Line}]" );

        s_hadRuntimeError = true;
    }

    private static void Report( int line, string where, string message )
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine( $"[line {line}] Error{where}: {message}" );
        s_hadError = true;
    }
}
