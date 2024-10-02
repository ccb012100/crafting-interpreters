using cslox.LoxCallables;

namespace cslox.Analyzers;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<ValueTuple> {
    private static readonly object s_uninitialized = new( );
    private readonly Dictionary<string , object> _globals = new( );
    private readonly Dictionary<Expr , int> _locals = new( );
    private readonly Dictionary<Expr , int> _slots = new( );
    private Environment _environment;

    public Interpreter( ) {
        _globals.Add( "clock" , new Clock( ) );
    }

    public void Interpret( List<Stmt> statements ) {
        try {
            foreach ( Stmt statement in statements ) {
                Execute( statement );
            }
        } catch ( RuntimeError e ) {
            Lox.RuntimeError( e );
        }
    }

    public void Eval( Expr expr ) {
        try {
            Console.WriteLine( Evaluate( expr ) );
        } catch ( RuntimeError re ) {
            Lox.RuntimeError( re );
        }
    }

    public void Resolve( Expr expr , int depth , int slot ) {
        _locals.Add( expr , depth );
        _slots.Add( expr , slot );
    }

    private object Evaluate( Expr expr ) {
        return expr.Accept( this );
    }

    private void Execute( Stmt stmt ) {
        stmt.Accept( this );
    }

    public void ExecuteBlock( List<Stmt> statements , Environment environment ) {
        Environment previous = _environment;

        try {
            _environment = environment;

            foreach ( Stmt stmt in statements ) {
                Execute( stmt );
            }
        } finally {
            _environment = previous;
        }
    }

    private class Clock : ILoxCallable {
        public int Arity( ) {
            return 0;
        }

        public object Call( Interpreter interpreter , List<object> arguments ) {
            return DateTime.UtcNow.Ticks / 1000.0;
        }

        public override string ToString( ) {
            return "<native fn>";
        }
    }

    private class BreakException : Exception;

    #region Expr.IVisitor<object>

    public object VisitAssignExpr( Expr.Assign expr ) {
        object value = Evaluate( expr.Value );

        if ( _locals.TryGetValue( expr , out int distance ) ) {
            _environment.AssignAt( distance , _slots[expr] , value );
        } else if ( _globals.ContainsKey( expr.Name.Lexeme ) ) {
            _globals[expr.Name.Lexeme] = value;
        } else {
            throw new RuntimeError( expr.Name , $"VisitAssignExpr -> Undefined variable '{expr.Name.Lexeme}.'" );
        }

        return value;
    }

    public object VisitBinaryExpr( Expr.Binary expr ) {
        object left = Evaluate( expr.Left );
        object right = Evaluate( expr.Right );

        switch ( expr.Operator.Type ) {
            case MINUS:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left - ( double ) right;
            case SLASH:
                CheckNumberOperands( expr.Operator , left , right );

                if ( ( double ) right == 0 ) {
                    throw new RuntimeError( expr.Operator , "Division by 0." );
                }

                return ( double ) left / ( double ) right;
            case STAR:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left * ( double ) right;
            case GREATER:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left > ( double ) right;
            case GREATER_EQUAL:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left >= ( double ) right;
            case LESS:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left < ( double ) right;
            case LESS_EQUAL:
                CheckNumberOperands( expr.Operator , left , right );

                return ( double ) left <= ( double ) right;
            case BANG_EQUAL:
                CheckNumberOperands( expr.Operator , left , right );

                return !IsEqual( left , right );
            case EQUAL_EQUAL:
                CheckNumberOperands( expr.Operator , left , right );

                return IsEqual( left , right );
            case PLUS:
                return (left, right) switch {
                    (double dl, double dr ) => dl + dr,
                    (string sl, double dr ) => sl + Stringify( dr ),
                    (double dl, string sr ) => Stringify( dl ) + sr,
                    (string sl, string sr ) => sl + sr,
                    _ => throw new RuntimeError( expr.Operator , "Operands must be number or strings." )
                };

            default:
                return null; // unreachable
        }
    }

    public object VisitCallExpr( Expr.Call expr ) {
        object callee = Evaluate( expr.Callee );
        List<object> arguments = expr.Arguments.Select( Evaluate ).ToList( );

        if ( callee is not ILoxCallable function ) {
            throw new RuntimeError( expr.Paren , "Can only call functions and classes." );
        }

        if ( arguments.Count != function.Arity( ) ) {
            throw new RuntimeError( expr.Paren , $"Expected {function.Arity( )} arguments but got {arguments.Count}." );
        }

        return function.Call( this , arguments );
    }

    public object VisitFunctionExpr( Expr.Function expr ) {
        return new LoxFunction( null , expr , _environment );
    }

    public object VisitGetExpr( Expr.Get expr ) {
        object obj = Evaluate( expr.Object );

        return obj switch {
            LoxInstance instance => instance.Get( expr.Name ),
            _ => throw new RuntimeError( expr.Name , "Only instances have properties." )
        };
    }

    public object VisitGroupingExpr( Expr.Grouping expr ) {
        return Evaluate( expr.Expression );
    }

    public object VisitLiteralExpr( Expr.Literal expr ) {
        return expr.Value;
    }

    public object VisitLogicalExpr( Expr.Logical expr ) {
        object left = Evaluate( expr.Left );

