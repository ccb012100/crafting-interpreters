namespace cslox;

internal abstract class Expr
{
    internal interface Visitor<out R>
    {
        R visitBinaryExpr(Binary expr);
        R visitGroupingExpr(Grouping expr);
        R visitLiteralExpr(Literal expr);
        R visitUnaryExpr(Unary expr);
    }

    public class Binary : Expr
    {
        public Binary(Expr left, Token @operator, Expr right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }

        public override R accept<R>(Visitor<R> visitor) => visitor.visitBinaryExpr(this);

        public Expr left;
        public Token @operator;
        public Expr right;
    }

    public class Grouping : Expr
    {
        public Grouping(Expr expression)
        {
            this.expression = expression;
        }

        public override R accept<R>(Visitor<R> visitor) => visitor.visitGroupingExpr(this);

        public Expr expression;
    }

    public class Literal : Expr
    {
        public Literal(object? value)
        {
            this.value = value;
        }

        public override R accept<R>(Visitor<R> visitor) => visitor.visitLiteralExpr(this);

        public object? value; // Literal can be null
    }

    public class Unary : Expr
    {
        public Unary(Token oper, Expr right)
        {
            this.oper = oper;
            this.right = right;
        }

        public override R accept<R>(Visitor<R> visitor) => visitor.visitUnaryExpr(this);

        public Token oper;
        public Expr right;
    }

    public abstract R accept<R>(Visitor<R> visitor);
}
