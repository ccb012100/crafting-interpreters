using System.Text;

using cslox.Analyzers;
using cslox.Visitors;

using Environment = System.Environment;

namespace cslox;

internal static class Lox {
    private static readonly Interpreter s_interpreter = new( );
    private static bool s_hadError;
    private static bool s_hadRuntimeError;

    #region Run

    public enum Visitor {
        Interpreter,
        Ast,
        Rpn
    }

    public static void RunFile( string path , Visitor visitor ) {
        if ( path is "" ) {
            Console.WriteLine( "File parameter is empty; aborting..." );
            Environment.Exit( 66 );
        }

        byte[ ] bytes = File.ReadAllBytes( path );
        string source = Encoding.Default.GetString( bytes ); // TODO: get encoding of path

        List<Stmt> statements = source.ToParser( ).Parse( );

        // Stop if there was a syntax error.
        if ( s_hadError ) {
            return;
        }

        new Resolver( s_interpreter ).Resolve( statements );

        if ( s_hadError ) {
            return;
        }

        switch ( visitor ) {
            case Visitor.Interpreter:
                s_interpreter.Interpret( statements );

                break;
            case Visitor.Ast:
                var ast = new AstPrinter( );

                foreach ( Stmt s in statements ) {
                    Console.WriteLine( ast.Print( s ) );
                }

                break;
            case Visitor.Rpn:
                throw new NotImplementedException( "Need to update RpnPrinter" );
            default:
                throw new ArgumentOutOfRangeException( nameof( visitor ) , visitor , null );
        }

        if ( s_hadError ) {
            Environment.Exit( 65 );
        }

        if ( s_hadRuntimeError ) {
            Environment.Exit( 70 );
        }
    }

    public static void RunPrompt( ) {
        while ( Console.ReadLine( ) is { } line ) {
            s_hadError = false; // reset the state

            if ( string.IsNullOrEmpty( line ) ) {
                break;
            }

            if ( line.EndsWith( ';' ) ) {
                RunStatementsInPrompt( line.ToParser( ) );
            } else {
                RunExpressionInPrompt( line.ToParser( ) );
            }
        }
    }

    private static void RunStatementsInPrompt( Parser parser ) {
        List<Stmt> statements = parser.Parse( );

        // Stop if there was a syntax error.
        if ( s_hadError ) {
            return;
        }

        s_interpreter.Interpret( statements );
    }

    private static void RunExpressionInPrompt( Parser parser ) {
        Expr expression = null;

        try {
            expression = parser.ParseExpression( );
        } catch ( Parser.ParseError pe ) {
            s_hadError = true;
            Console.WriteLine( $"{pe.Message}" );
        }

        // Stop if there was a syntax or parse error.
        if ( s_hadError ) {
            return;
        }

        s_interpreter.Eval( expression );
    }

    private static Parser ToParser( this string source ) {
        return new Parser( new Scanner( source ).ScanTokens( ) );
    }

    #endregion

    #region Error

    public static void RuntimeError( RuntimeError error ) {
        Console.WriteLine( $"{error.Message}\n[line {error.Token.Line}]" );

        s_hadRuntimeError = true;
    }

    public static void Error( int line , string message ) {
        Report( line , "" , message );
    }

    public static void Error( Token token , string message ) {
        Report(
            token.Line ,
            token.Type == EOF ? " at end" : " at '" + token.Lexeme + "'" ,
            message
        );
    }

    private static void Report( int line , string where , string message ) {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine( $"[line {line}] Error{where}: {message}" );
        s_hadError = true;
    }

    #endregion
}
