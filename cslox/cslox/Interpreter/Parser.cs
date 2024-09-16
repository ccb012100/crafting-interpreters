using cslox.DataTypes;

namespace cslox.Interpreter;
/*
 * ┌──────────────────────────────────────────────────────────────────────────┐
 * │                       Expression Grammar                                 │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │    expression      →    ternary ;                                        │
 * │    ternary         →    comma ( "?" comma ":" comma )* ;                 │
 * │    comma           →    "(" equality ( "," equality )* ;                 │
 * │    equality        →    comparison ( ( "!=" | "==" ) comparison )* ;     │
 * │    comparison      →    term ( ( ">" | ">=" | "<" | "<=" ) term )* ;     │
 * │    term            →    factor ( ( "-" | "+" ) factor )* ;               │
 * │    factor          →    unary ( ( "/" | "*" ) unary )* ;                 │
 * │    unary           →    ( "!" | "-" ) unary                              │
 * │                    |    primary ;                                        │
 * │    primary         →    NUMBER | STRING | "true" | "false" | "nil"       │
 * │                    |    "(" expression ")" ;                             │
 * └──────────────────────────────────────────────────────────────────────────┘
 *
 * ┌──────────────────────────────────────────────────────────────────────────┐
 * │    Grammar notation    |   Code representation                           │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │    Terminal            |   Code to match and consume a token             │
 * │    Non-terminal        |   Call to that rule’s function                  │
 * │    |                   |   if or switch statement                        │
 * │    * or +              |   while or for loop                             │
 * │    ?                   |   if statement                                  │
 * └──────────────────────────────────────────────────────────────────────────┘
 */
internal class Parser( List<Token> tokens )
{
    private readonly List<Token> _tokens = tokens;

    private int _current;

    public Expr Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Ternary();
    }

    private Expr Ternary()
    {
        Expr expr = Comma();

        if (Match( QUESTION_MARK ))
        {
            _ = Ternary();

            Consume( COLON, "Expect ':' after expression." );

            expr = Ternary();
        }

        return expr;
    }

    private Expr Comma()
    {
        Expr expr = Equality();

        while (Match( COMMA ))
        {
            expr = Equality();
        }

        return expr;
    }

    private Expr Equality()
    {
        if (Peek().Type is BANG_EQUAL or EQUAL_EQUAL)
        {
            Token @operator = Advance();
            _ = Comparison(); // right-hand operand

            throw Error( @operator, "Missing left-hand operand" );
        }

        Expr expr = Comparison();

        while (Match( BANG_EQUAL, EQUAL_EQUAL ))
        {
            Token @operator = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr Comparison()
    {
        if (Peek().Type is GREATER or GREATER_EQUAL or LESS or LESS_EQUAL)
        {
            Token @operator = Advance();
            _ = Comparison(); // right-hand operand

            throw Error( @operator, "Missing left-hand operand" );
        }

        Expr expr = Term();

        while (Match( GREATER, GREATER_EQUAL, LESS, LESS_EQUAL ))
        {
            Token @operator = Previous();
            Expr right = Term();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr Term()
    {
        if (Peek().Type is MINUS or PLUS)
        {
            Token @operator = Advance();
            _ = Comparison(); // right-hand operand

            throw Error( @operator, "Missing left-hand operand" );
        }

        Expr expr = Factor();

        while (Match( MINUS, PLUS ))
        {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr Factor()
    {
        if (Peek().Type is SLASH or STAR)
        {
            Token @operator = Advance();
            _ = Comparison(); // right-hand operand

            throw Error( @operator, "Missing left-hand operand" );
        }

        Expr expr = Unary();

        while (Match( SLASH, STAR ))
        {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new Expr.Binary( expr, @operator, right );
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match( BANG, MINUS ))
        {
            Token @operator = Previous();
            Expr right = Unary();

            return new Expr.Unary( @operator, right );
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match( FALSE ))
        {
            return new Expr.Literal( false );
        }

        if (Match( TRUE ))
        {
            return new Expr.Literal( true );
        }

        if (Match( NIL ))
        {
            return new Expr.Literal( null );
        }

        if (Match( NUMBER, STRING ))
        {
            return new Expr.Literal( Previous().Literal );
        }

        if (Match( LEFT_PAREN ))
        {
            Expr expr = Expression();
            Consume( RIGHT_PAREN, "Expect ')' after expression." );

            return new Expr.Grouping( expr );
        }

        throw Error( Peek(), "Expect expression." );
    }

    private bool Match( params TokenType[] types )
    {
        if (!types.Any( Check ))
        {
            return false;
        }

        Advance();

        return true;
    }

    private bool Check( TokenType type )
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return _tokens.ElementAt( _current );
    }

    private Token Previous()
    {
        return _tokens.ElementAt( _current - 1 );
    }

    private Token Consume( TokenType type, string message )
    {
        if (Check( type ))
        {
            return Advance();
        }

        throw Error( Peek(), message );
    }

    private static ParseError Error( Token token, string message )
    {
        Lox.Error( token, message );

        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON)
            {
                return;
            }

            switch (Peek().Type)
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

            Advance();
        }
    }

    private class ParseError : Exception;
}
