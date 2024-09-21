using System.Text;

using static cslox.DataTypes.Expr;

namespace cslox.Visitors;

internal class AstPrinter : IVisitor<string> {
    public string VisitAssignExpression( AssignExpression expr ) {
        throw new NotImplementedException( );
    }

    public string VisitBinaryExpression( BinaryExpression expr ) {
        return Parenthesize(
            expr.Operator.Lexeme ,
            expr.Left ,
            expr.Right
        );
    }

    public string VisitGroupingExpression( GroupingExpression expr ) {
        return Parenthesize( "group" , expr.Expression );
    }

    public string VisitLiteralExpression( LiteralExpression expr ) {
        return expr.Value == null ? "nil" : expr.Value.ToString( );
    }

    public string VisitUnaryExpression( UnaryExpression expr ) {
        return Parenthesize( expr.Operator.Lexeme , expr.Right );
    }

    public string VisitVariableExpression( VariableExpression expr ) {
        throw new NotImplementedException( );
    }

    public string VisitLogicalExpression( LogicalExpression expr ) {
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
