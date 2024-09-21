namespace cslox.DataTypes;

internal abstract class Stmt {
    public abstract T Accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T> {
        T VisitExpressionStmt( ExpressionStmt stmt );
        T VisitPrintStmt( Print stmt );
        T VisitVarStmt( Var stmt );
        T VisitBlockStmt( Block stmt );
        T VisitIfStmt( If stmt );
        T VisitWhileStmt( While stmt );
    }

    public class ExpressionStmt( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitExpressionStmt( this );
        }
    }

    public class Print( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitPrintStmt( this );
        }
    }

    public class Var( Token name , Expr initializer ) : Stmt {
        public readonly Token Name = name;
        public readonly Expr Initializer = initializer;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVarStmt( this );
        }
    }

    public class Block( List<Stmt> statements ) : Stmt {
        public readonly List<Stmt> Statements = statements;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBlockStmt( this );
        }
    }

    public class If( Expr condition , Stmt thenBranch , Stmt elseBranch ) : Stmt {
        public readonly Expr Condition = condition;
        public readonly Stmt ThenBranch = thenBranch;
        public readonly Stmt ElseBranch = elseBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitIfStmt( this );
        }
    }

    public class While( Expr condition , Stmt body ) : Stmt {
        public readonly Expr Condition = condition;
        public readonly Stmt Body = body;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitWhileStmt( this );
        }
    }
}
