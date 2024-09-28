namespace cslox.Analyzers;

public class Resolver( Interpreter interpreter ) : Expr.IVisitor<ValueTuple> , Stmt.IVisitor<ValueTuple> {
    private readonly Interpreter _interpreter = interpreter;
    private readonly Stack<Dictionary<string , Variable>> _scopes = new( );
    private FunctionType _currentFunction = FunctionType.None;

    public void Resolve( List<Stmt> statements ) {
        foreach ( Stmt stmt in statements ) {
            Resolve( stmt );
        }
    }

    private enum FunctionType {
        None ,
        Function
    }

    private enum VariableState {
        Declared ,
        Defined ,
        Read
    }

    private class Variable( Token name , VariableState state ) {
        public readonly Token Name = name;
        public VariableState State = state;
    }

    #region Expr.IVisitor

    public ValueTuple VisitAssignExpr( Expr.Assign expr ) {
        Resolve( expr.Value );
        ResolveLocal( expr , expr.Name , false );

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
            && _scopes.Peek( ).TryGetValue( expr.Name.Lexeme , out Variable variable )
            && variable.State == VariableState.Declared ) {
            Lox.Error( expr.Name , "Can't read local variable in its own initializer." );
        }

        ResolveLocal( expr , expr.Name , true );

        return ValueTuple.Create( );
    }

    private void ResolveLocal( Expr expr , Token name , bool isRead ) {
        for ( int i = _scopes.Count - 1 ; i >= 0 ; i-- ) {
            if ( !_scopes.ElementAt( i ).TryGetValue( name.Lexeme , out Variable variable ) ) {
                continue;
            }

            _interpreter.Resolve( expr , _scopes.Count - 1 - i );

            if ( isRead ) {
                variable.State = VariableState.Read;
            }

            return;
        }
        // Not found; assume it's global.
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

        ResolveFunction( stmt , FunctionType.Function );

        return ValueTuple.Create( );
    }

    private void ResolveFunction( Stmt.FunctionStmt function , FunctionType type ) {
        FunctionType enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope( );

        foreach ( Token param in function.Function.Parameters ) {
            Declare( param );
            Define( param );
        }

        Resolve( function.Function.Body );
        EndScope( );
        _currentFunction = enclosingFunction;
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
        if ( _currentFunction == FunctionType.None ) {
            Lox.Error( stmt.Keyword , "Can't return from top-level-code." );
        }

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

    private void Declare( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        Dictionary<string , Variable> scope = _scopes.Peek( );

        if ( scope.ContainsKey( name.Lexeme ) ) {
            Lox.Error( name , "Already a variable with this name in the scope." );
        }

        scope.Add( name.Lexeme , new Variable( name , VariableState.Declared ) );
    }

    private void Define( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        _scopes.Peek( )[name.Lexeme].State = VariableState.Defined;
    }

    public ValueTuple VisitWhileStmt( Stmt.While stmt ) {
        Resolve( stmt.Condition );
        Resolve( stmt.Body );

        return ValueTuple.Create( );
    }

    private void BeginScope( ) {
        _scopes.Push( new Dictionary<string , Variable>( ) );
    }

    private void EndScope( ) {
        Dictionary<string , Variable> scope = _scopes.Pop( );

        foreach ( Variable variable in scope.Values.Where( variable => variable.State == VariableState.Defined ) ) {
            Lox.Error( variable.Name , "Local variable is not used." );
        }
    }

    private void Resolve( Stmt stmt ) {
        stmt.Accept( this );
    }

    #endregion
}