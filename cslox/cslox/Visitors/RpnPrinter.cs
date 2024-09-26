namespace cslox.Visitors;

/// <summary>
///     Print in Reverse Polish Notation
/// </summary>
internal class RpnPrinter : Expr.IVisitor<string> {
    public string VisitAssignExpr( Expr.Assign expr ) {
        throw new NotImplementedException( );
    }

    public string VisitBinaryExpr( Expr.Binary expr ) {
        return $"{expr.Left.Accept( this )} {expr.Right.Accept( this )} {expr.Operator.Lexeme}";
    }

    public string VisitCallExpr( Expr.Call expr ) {
        throw new NotImplementedException( );
    }

    public string VisitGroupingExpr( Expr.Grouping expr ) {
        return expr.Expression.Accept( this );
    }

    public string VisitLiteralExpr( Expr.Literal expr ) {
        return expr.Value == null ? "nil" : expr.Value.ToString( );
    }

    public string VisitUnaryExpr( Expr.Unary expr ) {
        return $"{expr.Right.Accept( this )} {expr.Operator.Lexeme}";
    }

    public string VisitVariableExpr( Expr.Variable expr ) {
        throw new NotImplementedException( );
    }

    public string VisitLogicalExpr( Expr.Logical expr ) {
        throw new NotImplementedException( );
    }

    public string Print( Expr expr ) {
        return expr.Accept( this );
    }

    public string VisitConditionalExpr( Expr.Conditional expr ) {
        throw new NotImplementedException( );
    }
}