        return expr.Operator.Type switch {
            OR when IsTruthy( left ) => left,
            AND when !IsTruthy( left ) => left,
            _ => Evaluate( expr.Right )
        };
    }

    public object VisitConditionalExpr( Expr.Conditional expr ) {
        object condition = Evaluate( expr.Condition );

        return IsTruthy( condition ) ? Evaluate( expr.ThenBranch ) : Evaluate( expr.ElseBranch );
    }

    public object VisitSetExpr( Expr.Set expr ) {
        object @object = Evaluate( expr.Object );

        if ( @object is not LoxInstance instance ) {
            throw new RuntimeError( expr.Name , "Only instances have fields." );
        }

        object value = Evaluate( expr.Value );
        instance.Set( expr.Name , value );

        return value;
    }

    public object VisitUnaryExpr( Expr.Unary expr ) {
        object right = Evaluate( expr.Right );

        switch ( expr.Operator.Type ) {
            case BANG:
                return !IsTruthy( right );
            case MINUS:
                CheckNumberOperand( expr.Operator , right );

                return -( double ) right;
        }

        return null; // Unreachable
    }

    public object VisitVariableExpr( Expr.Variable expr ) {
        return LookUpVariable( expr.Name , expr );
    }

    private object LookUpVariable( Token name , Expr expr ) {
        if ( _locals.TryGetValue( expr , out int distance ) ) {
            return _environment.GetAt( distance , _slots[expr] );
        }

        if ( _globals.TryGetValue( name.Lexeme , out object value ) ) {
            return value;
        }

        throw new RuntimeError( name , $"Undefined variable '{name.Lexeme}'." );
    }

    #endregion

    #region Stmt.IVisitor<ValueTuple>

    public ValueTuple VisitBlockStmt( Stmt.Block stmt ) {
        ExecuteBlock( stmt.Statements , new Environment( _environment ) );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitBreakStmt( ) {
        throw new BreakException( );
    }

    public ValueTuple VisitClassStmt( Stmt.Class stmt ) {
        Define( stmt.Name , null );
        LoxClass klass = new( stmt.Name.Lexeme );

        if ( _globals.ContainsKey( stmt.Name.Lexeme ) ) {
            _globals[stmt.Name.Lexeme] = klass;
        } else {
            throw new RuntimeError( stmt.Name , $"Undefined class '{stmt.Name.Lexeme}.'" );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitExpressionStmt( Stmt.ExpressionStmt stmt ) {
        Evaluate( stmt.Expression );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitFunctionStmt( Stmt.FunctionStmt stmt ) {
        LoxFunction function = new( stmt.Name.Lexeme , stmt.Function , _environment );
        Define( stmt.Name , function );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitIfStmt( Stmt.If stmt ) {
        if ( IsTruthy( Evaluate( stmt.Condition ) ) ) {
            Execute( stmt.ThenBranch );
        } else if ( stmt.ElseBranch is not null ) {
            Execute( stmt.ElseBranch );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitPrintStmt( Stmt.Print stmt ) {
        object value = Evaluate( stmt.Expression );
        Console.WriteLine( Stringify( value ) );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitReturnStmt( Stmt.Return stmt ) {
        object value = null;

        if ( stmt.Value is not null ) {
            value = Evaluate( stmt.Value );
        }

        throw new Return( value );
    }

    public ValueTuple VisitVarStmt( Stmt.Var stmt ) {
        object value = s_uninitialized;

        if ( stmt.Initializer is not null ) {
            value = Evaluate( stmt.Initializer );
        }

        Define( stmt.Name , value );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitWhileStmt( Stmt.While stmt ) {
        try {
            while ( IsTruthy( Evaluate( stmt.Condition ) ) ) {
                Execute( stmt.Body );
            }
        } catch ( BreakException ) {
            // do nothing
        }

        return ValueTuple.Create( );
    }

    private void Define( Token name , object value ) {
        if ( _environment is not null ) {
            _environment.Define( value );
        } else {
            _globals.Add( name.Lexeme , value );
        }
    }

    #endregion

    #region private methods

    private static bool IsTruthy( object obj ) {
        return obj switch {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsEqual( object a , object b ) {
        if ( a is null ) // guard against null references
        {
            return b is null;
        }

        return a.Equals( b );
    }

    private static string Stringify( object obj ) {
        switch ( obj ) {
            case null:
                return "nil";
            case double d: {
                    string str = d.ToString( "N2" );

                    return str.EndsWith( ".00" ) ? str[..^3] : str;
                }
            case string s:
                return s;
            default:
                return obj.ToString( );
        }
    }

    private static void CheckNumberOperand( Token @operator , object operand ) {
        if ( operand is double ) {
            return;
        }

        throw new RuntimeError( @operator , "Operand must be a number" );
    }

    private static void CheckNumberOperands( Token @operator , object left , object right ) {
        switch (left, right) {
            case (double, double ):
                return;
            case (double, _ ):
                throw new RuntimeError( @operator , "Right operand must be a number." );
            case (_, double ):
                throw new RuntimeError( @operator , "Left operand must be a number." );
            default:
                throw new RuntimeError( @operator , "Operands must be numbers." );
        }
    }

    #endregion
}
