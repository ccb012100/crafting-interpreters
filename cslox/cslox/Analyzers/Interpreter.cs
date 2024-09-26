using static cslox.DataTypes.Expr;
using static cslox.DataTypes.Stmt;

namespace cslox.Analyzers;

internal class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<ValueTuple> {
    private readonly Environment _globals = new( );
    private Environment _environment;

    public Interpreter( ) {
        _environment = _globals;

        _globals.Define( "clock" , new Clock( ) , true );
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

    private class BreakException : Exception;

    #region Expr.IVisitor<object>

    public object VisitAssignExpr( Assign expr ) {
        object value = Evaluate( expr.Value );
        _environment.Assign( expr.Name , value , true );

        return value;
    }

    public object VisitBinaryExpr( Binary expr ) {
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

    public object VisitCallExpr( Call expr ) {
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

    public object VisitGroupingExpr( Grouping expr ) {
        return Evaluate( expr.Expression );
    }

    public object VisitLiteralExpr( Literal expr ) {
        return expr.Value;
    }

    public object VisitUnaryExpr( Unary expr ) {
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

    public object VisitVariableExpr( Variable expr ) {
        return _environment.Get( expr.Name );
    }

    public object VisitLogicalExpr( Logical expr ) {
        object left = Evaluate( expr.Left );

        return expr.Operator.Type switch {
            OR when IsTruthy( left ) => left,
            AND when !IsTruthy( left ) => left,
            _ => Evaluate( expr.Right )
        };
    }

    #endregion

    #region Stmt.IVisitor<ValueTuple>

    public ValueTuple VisitExpressionStmt( ExpressionStmt stmt ) {
        Evaluate( stmt.Expression );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitPrintStmt( Print stmt ) {
        object value = Evaluate( stmt.Expression );
        Console.WriteLine( Stringify( value ) );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitVarStmt( Var stmt ) {
        object value = null;

        if ( stmt.Initializer is not null ) {
            value = Evaluate( stmt.Initializer );
        }

        _environment.Define( stmt.Name.Lexeme , value , stmt.Initializer != null );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitBlockStmt( Block stmt ) {
        ExecuteBlock( stmt.Statements , new Environment( _environment ) );

        return ValueTuple.Create( );
    }

    public ValueTuple VisitIfStmt( If stmt ) {
        if ( IsTruthy( Evaluate( stmt.Condition ) ) ) {
            Execute( stmt.ThenBranch );
        } else if ( stmt.ElseBranch is not null ) {
            Execute( stmt.ElseBranch );
        }

        return ValueTuple.Create( );
    }

    public ValueTuple VisitWhileStmt( While stmt ) {
        try {
            while ( IsTruthy( Evaluate( stmt.Condition ) ) ) {
                Execute( stmt.Body );
            }
        } catch ( BreakException ) {
            // do nothing
        }

        return ValueTuple.Create( );
    }

    #endregion

    #region private methods

    private object Evaluate( Expr expr ) {
        return expr.Accept( this );
    }

    private void Execute( Stmt stmt ) {
        stmt.Accept( this );
    }

    private void ExecuteBlock( List<Stmt> statements , Environment environment ) {
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

    public ValueTuple VisitBreakStmt( Break stmt ) {
        throw new BreakException( );
    }

    #endregion

    private class Clock : ILoxCallable {
        public int Arity( ) => 0;

        public object Call( Interpreter interpreter , List<object> arguments ) {
            return DateTime.UtcNow.Ticks / 1000.0;
        }

        public override string ToString( ) {
            return "<native fn>";
        }
    }
}
