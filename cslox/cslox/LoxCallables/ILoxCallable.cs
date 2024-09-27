using cslox.Analyzers;

namespace cslox.LoxCallables;

internal interface ILoxCallable {
    int Arity( );
    object Call( Interpreter interpreter , List<object> arguments );
}
