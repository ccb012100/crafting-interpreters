namespace cslox;

internal abstract class Expr
{
    public abstract T accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T>
    {
        T visitBinaryExpr( Binary expr );
        T visitGroupingExpr( Grouping expr );
        T visitLiteralExpr( Literal expr );
        T visitUnaryExpr( Unary expr );
    }

    public class Binary : Expr
    {
        public readonly Expr Left;
        public readonly Token Operator;
        public readonly Expr Right;

        public Binary( Expr left, Token @operator, Expr right )
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public override T accept<T>( IVisitor<T> visitor )
        {
            return visitor.visitBinaryExpr( this );
        }
    }

    public class Grouping : Expr
    {
        public readonly Expr Expression;

        public Grouping( Expr expression )
        {
            Expression = expression;
        }

        public override TR accept<TR>( IVisitor<TR> visitor )
        {
            return visitor.visitGroupingExpr( this );
        }
    }

    public class Literal : Expr
    {
        public readonly object Value; // Literal can be null

        public Literal( object value )
        {
            Value = value;
        }

        public override T accept<T>( IVisitor<T> visitor )
        {
            return visitor.visitLiteralExpr( this );
        }
    }

    public class Unary : Expr
    {
        public readonly Token Oper;
        public readonly Expr Right;

        public Unary( Token oper, Expr right )
        {
            Oper = oper;
            Right = right;
        }

        public override T accept<T>( IVisitor<T> visitor )
        {
            return visitor.visitUnaryExpr( this );
        }
    }
}
