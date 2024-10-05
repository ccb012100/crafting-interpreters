namespace cslox.Analyzers;

public class Resolver( Interpreter interpreter ) : Expr.IVisitor<ValueTuple>, Stmt.IVisitor<ValueTuple> {
    private readonly Interpreter _interpreter = interpreter;

    /*
     * IMPORTANT:   C#'s Stack indexing does not work like Java:
     *              stack[0] will always return the TOP of the stack (not the bottom)
     */
    private readonly Stack<Dictionary<string , Variable>> _scopes = new( );
    private ClassType _currentClass = ClassType.None;
    private FunctionType _currentFunction = FunctionType.None;

    public void Resolve( List<Stmt> statements ) {
        foreach ( Stmt stmt in statements ) {
            Resolve( stmt );
        }
    }

    private enum ClassType {
        None,
        Class,
        Subclass,
        Trait
    }

    private enum FunctionType {
        None,
        Function,
        Initializer,
        Method
    }

    private enum VariableState {
        Declared,
        Defined,
        Read
    }

    private class Variable( Token name , int slot , VariableState state ) {
        public readonly Token Name = name;
        public readonly int Slot = slot;
        public VariableState State = state;

        public override string ToString( ) {
            return $"Variable Name={Name}, Slot={Slot}, State={State}";
        }
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

    public ValueTuple VisitGetExpr( Expr.Get expr ) {
        Resolve( expr.Object );

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

    public ValueTuple VisitSetExpr( Expr.Set expr ) {
        Resolve( expr.Value );
        Resolve( expr.Object );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitSuperExpr( Expr.Super expr ) {
        if ( _currentClass == ClassType.None ) {
            Lox.Error( expr.Keyword , "Can't use 'super' keyword outside of a class." );
        } else if ( _currentClass == ClassType.Trait ) {
            Lox.Error( expr.Keyword , "Can't use 'super' keyword in a trait." );
        } else if ( _currentClass != ClassType.Subclass ) {
            Lox.Error( expr.Keyword , "Can't use 'super' keyword in a class with no superclass." );
        }

        Token superToken = expr.Keyword with { Lexeme = "super" }; // we want ResolveLocal to look for "super"

        ResolveLocal( expr , superToken , true );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitThisExpr( Expr.This expr ) {
        if ( _currentClass == ClassType.None ) {
            Lox.Error( expr.Keyword , "Can't use 'this' keyword outside of a class." );

            return ValueTuple.Create( );
        }

        Token thisToken = expr.Keyword with { Lexeme = "this" }; // we want ResolveLocal to look for "this"

        ResolveLocal( expr , thisToken , true );

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
        // This differs from the Java implementation because indexing in Stack type in C# is the opposite (0 = Top).
        for ( int i = 0 ; i < _scopes.Count ; i++ ) {
            Dictionary<string , Variable> scope = _scopes.ElementAt( i );

            if ( !scope.TryGetValue( name.Lexeme , out Variable variable ) ) {
                continue;
            }

            _interpreter.Resolve( expr , i , variable.Slot );

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

    public ValueTuple VisitClassStmt( Stmt.Class stmt ) {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.Class;

        Declare( stmt.Name );
        Define( stmt.Name );

        if ( stmt.Superclass is not null ) {
            if ( stmt.Name.Lexeme.Equals( stmt.Superclass.Name.Lexeme ) ) {
                Lox.Error( stmt.Superclass.Name , "A class can't inherit from itself." );
            }

            _currentClass = ClassType.Subclass;
            Resolve( stmt.Superclass );

            BeginScope( );

            Dictionary<string , Variable> superScope = _scopes.Peek( );
            superScope.Add( "super" , new Variable( stmt.Name , superScope.Count , VariableState.Read ) );
        }

        foreach ( Expr trait in stmt.Traits ) {
            Resolve( trait );
        }

        BeginScope( );

        Dictionary<string , Variable> scope = _scopes.Peek( );
        scope.Add( "this" , new Variable( stmt.Name , scope.Count , VariableState.Read ) );

        foreach ( Stmt.FunctionStmt method in stmt.Methods ) {
            FunctionType declaration = method.Name.Lexeme.Equals( "init" )
                ? FunctionType.Initializer
                : FunctionType.Method;

            ResolveFunction( method , declaration );
        };

        EndScope( );

        foreach ( Stmt.FunctionStmt method in stmt.ClassMethods ) {
            BeginScope( );
            Dictionary<string , Variable> classMethodScope = _scopes.Peek( );
            classMethodScope.Add( "this" , new Variable( stmt.Name , scope.Count , VariableState.Read ) );
            ResolveFunction( method , FunctionType.Method );
            EndScope( );
        }

        if ( stmt.Superclass is not null ) {
            EndScope( );
        }

        _currentClass = enclosingClass;

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
            if ( _currentFunction == FunctionType.Initializer ) {
                Lox.Error( stmt.Keyword , "Can't return a value from an initializer." );
            }

            Resolve( stmt.Value );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitTraitStmt( Stmt.Trait stmt ) {
        Declare( stmt.Name );
        Define( stmt.Name );

        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.Trait;

        foreach ( Expr trait in stmt.Traits ) {
            Resolve( trait );
        }

        BeginScope( );

        Dictionary<string , Variable> scope = _scopes.Peek( );
        scope.Add( "this" , new Variable( stmt.Name , scope.Count , VariableState.Read ) );

        foreach ( Stmt.FunctionStmt method in stmt.Methods ) {
            ResolveFunction( method , FunctionType.Method );
        }

        EndScope( );

        _currentClass = enclosingClass;

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

    public ValueTuple VisitWhileStmt( Stmt.While stmt ) {
        Resolve( stmt.Condition );
        Resolve( stmt.Body );

        return ValueTuple.Create( );
    }

    private void BeginScope( ) {
        _scopes.Push( [ ] );
    }

    private void EndScope( ) {
        Dictionary<string , Variable> scope = _scopes.Pop( );

        foreach ( (string _, Variable v) in scope.Where( kv => kv.Value.State == VariableState.Defined ) ) {
            Lox.Error( v.Name , "Local variable is not used in scope." );
        }
    }

    private void Resolve( Stmt stmt ) {
        stmt.Accept( this );
    }

    private void ResolveFunction( Stmt.FunctionStmt function , FunctionType type ) {
        FunctionType enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope( );

        if ( function.Function.Parameters is not null ) {
            foreach ( Token param in function.Function.Parameters ) {
                Declare( param );
                Define( param );
            }
        }

        Resolve( function.Function.Body );
        EndScope( );
        _currentFunction = enclosingFunction;
    }

    private void Declare( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        Dictionary<string , Variable> scope = _scopes.Peek( );

        // instead of if `(!scope.ContainsKey( name.Lexeme )) { scope.Add() }, we use TryAdd
        if ( !scope.TryAdd( name.Lexeme , new Variable( name , scope.Count , VariableState.Declared ) ) ) {
            Lox.Error( name , "Already a variable with this name in the scope." );
        }
    }

    private void Define( Token name ) {
        if ( _scopes.Count == 0 ) {
            return;
        }

        _scopes.Peek( )[name.Lexeme].State = VariableState.Defined;
    }

    #endregion
}
