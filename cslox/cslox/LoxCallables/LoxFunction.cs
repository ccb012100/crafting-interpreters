using cslox.Analyzers;

using Environment = cslox.Analyzers.Environment;

namespace cslox.LoxCallables;

internal class LoxFunction( string name , Expr.Function declaration , Environment closure ) : ILoxCallable {
    private readonly Expr.Function _declaration = declaration;
    private readonly string _fnNamePrintableForm = name is null ? "<fn>" : $"<fn {name}>";
    private readonly Environment _closure = closure;

    public int Arity( ) => _declaration.Parameters.Count;

    public object Call( Interpreter interpreter , List<object> arguments ) {
        Environment environment = new( _closure );

        for ( int i = 0 ; i < _declaration.Parameters.Count ; i++ ) {
            environment.Define( arguments[i] );
        }

        try {
            interpreter.ExecuteBlock( _declaration.Body , environment );
        } catch ( Return r ) {
            return r.Value;
        }

        return null;
    }

    public override string ToString( ) => _fnNamePrintableForm;
}
