// ReSharper disable InconsistentNaming

namespace cslox;

internal abstract class Expr
{
    public abstract R accept<R>( Visitor<R> visitor );

    internal interface Visitor<out R>
    {
        R visitBinaryExpr( Binary expr );
        R visitGroupingExpr( Grouping expr );
        R visitLiteralExpr( Literal expr );
        R visitUnaryExpr( Unary expr );
    }

    public class Binary : Expr
    {
        public Expr left;
        public Token @operator;
        public Expr right;

        public Binary( Expr left, Token @operator, Expr right )
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }

        public override R accept<R>( Visitor<R> visitor )
        {
            return visitor.visitBinaryExpr( this );
        }
    }

    public class Grouping : Expr
    {
        public Expr expression;

        public Grouping( Expr expression )
        {
            this.expression = expression;
        }

        public override R accept<R>( Visitor<R> visitor )
        {
            return visitor.visitGroupingExpr( this );
        }
    }

    public class Literal : Expr
    {
        public object? value; // Literal can be null

        public Literal( object? value )
        {
            this.value = value;
        }

        public override R accept<R>( Visitor<R> visitor )
        {
            return visitor.visitLiteralExpr( this );
        }
    }

    public class Unary : Expr
    {
        public Token oper;
        public Expr right;

        public Unary( Token oper, Expr right )
        {
            this.oper = oper;
            this.right = right;
        }

        public override R accept<R>( Visitor<R> visitor )
        {
            return visitor.visitUnaryExpr( this );
        }
    }
}
