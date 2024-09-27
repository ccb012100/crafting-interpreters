namespace cslox.Analyzers;

internal class Environment( Environment enclosing ) {
    private readonly Environment _enclosing = enclosing;
    private readonly Dictionary<string , object> _values = [ ];

    public Environment( ) : this( null ) { }

    public void Define( string name , object value ) {
        _values.Add( name , value );
    }

    public object Get( Token name ) {
        if ( _values.TryGetValue( name.Lexeme , out object value ) ) {
            return value;
        }

        if ( _enclosing is not null ) {
            return _enclosing.Get( name );
        }

        throw new RuntimeError( name , $"Undefined variable '{name.Lexeme}'." );
    }

    public void Assign( Token name , object value ) {
        if ( _values.ContainsKey( name.Lexeme ) ) {
            _values[name.Lexeme] = value;
        } else if ( _enclosing is not null ) {
            _enclosing.Assign( name , value );
        } else {
            throw new RuntimeError( name , $"Undefined variable '{name.Lexeme}'" );
        }
    }
}
