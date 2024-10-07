using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxInstance( LoxClass klass ) {
    private readonly Dictionary<string , object> _fields = [ ];
    private readonly LoxClass _klass = klass;

    public LoxInstance( ) : this( null ) {
    }

    public override string ToString( ) {
        return $"{_klass.Name} instance";
    }

    public virtual object Get( Token name ) {
        if ( _fields.TryGetValue( name.Lexeme , out object f ) ) {
            return f;
        }

        // Fields shadow methods
        LoxFunction method = _klass.FindMethod( name.Lexeme );

        if ( method is not null ) {
            return method.Bind( name.Lexeme , this );
        }

        throw new RuntimeError( name , $"Undefined property '{name.Lexeme}'." );
    }

    public virtual void Set( Token name , object value ) {
        _fields[name.Lexeme] = value; // Add or Update
    }
}
