using cslox.Analyzers;
using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxClass( string name , Dictionary<string , LoxFunction> methods ) : ILoxCallable {
    private readonly Dictionary<string , LoxFunction> _methods = methods;
    public readonly string Name = name;

    public int Arity( ) {
        return 0;
    }

    public object Call( Interpreter interpreter , List<object> arguments ) {
        return new LoxInstance( this );
    }

    public override string ToString( ) {
        return Name;
    }

    public LoxFunction FindMethod( string name ) {
        if ( _methods.TryGetValue( name , out LoxFunction method ) ) {
            return method;
        }

        return null;
    }
}
