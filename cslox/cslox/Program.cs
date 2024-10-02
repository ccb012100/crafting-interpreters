// See https://aka.ms/new-console-template for more information

namespace cslox;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program {
    private static void Main( string[ ] args ) {
        // using exit code convention from UNIX “sysexits.h” header https://www.freebsd.org/cgi/man.cgi?query=sysexits

        switch ( args.Length ) {
            case 0: {
                    Console.WriteLine( "Enter code:" );
                    Lox.RunPrompt( );
                    // Lox.RunFile( "test-code/test.lox" );

                    return;
                }
            case 1: {
                    Lox.RunFile( args[0] , Lox.Visitor.Interpreter );

                    return;
                }
            case 2 when args[1] == "--ast": {
                    Lox.RunFile( args[0] , Lox.Visitor.Ast );

                    return;
                }
            case 2 when args[1] == "--rpn": {
                    Lox.RunFile( args[0] , Lox.Visitor.Rpn );

                    return;
                }
            default: {
                    Console.WriteLine( "Usage: dotnet run [SCRIPT [--ast|--rpn]]" );
                    Environment.Exit( 64 );

                    break;
                }
        }
    }
}
