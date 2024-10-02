namespace cslox.DataTypes;

public class LoxInstance( LoxClass klass ) {
    private readonly LoxClass _klass = klass;
    private readonly Dictionary<string , object> fields = [ ];

    public override string ToString( ) {
        return $"{_klass.Name} instance";
    }

    public object Get( Token name ) {
        if ( fields.TryGetValue( name.Lexeme , out object f ) ) {
            return f;
        }

        throw new RuntimeError( name , $"Undefined property '{name.Lexeme}'." );
    }

    public void Set( Token name , object value ) {
        fields[name.Lexeme] = value; // Add or Update
    }
}
