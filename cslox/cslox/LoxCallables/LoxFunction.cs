
using cslox.Analyzers;

using Environment = cslox.Analyzers.Environment;

namespace cslox.LoxCallables;

internal class LoxFunction( Stmt.Function declaration ) : ILoxCallable {
    private readonly Stmt.Function _declaration = declaration;
    private readonly string _printable = $"<fn {declaration.Name.Lexeme}>";

    public int Arity( ) => _declaration.Parameters.Count;

    public object Call( Interpreter interpreter , List<object> arguments ) {
        Environment environment = new( );

        for ( int i = 0 ; i < _declaration.Parameters.Count ; i++ ) {
            environment.Define( _declaration.Parameters[i].Lexeme , arguments[i] , true );
        }

        interpreter.ExecuteBlock( _declaration.Body , environment );

        return null;
    }

    public override string ToString( ) => _printable;
}