namespace cslox.Visitors;

/// <summary>
///     Print in Reverse Polish Notation
/// </summary>
internal class RpnPrinter : Expr.IVisitor<string> {
    public string VisitAssignExpression( Expr.AssignExpression expr ) {
        throw new NotImplementedException( );
    }

    public string VisitBinaryExpression( Expr.BinaryExpression expr ) {
        return $"{expr.Left.Accept( this )} {expr.Right.Accept( this )} {expr.Operator.Lexeme}";
    }

    public string VisitGroupingExpression( Expr.GroupingExpression expr ) {
        return expr.Expression.Accept( this );
    }

    public string VisitLiteralExpression( Expr.LiteralExpression expr ) {
        return expr.Value == null ? "nil" : expr.Value.ToString( );
    }

    public string VisitUnaryExpression( Expr.UnaryExpression expr ) {
        return $"{expr.Right.Accept( this )} {expr.Operator.Lexeme}";
    }

    public string VisitVariableExpression( Expr.VariableExpression expr ) {
        throw new NotImplementedException( );
    }

    public string VisitLogicalExpression( Expr.LogicalExpression expr ) {
        throw new NotImplementedException( );
    }

    public string Print( Expr expr ) {
        return expr.Accept( this );
    }
}
