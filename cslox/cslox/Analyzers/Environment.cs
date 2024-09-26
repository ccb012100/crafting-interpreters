namespace cslox.Analyzers;

internal class Environment( Environment enclosing ) {
    private readonly Environment _enclosing = enclosing;
    private readonly Dictionary<string , (object value, bool initialized)> _values = [ ];

    public Environment( ) : this( null ) { }

    public void Define( string name , object value , bool initialized ) {
        _values.Add( name , (value, initialized) );
    }

    public object Get( Token name ) {
        if ( _values.TryGetValue( name.Lexeme , out (object value, bool initialized) variable ) ) {
            if ( !variable.initialized ) {
                throw new RuntimeError( name , $"Uninitialized variable '{name.Lexeme}'." );
            }

            return variable.value;
        }

        if ( _enclosing is not null ) {
            return _enclosing.Get( name );
        }

        throw new RuntimeError( name , $"Undefined variable '{name.Lexeme}'." );
    }

    public void Assign( Token name , object value , bool initialized ) {
        if ( _values.ContainsKey( name.Lexeme ) ) {
            _values[name.Lexeme] = (value, initialized);
        } else if ( _enclosing is not null ) {
            _enclosing.Assign( name , value , initialized );
        } else {
            throw new RuntimeError( name , $"Undefined variable '{name.Lexeme}'" );
        }
    }
}
