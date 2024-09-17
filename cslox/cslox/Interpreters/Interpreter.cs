using cslox.DataTypes;

namespace cslox.Interpreters;
internal class Interpreter : Expr.IVisitor<object>
{
    public void Interpret( Expr expression )
    {
        try
        {
            object value = Evaluate( expression );
            Console.WriteLine( Stringify( value ) );
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError( e );
        }
    }

    public object VisitBinaryExpr( Expr.Binary expr )
    {
        object left = Evaluate( expr.Left );
        object right = Evaluate( expr.Right );

        switch (expr.Operator.Type)
        {
            case MINUS:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left - (double)right;
            case SLASH:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left * (double)right;
            case GREATER:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands( expr.Operator, left, right );
                return (double)left <= (double)right;
            case BANG_EQUAL:
                CheckNumberOperands( expr.Operator, left, right );
                return !IsEqual( left, right );
            case EQUAL_EQUAL:
                CheckNumberOperands( expr.Operator, left, right );
                return IsEqual( left, right );
            case PLUS:
                if (left is double dl && right is double dr)
                {
                    return dl + dr;
                }
                else
                {
                    if (left is string sl && right is string sr)
                    {
                        return sl + sr;
                    }

                    throw new RuntimeError( expr.Operator, "Operands must both be number or both be strings." );
                }
            default:
                return null; // unreachable
        };
    }

    public object VisitGroupingExpr( Expr.Grouping expr )
    {
        return Evaluate( expr.Expression );
    }

    public object VisitLiteralExpr( Expr.Literal expr )
    {
        return expr.Value;
    }

    public object VisitUnaryExpr( Expr.Unary expr )
    {
        object right = Evaluate( expr.Right );

        switch (expr.Operator.Type)
        {
            case BANG:
                return !IsTruthy( right );
            case MINUS:
                CheckNumberOperand( expr.Operator, right );
                return -(double)right;
        };

        return null; // Unreachable
    }

    private object Evaluate( Expr expr )
    {
        return expr.Accept( this );
    }

    private static bool IsTruthy( object obj )
    {
        return obj switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsEqual( object a, object b )
    {
        if (a is null) // guard against null references
        {
            return b is null;
        }

        return a.Equals( b );
    }

    private static string Stringify( object obj )
    {
        switch (obj)
        {
            case null:
                return "nil";
            case double d:
                {
                    string str = d.ToString( "N2" );

                    return str.EndsWith( ".00" ) ? str[..^3] : str;
                }
            default:
                return obj.ToString();
        }
    }

    private static void CheckNumberOperand( Token @operator, object operand )
    {
        if (operand is double)
        {
            return;
        }

        throw new RuntimeError( @operator, "Operand must be a number" );
    }

    private static void CheckNumberOperands( Token @operator, object left, object right )
    {
        switch (left, right)
        {
            case (double, double ):
                return;
            case (double, _ ):
                throw new RuntimeError( @operator, "Right operand must be a number." );
            case (_, double ):
                throw new RuntimeError( @operator, "Left operand must be a number." );
            default:
                throw new RuntimeError( @operator, "Operands must be numbers." );
        }
    }
}
