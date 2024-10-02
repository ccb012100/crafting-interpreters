using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxInstance( LoxClass klass ) {
    private readonly LoxClass _klass = klass;
    private readonly Dictionary<string , object> _fields = [ ];

    public override string ToString( ) {
        return $"{_klass.Name} instance";
    }

    public object Get( Token name ) {
        if ( _fields.TryGetValue( name.Lexeme , out object f ) ) {
            return f;
        }

        // Fields shadow methods
        LoxFunction method = _klass.FindMethod( name.Lexeme );

        if ( method is not null ) {
            return method;
        }

        throw new RuntimeError( name , $"Undefined property '{name.Lexeme}'." );
    }

    public void Set( Token name , object value ) {
        _fields[name.Lexeme] = value; // Add or Update
    }
}
