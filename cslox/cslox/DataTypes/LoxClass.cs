using cslox.Analyzers;
using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxClass( string name ) : ILoxCallable {
    public readonly string Name = name;

    public int Arity( ) => 0;

    public object Call( Interpreter interpreter , List<object> arguments ) {
        return new LoxInstance( this );
    }

    public override string ToString( ) {
        return Name;
    }
}
