namespace cslox.DataTypes;

internal abstract class Expr
{
    public abstract T Accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T>
    {
        T VisitBinaryExpr( Binary expr );
        T VisitGroupingExpr( Grouping expr );
        T VisitLiteralExpr( Literal expr );
        T VisitUnaryExpr( Unary expr );
    }

    public class Binary( Expr left, Token @operator, Expr right ) : Expr
    {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor )
        {
            return visitor.VisitBinaryExpr( this );
        }
    }

    public class Grouping( Expr expression ) : Expr
    {
        public readonly Expr Expression = expression;

        public override TR Accept<TR>( IVisitor<TR> visitor )
        {
            return visitor.VisitGroupingExpr( this );
        }
    }

    public class Literal( object value ) : Expr
    {
        public readonly object Value = value; // Literal can be null

        public override T Accept<T>( IVisitor<T> visitor )
        {
            return visitor.VisitLiteralExpr( this );
        }
    }

    public class Unary( Token @operator, Expr right ) : Expr
    {
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor )
        {
            return visitor.VisitUnaryExpr( this );
        }
    }
}
