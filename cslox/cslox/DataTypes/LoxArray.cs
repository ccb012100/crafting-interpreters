using System.Text;

using cslox.Analyzers;
using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxArray( int size ) : LoxInstance( null ) {
    private readonly object[ ] _elements = new object[size];

    public override object Get( Token name ) => name.Lexeme switch {
        "get" => new LoxArrayGetInstance( _elements , name ),
        "set" => new LoxArraySetInstance( _elements , name ),
        "length" => ( double ) _elements.Length,
        _ => throw new RuntimeError( name , $"Undefined property '{name.Lexeme}'." ),
    };

    public override void Set( Token name , object value ) {
        throw new RuntimeError( name , "Can't add properties to arrays." );
    }

    public override string ToString( ) {
        StringBuilder sb = new( "[ " );

        for ( int i = 0 ; i < _elements.Length ; i++ ) {
            if ( i is not 0 ) {
                sb.Append( " , " );
            }

            sb.Append( _elements[i] switch {
                string s => $"'{s}'",
                double d => d.ToString( "N2" ),
                null => "nil",
                _ => _elements[i].ToString( ),
            } );
        }

        return sb.Append( " ]" ).ToString( );
    }

    public class LoxArrayGetInstance( object[ ] elements , Token name ) : ILoxCallable {
        private readonly Token _name = name;
        private readonly object[ ] _elements = elements;

        public int Arity( ) => 1;

        public object Call( Interpreter interpreter , List<object> arguments ) {
            int index = ( int ) ( double ) arguments[0];

            if ( index < 0 ) {
                throw new RuntimeError( _name , "Array index can't be negative." );
            }

            if ( index > _elements.Length - 1 ) {
                throw new RuntimeError( _name , "Invalid array index" );
            }

            return _elements[index];
        }
    }
    public class LoxArraySetInstance( object[ ] elements , Token name ) : ILoxCallable {
        private readonly Token _name = name;

        public int Arity( ) => 2;

        public object Call( Interpreter interpreter , List<object> arguments ) {
            int index = ( int ) ( double ) arguments[0];

            if ( index < 0 ) {
                throw new RuntimeError( _name , "Array index can't be negative." );
            }

            if ( index > elements.Length - 1 ) {
                throw new RuntimeError( _name , "Invalid array index" );
            }

            object value = arguments[1];

            return elements[index] = value;
        }
    }
}
