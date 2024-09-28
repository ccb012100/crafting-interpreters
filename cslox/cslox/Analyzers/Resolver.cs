namespace cslox.Analyzers;

public class Resolver( Interpreter interpreter ) : Expr.IVisitor<ValueTuple>, Stmt.IVisitor<ValueTuple> {
    private readonly Stack<Dictionary<string , bool>> _scopes = new( );
    private readonly Interpreter _interpreter = interpreter;

    #region Expr.IVisitor

    public ValueTuple VisitAssignExpr( Expr.Assign expr ) {
        Resolve( expr.Value );
        ResolveLocal( expr , expr.Name );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitBinaryExpr( Expr.Binary expr ) {
        Resolve( expr.Left );
        Resolve( expr.Right );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitCallExpr( Expr.Call expr ) {
        Resolve( expr.Callee );

        foreach ( Expr arg in expr.Arguments ) {
            Resolve( arg );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitFunctionExpr( Expr.Function function ) {
        BeginScope( );

        foreach ( Token param in function.Parameters ) {
            Declare( param );
            Define( param );
        }

        Resolve( function.Body );
        EndScope( );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitGroupingExpr( Expr.Grouping expr ) {
        Resolve( expr.Expression );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitLiteralExpr( Expr.Literal expr ) {
        return ValueTuple.Create( );
    }

    public ValueTuple VisitLogicalExpr( Expr.Logical expr ) {
        Resolve( expr.Left );
        Resolve( expr.Right );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitConditionalExpr( Expr.Conditional expr ) {
        Resolve( expr.Condition );
        Resolve( expr.ThenBranch );
        Resolve( expr.ElseBranch );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitUnaryExpr( Expr.Unary expr ) {
        Resolve( expr.Right );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitVariableExpr( Expr.Variable expr ) {
        if ( _scopes.Count > 0
            && _scopes.Peek( ).TryGetValue( expr.Name.Lexeme , out bool value )
            && value == false ) {
            Lox.Error( expr.Name , "Can't read local variable in its own initializer." );
        }

        ResolveLocal( expr , expr.Name );

        return ValueTuple.Create( );
    }

    private void ResolveLocal( Expr expr , Token name ) {
        for ( int i = _scopes.Count - 1 ; i >= 0 ; i-- ) {
            if ( !_scopes.ElementAt( i ).ContainsKey( name.Lexeme ) ) {
                continue;
            }

            _interpreter.Resolve( expr , _scopes.Count - 1 - i );

            return;
        }
    }

    private void Resolve( Expr expr ) {
        expr.Accept( this );
    }

    #endregion

    #region Stmt.IVisitor

    public ValueTuple VisitBlockStmt( Stmt.Block stmt ) {
        BeginScope( );
        Resolve( stmt.Statements );
        EndScope( );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitBreakStmt( ) {
        return ValueTuple.Create( );
    }

    public ValueTuple VisitExpressionStmt( Stmt.ExpressionStmt stmt ) {
        Resolve( stmt.Expression );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitFunctionStmt( Stmt.FunctionStmt stmt ) {
        Declare( stmt.Name );
        Define( stmt.Name );

        ResolveFunction( stmt );

        return ValueTuple.Create( );
    }

    private void ResolveFunction( Stmt.FunctionStmt function ) {
        BeginScope( );

        foreach ( Token param in function.Function.Parameters ) {
            Declare( param );
            Define( param );
        }

        Resolve( function.Function.Body );
        EndScope( );
    }

    public ValueTuple VisitIfStmt( Stmt.If stmt ) {
        Resolve( stmt.Condition );
        Resolve( stmt.ThenBranch );

        if ( stmt.ElseBranch is not null ) {
            Resolve( stmt.ElseBranch );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitPrintStmt( Stmt.Print stmt ) {
        Resolve( stmt.Expression );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitReturnStmt( Stmt.Return stmt ) {
        if ( stmt.Value is not null ) {
            Resolve( stmt.Value );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitVarStmt( Stmt.Var stmt ) {
        Declare( stmt.Name );

        if ( stmt.Initializer is not null ) {
            Resolve( stmt.Initializer );
        }

        Define( stmt.Name );

        return ValueTuple.Create( );
    }

    private void Define( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        _scopes.Peek( ).Add( name.Lexeme , true );
    }

    private void Declare( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        _scopes.Peek( ).Add( name.Lexeme , false );
    }

    public ValueTuple VisitWhileStmt( Stmt.While stmt ) {
        Resolve( stmt.Condition );
        Resolve( stmt.Body );

        return ValueTuple.Create( );
    }

    private void BeginScope( ) {
        _scopes.Push( new Dictionary<string , bool>( ) );
    }

    private void EndScope( ) {
        _scopes.Pop( );
    }

    public void Resolve( List<Stmt> statements ) {
        foreach ( Stmt stmt in statements ) {
            Resolve( stmt );
        }
    }

    private void Resolve( Stmt stmt ) {
        stmt.Accept( this );
    }

    #endregion
}
