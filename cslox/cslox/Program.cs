// See https://aka.ms/new-console-template for more information

using cslox.Extensions;

namespace cslox;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program {
    private static void Main( string[ ] args ) {
        Console.WriteLine( $"*args* {args.toJson( false )}\n" );

        // using exit code convention from UNIX “sysexits.h” header https://www.freebsd.org/cgi/man.cgi?query=sysexits
        switch ( args.Length ) {
            case 0:
                Console.WriteLine( "Enter code:" );
                Lox.RunPrompt( );

                return;
            case 1:
                Lox.RunFile( args[0] );

                return;
            default:
                Console.WriteLine( "Usage: dotnet run [SCRIPT]" );
                Environment.Exit( 64 );

                break;
        }
    }
}
