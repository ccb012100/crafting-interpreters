using System.Diagnostics;

using cslox.Analyzers;

using Environment = cslox.Analyzers.Environment;

namespace cslox.LoxCallables;

public class LoxFunction( string name , Expr.Function declaration , Environment closure , bool isInitializer )
    : ILoxCallable {
    private readonly Environment _closure = closure;
    private readonly Expr.Function _declaration = declaration;
    private readonly string _fnNamePrintableForm = name is null ? "<fn>" : $"<fn {name}>";
    private readonly bool _isInitializer = isInitializer;

    public LoxFunction( Stmt.FunctionStmt functionStmt , Environment closure , bool isInitializer ) : this(
        functionStmt.Name.Lexeme ,
        functionStmt.Function ,
        closure ,
        isInitializer
    ) {
    }

    public int Arity( ) {
        return _declaration.Parameters.Count;
    }

    public bool IsGetter( ) {
        return _declaration.Parameters is null;
    }

    public object Call( Interpreter interpreter , List<object> arguments ) {
        Environment environment = new( _closure );

        if ( _declaration.Parameters is not null ) {
            for ( int i = 0 ; i < _declaration.Parameters.Count ; i++ ) {
                environment.Define( arguments[i] );
            }
        }

        try {
            interpreter.ExecuteBlock( _declaration.Body , environment );
        } catch ( Return r ) {
            /*
             * _closure.GetAt(0,0) returns the `this` Instance
             * (this works because the closure Environment has only a single variable: the instance that the initializer was called on).
            */
            return _isInitializer ? _closure.GetAt( 0 , 0 ) : r.Value;
        }

        return _isInitializer ? _closure.GetAt( 0 , 0 ) : null;
    }

    public override string ToString( ) {
        return _fnNamePrintableForm;
    }

    public LoxFunction Bind( string name , LoxInstance instance ) {
        Environment environment = new( _closure );
        environment.Define( instance );

        return new LoxFunction( name , _declaration , environment , _isInitializer );
    }
}
