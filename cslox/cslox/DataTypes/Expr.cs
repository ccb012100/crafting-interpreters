namespace cslox.DataTypes;

public abstract class Expr {
    public abstract T Accept<T>( IVisitor<T> visitor );

    public interface IVisitor<out T> {
        T VisitAssignExpr( Assign expr );
        T VisitBinaryExpr( Binary expr );
        T VisitCallExpr( Call expr );
        T VisitFunctionExpr( Function expr );
        T VisitGroupingExpr( Grouping expr );
        T VisitLiteralExpr( Literal expr );
        T VisitLogicalExpr( Logical expr );
        T VisitConditionalExpr( Conditional expr );
        T VisitUnaryExpr( Unary expr );
        T VisitVariableExpr( Variable expr );
    }

    public class Assign( Token name , Expr value ) : Expr {
        public readonly Token Name = name;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitAssignExpr( this );
        }
    }

    public class Binary( Expr left , Token @operator , Expr right ) : Expr {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBinaryExpr( this );
        }
    }

    public class Call( Expr callee , Token paren , List<Expr> arguments ) : Expr {
        public readonly List<Expr> Arguments = arguments;
        public readonly Expr Callee = callee;
        public readonly Token Paren = paren;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitCallExpr( this );
        }
    }

    public class Function( List<Token> parameters , List<Stmt> body ) : Expr {
        public readonly List<Token> Parameters = parameters;
        public readonly List<Stmt> Body = body;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitFunctionExpr( this );
        }
    }

    public class Grouping( Expr expression ) : Expr {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitGroupingExpr( this );
        }
    }

    public class Literal( object value ) : Expr {
        public readonly object Value = value; // Literal can be null

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitLiteralExpr( this );
        }
    }

    public class Logical( Expr left , Token @operator , Expr right ) : Expr {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitLogicalExpr( this );
        }
    }

    public class Conditional( Expr condition , Expr thenBranch , Expr elseBranch ) : Expr {
        public readonly Expr Condition = condition;
        public readonly Expr ThenBranch = thenBranch;
        public readonly Expr ElseBranch = elseBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitConditionalExpr( this );
        }
    }

    public class Unary( Token @operator , Expr right ) : Expr {
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitUnaryExpr( this );
        }
    }

    public class Variable( Token name ) : Expr {
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVariableExpr( this );
        }
    }
}
