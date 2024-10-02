using cslox.Extensions;

namespace cslox.DataTypes;

public abstract class Stmt {
    public abstract T Accept<T>( IVisitor<T> visitor );

    public interface IVisitor<out T> {
        T VisitBlockStmt( Block stmt );
        T VisitBreakStmt( );
        T VisitClassStmt( Class stmt );
        T VisitExpressionStmt( ExpressionStmt stmt );
        T VisitFunctionStmt( FunctionStmt stmt );
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

        public override string ToString( ) {
            return $"Block Statements=[ {Statements.ToPrintString( )} ]";
        }
    }

    public class Break : Stmt {
        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBreakStmt( );
        }

        public override string ToString( ) {
            return "Break";
        }
    }

    public class Class( Token name , List<FunctionStmt> methods ) : Stmt {
        public readonly List<FunctionStmt> Methods = methods;
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitClassStmt( this );
        }

        public override string ToString( ) {
            return $"Class Name=<{Name}> Methods=<{Methods.ToPrintString( )}>";
        }
    }

    public class ExpressionStmt( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitExpressionStmt( this );
        }

        public override string ToString( ) {
            return $"ExpressionStmt Expression=<{Expression}>";
        }
    }

    public class FunctionStmt( Token name , Expr.Function function ) : Stmt {
        public readonly Expr.Function Function = function;
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitFunctionStmt( this );
        }

        public override string ToString( ) {
            return $"Function Name=<{Name}> Function=<{Function}>";
        }
    }

    public class If( Expr condition , Stmt thenBranch , Stmt elseBranch ) : Stmt {
        public readonly Expr Condition = condition;
        public readonly Stmt ElseBranch = elseBranch;
        public readonly Stmt ThenBranch = thenBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitIfStmt( this );
        }

        public override string ToString( ) {
            return $"Condition=<{Condition}> ElseBranch=<{ElseBranch}> ThenBranch=<{ThenBranch}>";
        }
    }

    public class Print( Expr expression ) : Stmt {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitPrintStmt( this );
        }

        public override string ToString( ) {
            return $"Print Expression=<{Expression}>";
        }
    }

    public class Return( Token keyword , Expr value ) : Stmt {
        public readonly Token Keyword = keyword;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitReturnStmt( this );
        }

        public override string ToString( ) {
            return $"Return keyword=<{Keyword}> Value=<{Value}>";
        }
    }

    public class Var( Token name , Expr initializer ) : Stmt {
        public readonly Expr Initializer = initializer;
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVarStmt( this );
        }

        public override string ToString( ) {
            return $"Var Name=<{Name}> Initializer=<{Initializer}>";
        }
    }

    public class While( Expr condition , Stmt body ) : Stmt {
        public readonly Stmt Body = body;
        public readonly Expr Condition = condition;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitWhileStmt( this );
        }

        public override string ToString( ) {
            return $"While Body={{{Body}}} Condition=<{Condition}>";
        }
    }
}
