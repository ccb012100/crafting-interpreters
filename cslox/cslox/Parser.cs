namespace cslox;

using static TokenType;

internal class Parser
{
    private readonly List<Token> _tokens;

    private int _current;

    public Parser( List<Token> tokens )
    {
        _tokens = tokens;
    }

    private Expr expression()
    {
        return equality();
    }

    private Expr equality()
    {
        Expr expr = comparison();

        while (match( BANG_EQUAL, EQUAL_EQUAL ))
        {
            Token @operator = previous();
            Expr right = comparison();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private bool match( params TokenType[] types )
    {
        if (!types.Any( check ))
        {
            return false;
        }

        advance();

        return true;
    }

    private bool check( TokenType type )
    {
        if (isAtEnd())
        {
            return false;
        }

        return peek().Type == type;
    }

    private Token advance()
    {
        if (!isAtEnd())
        {
            _current++;
        }

        return previous();
    }

    private bool isAtEnd()
    {
        return peek().Type == EOF;
    }

    private Token peek()
    {
        return _tokens.ElementAt( _current );
    }

    private Token previous()
    {
        return _tokens.ElementAt( _current - 1 );
    }

    private Expr comparison()
    {
        Expr expr = term();

        while (match( GREATER, GREATER_EQUAL, LESS, LESS_EQUAL ))
        {
            Token @operator = previous();
            Expr right = term();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr term()
    {
        Expr expr = factor();

        while (match( MINUS, PLUS ))
        {
            Token @operator = previous();
            Expr right = factor();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr factor()
    {
        Expr expr = unary();

        while (match( SLASH, STAR ))
        {
            Token @operator = previous();
            Expr right = unary();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr unary()
    {
        if (match( BANG, MINUS ))
        {
            Token @operator = previous();
            Expr right = unary();

            return new Expr.Unary( @operator, right );
        }

        return primary();
    }

    private Expr primary()
    {
        if (match( FALSE ))
        {
            return new Expr.Literal( false );
        }

        if (match( TRUE ))
        {
            return new Expr.Literal( true );
        }

        if (match( NIL ))
        {
            return new Expr.Literal( null );
        }

        if (match( NUMBER, STRING ))
        {
            return new Expr.Literal( previous().Literal );
        }

        if (match( LEFT_PAREN ))
        {
            Expr expr = expression();
            consume( RIGHT_PAREN, "Expect ')' after expression." );

            return new Expr.Grouping( expr );
        }

        throw new NotImplementedException( "This comes later in the chapter" );
    }

    private Token consume( TokenType type, string message )
    {
        if (check( type ))
        {
            return advance();
        }

        throw error( peek(), message );
    }

    private static ParseError error( Token token, string message )
    {
        Lox.error( token, message );

        return new ParseError();
    }

    private void synchronize()
    {
        advance();

        while (!isAtEnd())
        {
            if (previous().Type == SEMICOLON)
            {
                return;
            }

            switch (peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            advance();
        }
    }

    private class ParseError : Exception;
}
