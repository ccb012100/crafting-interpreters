namespace cslox.DataTypes;

internal abstract class Expr {
    public abstract T Accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T> {
        T VisitAssignExpression( AssignExpression expr );
        T VisitBinaryExpression( BinaryExpression expr );
        T VisitGroupingExpression( GroupingExpression expr );
        T VisitLiteralExpression( LiteralExpression expr );
        T VisitUnaryExpression( UnaryExpression expr );
        T VisitVariableExpression( VariableExpression expr );
    }

    public class AssignExpression( Token name , Expr value ) : Expr {
        public readonly Token Name = name;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitAssignExpression( this );
        }
    }

    public class BinaryExpression( Expr left , Token @operator , Expr right ) : Expr {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBinaryExpression( this );
        }
    }

    public class GroupingExpression( Expr expression ) : Expr {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitGroupingExpression( this );
        }
    }

    public class LiteralExpression( object value ) : Expr {
        public readonly object Value = value; // Literal can be null

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitLiteralExpression( this );
        }
    }

    public class UnaryExpression( Token @operator , Expr right ) : Expr {
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitUnaryExpression( this );
        }
    }

    public class VariableExpression( Token name ) : Expr {
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVariableExpression( this );
        }
    }
}
