using System.Text;

using static cslox.DataTypes.Expr;

namespace cslox.Visitors;

internal class AstPrinter : IVisitor<string>, Stmt.IVisitor<string> {
    public string Print( Expr expr ) {
        return expr.Accept( this );
    }

    public string Print( Stmt stmt ) {
        return stmt.Accept( this );
    }

    private string Function( Function function , Token name = null ) {
        StringBuilder sb = new( "(fun " );

        if ( name is not null ) {
            sb.Append( name.Lexeme );
        }

        sb.Append( '(' );

        foreach ( Token parameter in function?.Parameters ?? [ ] ) {
            if ( parameter != function.Parameters[0] ) {
                sb.Append( ' ' );
            }

            sb.Append( parameter.Lexeme );
        }

        sb.Append( ") " );

        foreach ( Stmt bodyStmt in function.Body ) {
            sb.Append( bodyStmt.Accept( this ) );
        }

        return sb.Append( ')' ).ToString( );
    }

    private string Parenthesize( string name , params Expr[ ] exprs ) {
        StringBuilder sb = new StringBuilder( )
            .Append( '(' )
            .Append( name );

        foreach ( Expr expr in exprs ) {
            sb.Append( ' ' ).Append( expr.Accept( this ) );
        }

        return sb.Append( ')' ).ToString( );
    }

    private string ParenthesizeObjects( string name , params object[ ] parts ) {
        StringBuilder sb = new StringBuilder( "(" ).Append( name );
        transform( sb , parts );
        return sb.Append( ')' ).ToString( );

        void transform( StringBuilder sb , params object[ ] parts ) {
            foreach ( object part in parts ) {
                sb.Append( ' ' );

                switch ( part ) {
                    case Expr expression: {
                            sb.Append( expression.Accept( this ) );

                            break;
                        }
                    case Stmt stmt: {
                            sb.Append( stmt.Accept( this ) );

                            break;
                        }
                    case Token token: {
                            sb.Append( token.Lexeme );

                            break;
                        }
                    case List<object> list:
                        transform( sb , list.ToArray( ) );
                        break;
                    default: {
                            sb.Append( part );

                            break;
                        }
                }
            }
        }
    }

    #region Expr.IVisitor<string>

    public string VisitAssignExpr( Assign expr ) {
        return ParenthesizeObjects( "=" , expr.Name.Lexeme , expr.Value );
    }

    public string VisitBinaryExpr( Binary expr ) {
        return Parenthesize( expr.Operator.Lexeme , expr.Left , expr.Right );
    }

    public string VisitCallExpr( Call expr ) {
        return ParenthesizeObjects( "call" , [expr.Callee , .. expr.Arguments] );
    }

    public string VisitFunctionExpr( Function expr ) {
        return Function( expr );
    }

    public string VisitGetExpr( Get expr ) {
        return $"{expr.Name.Lexeme} {expr.Object.Accept( this )}";
    }

    public string VisitGroupingExpr( Grouping expr ) {
        return Parenthesize( "group" , expr.Expression );
    }

    public string VisitLiteralExpr( Literal expr ) {
        if ( expr is null ) {
            return "nil";
        }

        return expr.Value is string s ? $"\"{s}\"" : expr.Value.ToString( );
    }

    public string VisitSetExpr( Set expr ) {
        return ParenthesizeObjects( "=" , expr.Object , expr.Name.Lexeme , expr.Value );
    }

    public string VisitSuperExpr( Super expr ) {
        return ParenthesizeObjects( "super" , expr.Method );
    }

    public string VisitThisExpr( This expr ) {
        return "this";
    }

    public string VisitUnaryExpr( Unary expr ) {
        return Parenthesize( expr.Operator.Lexeme , expr.Right );
    }

    public string VisitVariableExpr( Variable expr ) {
        return expr.Name.Lexeme;
    }

    public string VisitLogicalExpr( Logical expr ) {
        return Parenthesize( expr.Operator.Lexeme , expr.Left , expr.Right );
    }

    public string VisitConditionalExpr( Conditional expr ) {
        return ParenthesizeObjects( "if-else" , expr.Condition , expr.ThenBranch , expr.ElseBranch );
    }

    #endregion

    #region Stmt.IVisitor<string>

    public string VisitBlockStmt( Stmt.Block stmt ) {
        StringBuilder sb = new( "(block" );

        foreach ( Stmt s in stmt.Statements ) {
            sb.Append( s.Accept( this ) );
        }

        return sb.Append( ')' ).ToString( );
    }

    public string VisitBreakStmt( ) {
        return "(break)";
    }

    public string VisitClassStmt( Stmt.Class stmt ) {
        List<object> parms = [stmt.Name];

        if ( stmt.Superclass is not null ) {
            parms.Add( Parenthesize( "extends" , stmt.Superclass ) );
        }

        if ( stmt.Traits.Count > 0 ) {
            parms.Add( Parenthesize( "with" , [.. stmt.Traits] ) );
        }

        foreach ( Stmt.FunctionStmt function in stmt.ClassMethods ) {
            parms.Add( function.Accept( this ) );
        }

        foreach ( Stmt.FunctionStmt function in stmt.Methods ) {
            parms.Add( function.Accept( this ) );
        }

        return ParenthesizeObjects( $"class" , [.. parms] );
    }

    public string VisitExpressionStmt( Stmt.ExpressionStmt stmt ) {
        return Parenthesize( ";" , stmt.Expression );
    }

    public string VisitFunctionStmt( Stmt.FunctionStmt stmt ) {
        return Function( stmt.Function , stmt.Name );
    }

    public string VisitIfStmt( Stmt.If stmt ) {
        return stmt.ElseBranch is null
            ? ParenthesizeObjects( "if" , stmt.Condition , stmt.ThenBranch )
            : ParenthesizeObjects( "if-else" , stmt.Condition , stmt.ThenBranch , stmt.ElseBranch );
    }

    public string VisitPrintStmt( Stmt.Print stmt ) {
        return Parenthesize( "print" , stmt.Expression );
    }

    public string VisitReturnStmt( Stmt.Return stmt ) {
        return stmt.Value is null ? "(return)" : Parenthesize( "return" , stmt.Value );
    }

    public string VisitVarStmt( Stmt.Var stmt ) {
        return stmt.Initializer is null
            ? ParenthesizeObjects( "var" , stmt.Name )
            : ParenthesizeObjects( "var" , stmt.Name , "=" , stmt.Initializer );
    }

    public string VisitWhileStmt( Stmt.While stmt ) {
        return ParenthesizeObjects( "while" , stmt.Condition , stmt.Body );
    }

    public string VisitTraitStmt( Stmt.Trait stmt ) {
        if ( stmt.Traits.Count == 0 ) {
            return ParenthesizeObjects( "trait" , [stmt.Name , .. stmt.Methods] );
        }

        return ParenthesizeObjects( "trait" , stmt.Name , Parenthesize( "with" , [.. stmt.Traits] ) );
    }

    #endregion
}
