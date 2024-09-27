namespace cslox.DataTypes;

internal abstract class Stmt {
    public abstract T Accept<T>( IVisitor<T> visitor );

    internal interface IVisitor<out T> {
        T VisitBlockStmt( Block stmt );
        T VisitBreakStmt( );
        T VisitExpressionStmt( ExpressionStmt stmt );
        T VisitFunctionStmt( Function stmt );
        T VisitIfStmt( If stmt );
        T VisitPrintStmt( Print stmt );
        T VisitReturnStmt( Return stmt );
        T VisitVarStmt( Var stmt );
        T VisitWhileStmt( While stmt );
    }

    public class Block( List<Stmt> statements ) : Stmt {
        public readonly List<Stmt> Statements = statements;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBlockStmt( this );
        }
    }

    public class Break : Stmt {
        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBreakStmt( );
        }
    }

    public class ExpressionStmt( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitExpressionStmt( this );
        }
    }

    public class Function( Token name , List<Token> parameters , List<Stmt> body ) : Stmt {
        public readonly List<Stmt> Body = body;
        public readonly Token Name = name;
        public readonly List<Token> Parameters = parameters;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitFunctionStmt( this );
        }
    }

    public class If( Expr condition , Stmt thenBranch , Stmt elseBranch ) : Stmt {
        public readonly Expr Condition = condition;
        public readonly Stmt ElseBranch = elseBranch;
        public readonly Stmt ThenBranch = thenBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitIfStmt( this );
        }
    }

    public class Print( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitPrintStmt( this );
        }
    }

    public class Return( Token name , Expr value ) : Stmt {
        public readonly Token Name = name;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitReturnStmt( this );
        }
    }

    public class Var( Token name , Expr initializer ) : Stmt {
        public readonly Expr Initializer = initializer;
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVarStmt( this );
        }
    }

    public class While( Expr condition , Stmt body ) : Stmt {
        public readonly Stmt Body = body;
        public readonly Expr Condition = condition;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitWhileStmt( this );
        }
    }
}
