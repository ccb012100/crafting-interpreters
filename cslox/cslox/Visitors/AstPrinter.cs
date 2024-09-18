using System.Text;

namespace cslox.Visitors;

internal class AstPrinter : Expr.IVisitor<string>
{
    public string VisitBinaryExpr( Expr.Binary expr )
    {
        return Parenthesize(
            expr.Operator.Lexeme,
            expr.Left,
            expr.Right
        );
    }

    public string VisitGroupingExpr( Expr.Grouping expr )
    {
        return Parenthesize( "group", expr.Expression );
    }

    public string VisitLiteralExpr( Expr.Literal expr )
    {
        return expr.Value == null ? "nil" : expr.Value.ToString();
    }

    public string VisitUnaryExpr( Expr.Unary expr )
    {
        return Parenthesize( expr.Operator.Lexeme, expr.Right );
    }

    public string Print( Expr expr )
    {
        return expr.Accept( this );
    }

    private string Parenthesize( string name, params Expr[] exprs )
    {
        StringBuilder builder = new StringBuilder()
            .Append( '(' )
            .Append( name );

        foreach (Expr expr in exprs)
        {
            builder.Append( ' ' ).Append( expr.Accept( this ) );
        }

        return builder.Append( ')' ).ToString();
    }
}
