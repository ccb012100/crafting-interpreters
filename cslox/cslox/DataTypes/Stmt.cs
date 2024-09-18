namespace cslox.DataTypes;

internal abstract class Stmt
{
    internal interface IVisitor<out T>
    {
        T VisitExpressionStatementStmt( ExpressionStatement stmt );
        T VisitPrintStatementStmt( PrintStatement stmt );
    }

    public class ExpressionStatement( Expr expression ) : Stmt
    {
        public override T Accept<T>( IVisitor<T> visitor ) => visitor.VisitExpressionStatementStmt( this );

        public Expr Expression = expression;
    }

    public class PrintStatement( Expr expression ) : Stmt
    {
        public override T Accept<T>( IVisitor<T> visitor ) => visitor.VisitPrintStatementStmt( this );

        public Expr Expression = expression;
    }

    public abstract T Accept<T>( IVisitor<T> visitor );
}
