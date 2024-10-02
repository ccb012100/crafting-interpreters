using System.Diagnostics;

namespace cslox.Analyzers;

public class Environment( Environment enclosing ) {
    private readonly Environment _enclosing = enclosing;
    private readonly List<object> _values = [ ];

    public void Define( object value ) {
        _values.Add( value );
    }

    public object GetThis( int distance ) {
        Environment environment = this;

        for ( int i = 0 ; i < distance ; i++ ) {
            environment = environment._enclosing;
        }

        object instance = environment._values.SingleOrDefault( v => v.GetType( ) == typeof( LoxInstance ) );

        Debug.Assert(
            instance is not null ,
            $"Expected to find a LoxInstance in environment=<{environment}>. The Resolver should have caught this"
        );

        return instance;
    }

    public object GetAt( int distance , int slot ) {
        Environment environment = this;

        for ( int i = 0 ; i < distance ; i++ ) {
            environment = environment._enclosing;
        }

        Debug.Assert(
            slot < environment._values.Count ,
            $"Invalid slot value {slot} in environment=<{environment}>. This is probably because of a discrepancy between the Parser and Resolver"
        );

        return environment._values[slot];
    }

    public void AssignAt( int distance , int slot , object value ) {
        Environment environment = this;

        for ( int i = 0 ; i < distance ; i++ ) {
            environment = environment._enclosing;
        }

        environment._values[slot] = value;
    }

    public override string ToString( ) {
        string values = _values.Count > 0
            ? string.Join( " , " , _values )
            : "[ ]";

        return _enclosing is not null
            ? $"{values} -> {_enclosing}"
            : values;
    }
}
