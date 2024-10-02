using cslox.Analyzers;

using Environment = cslox.Analyzers.Environment;

namespace cslox.LoxCallables;

public class LoxFunction( string name , Expr.Function declaration , Environment closure ) : ILoxCallable {
    private readonly Environment _closure = closure;
    private readonly Expr.Function _declaration = declaration;
    private readonly string _fnNamePrintableForm = name is null ? "<fn>" : $"<fn {name}>";

    public LoxFunction( Stmt.FunctionStmt functionStmt , Environment closure ) : this(
        functionStmt.Name.Lexeme ,
        functionStmt.Function ,
        closure
    ) {
    }

    public int Arity( ) {
        return _declaration.Parameters.Count;
    }

    public object Call( Interpreter interpreter , List<object> arguments ) {
        Environment environment = new( _closure );

        for ( int i = 0 ; i < _declaration.Parameters.Count ; i++ ) {
            environment.Define( arguments[i] );
        }

        try {
            interpreter.ExecuteBlock( _declaration.Body , environment );
        } catch ( Return r ) {
            return r.Value;
        }

        return null;
    }

    public override string ToString( ) {
        return _fnNamePrintableForm;
    }

    public object Bind( string name , LoxInstance instance ) {
        Environment environment = new( _closure );
        environment.Define( instance );

        return new LoxFunction( name , _declaration , environment );
    }
}
