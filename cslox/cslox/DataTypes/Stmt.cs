namespace cslox.DataTypes;

internal abstract class Stmt {
    public abstract T Accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T> {
        T VisitExpressionStatement( ExpressionStatement stmt );
        T VisitPrintStatement( PrintStatement stmt );
        T VisitReturnStatement( ReturnStatement stmt );
        T VisitVarStatement( VarStatement stmt );
        T VisitBlockStatement( BlockStatement stmt );
        T VisitIfStatement( IfStatement stmt );
    }

    public class ExpressionStatement( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitExpressionStatement( this );
        }
    }

    public class PrintStatement( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitPrintStatement( this );
        }
    }

    public class ReturnStatement( Token keyword , Expr value ) : Stmt {
        public readonly Token Keyword = keyword;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitReturnStatement( this );
        }
    }

    public class VarStatement( Token name , Expr initializer ) : Stmt {
        public readonly Token Name = name;
        public readonly Expr Initializer = initializer;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVarStatement( this );
        }
    }

    public class BlockStatement( List<Stmt> statements ) : Stmt {
        public readonly List<Stmt> Statements = statements;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBlockStatement( this );
        }
    }

    public class IfStatement( Expr condition , Stmt thenBranch , Stmt elseBranch ) : Stmt {
        public readonly Expr Condition = condition;
        public readonly Stmt ThenBranch = thenBranch;
        public readonly Stmt ElseBranch = elseBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitIfStatement( this );
        }
    }
}
