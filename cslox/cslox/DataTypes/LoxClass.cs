using cslox.Analyzers;
using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxClass( LoxClass metaclass , string name , LoxClass superclass , Dictionary<string , LoxFunction> methods ) : LoxInstance( metaclass ), ILoxCallable {
    private readonly Dictionary<string , LoxFunction> _methods = methods;
    private readonly LoxClass _superclass = superclass;

    public readonly string Name = name;

    public int Arity( ) {
        LoxFunction initializer = FindMethod( "init" );

        return initializer?.Arity( ) ?? 0;
    }

    public object Call( Interpreter interpreter , List<object> arguments ) {
        LoxInstance instance = new( this );

        LoxFunction initializer = FindMethod( "init" );
        _ = initializer?.Bind( "init" , instance ).Call( interpreter , arguments );

        return instance;
    }

    public override string ToString( ) {
        return Name;
    }

    public LoxFunction FindMethod( string name ) {
        return _methods.TryGetValue( name , out LoxFunction method )
            ? method
            : _superclass?.FindMethod( name );
    }
}
