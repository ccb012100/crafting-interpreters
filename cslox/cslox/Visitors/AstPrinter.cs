using System.Text;

using static cslox.DataTypes.Expr;

namespace cslox.Visitors;

internal class AstPrinter : IVisitor<string> {
    public string VisitAssignExpr( Assign expr ) {
        throw new NotImplementedException( );
    }

    public string VisitBinaryExpr( Binary expr ) {
        return Parenthesize(
            expr.Operator.Lexeme ,
            expr.Left ,
            expr.Right
        );
    }

    public string VisitGroupingExpr( Grouping expr ) {
        return Parenthesize( "group" , expr.Expression );
    }

    public string VisitLiteralExpr( Literal expr ) {
        return expr.Value == null ? "nil" : expr.Value.ToString( );
    }

    public string VisitUnaryExpr( Unary expr ) {
        return Parenthesize( expr.Operator.Lexeme , expr.Right );
    }

    public string VisitVariableExpr( Variable expr ) {
        throw new NotImplementedException( );
    }

    public string VisitLogicalExpr( Logical expr ) {
        throw new NotImplementedException( );
    }

    public string Print( Expr expr ) {
        return expr.Accept( this );
    }

    private string Parenthesize( string name , params Expr[ ] exprs ) {
        StringBuilder builder = new StringBuilder( )
            .Append( '(' )
            .Append( name );

        foreach ( Expr expr in exprs ) {
            builder.Append( ' ' ).Append( expr.Accept( this ) );
        }

        return builder.Append( ')' ).ToString( );
    }
}
