namespace cslox.DataTypes;

public abstract class Expr {
    public abstract T Accept<T>( IVisitor<T> visitor );

    public interface IVisitor<out T> {
        T VisitAssignExpr( Assign expr );
        T VisitBinaryExpr( Binary expr );
        T VisitCallExpr( Call expr );
        T VisitFunctionExpr( Function expr );
        T VisitGetExpr( Get expr );
        T VisitGroupingExpr( Grouping expr );
        T VisitLiteralExpr( Literal expr );
        T VisitLogicalExpr( Logical expr );
        T VisitConditionalExpr( Conditional expr );
        T VisitSetExpr( Set expr );
        T VisitThisExpr( This expr );
        T VisitUnaryExpr( Unary expr );
        T VisitVariableExpr( Variable expr );
    }

    public class Assign( Token name , Expr value ) : Expr {
        public readonly Token Name = name;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitAssignExpr( this );
        }

        public override string ToString( ) {
            return $"AssignExpr => Name=<{Name}>, Value=<{Value}>";
        }
    }

    public class Binary( Expr left , Token @operator , Expr right ) : Expr {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitBinaryExpr( this );
        }

        public override string ToString( ) {
            return $"Binary => Left=<{Left}>, Operator=<{Operator}>, Right=<{Right}>";
        }
    }

    public class Call( Expr callee , Token paren , List<Expr> arguments ) : Expr {
        public readonly List<Expr> Arguments = arguments;
        public readonly Expr Callee = callee;
        public readonly Token Paren = paren;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitCallExpr( this );
        }

        public override string ToString( ) {
            return $"Call => Arguments=[{string.Join( "\n" , Arguments )}] Callee=<{Callee}>, Paren=<{Paren}>";
        }
    }

    public class Function( List<Token> parameters , List<Stmt> body ) : Expr {
        public readonly List<Stmt> Body = body;
        public readonly List<Token> Parameters = parameters;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitFunctionExpr( this );
        }

        public override string ToString( ) {
            return $"Function => Parameters=[{string.Join( "\n" , Parameters )}], Body={{{string.Join( "\n" , Body )}}} ";
        }
    }

    public class Get( Expr @object , Token name ) : Expr {
        public readonly Token Name = name;
        public readonly Expr Object = @object;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitGetExpr( this );
        }
    }

    public class Grouping( Expr expression ) : Expr {
        public readonly Expr Expression = expression;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitGroupingExpr( this );
        }

        public override string ToString( ) {
            return $"Grouping=({Expression})";
        }
    }

    public class Literal( object value ) : Expr {
        public readonly object Value = value; // Literal can be null

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitLiteralExpr( this );
        }

        public override string ToString( ) {
            return $"Literal=<{Value}>";
        }
    }

    public class Logical( Expr left , Token @operator , Expr right ) : Expr {
        public readonly Expr Left = left;
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitLogicalExpr( this );
        }

        public override string ToString( ) {
            return $"Logical Left=<{Left}>, Operator=<{Operator}>, Right=<{Right}>";
        }
    }

    public class Conditional( Expr condition , Expr thenBranch , Expr elseBranch ) : Expr {
        public readonly Expr Condition = condition;
        public readonly Expr ElseBranch = elseBranch;
        public readonly Expr ThenBranch = thenBranch;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitConditionalExpr( this );
        }

        public override string ToString( ) {
            return $"Conditional: Condition={Condition}, ThenBranch={ThenBranch}, ElseBranch=<{ElseBranch}>";
        }
    }

    public class Set( Expr @object , Token name , Expr value ) : Expr {
        public readonly Token Name = name;
        public readonly Expr Object = @object;
        public readonly Expr Value = value;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitSetExpr( this );
        }
    }

    public class This( Token keyword ) : Expr {
        public readonly Token Keyword = keyword;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitThisExpr( this );
        }
    }

    public class Unary( Token @operator , Expr right ) : Expr {
        public readonly Token Operator = @operator;
        public readonly Expr Right = right;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitUnaryExpr( this );
        }

        public override string ToString( ) {
            return $"Unary Operator=<{Operator}>, Right=<{Right}>";
        }
    }

    public class Variable( Token name ) : Expr {
        public readonly Token Name = name;

        public override T Accept<T>( IVisitor<T> visitor ) {
            return visitor.VisitVariableExpr( this );
        }

        public override string ToString( ) {
            return $"Variable Name=<{Name}>";
        }
    }
}
