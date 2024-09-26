namespace cslox.Analyzers;

internal interface ILoxCallable {
    int Arity( );
    object Call( Interpreter interpreter , List<object> arguments );
}
