using static cslox.DataTypes.Expr;
using static cslox.DataTypes.Stmt;

namespace cslox.Analyzers;

internal class Parser( List<Token> tokens ) {
    private int _current;

    public List<Stmt> Parse( ) {
        List<Stmt> statements = [];

        while ( !IsAtEnd( ) ) {
            statements.Add( Declaration( ) );
        }

        return statements;
    }

    #region Stmt

    private Stmt Statement( ) {
        if ( Match( PRINT ) ) {
            return PrintStatement( );
        }

        if ( Match( LEFT_BRACE ) ) {
            return new BlockStatement( Block( ) );
        }

        return ExpressionStatement( );
    }

    private PrintStatement PrintStatement( ) {
        Expr value = Expression( );

        Consume( SEMICOLON , "Expect ';' after value." );

        return new PrintStatement( value );
    }

    private VarStatement VarDeclaration( ) {
        Token name = Consume( IDENTIFIER , "Expect variable name." );

        Expr initializer = null;

        if ( Match( EQUAL ) ) {
            initializer = Expression( );
        }

        Consume( SEMICOLON , "Expect ';' after variable declaration." );

        return new VarStatement( name , initializer );
    }

    private ExpressionStatement ExpressionStatement( ) {
        Expr expr = Expression( );

        Consume( SEMICOLON , "Expect ';' after expression." );

        return new ExpressionStatement( expr );
    }

    private List<Stmt> Block( ) {
        List<Stmt> statements = [];

        while ( !Check( RIGHT_BRACE ) && !IsAtEnd( ) ) {
            statements.Add( Declaration( ) );
        }

        Consume( RIGHT_BRACE , "Expect '}' after block" );

        return statements;
    }

    private Stmt Declaration( ) {
        try {
            return Match( VAR ) ? VarDeclaration( ) : Statement( );
        } catch ( ParseError ) {
            Synchronize( );

            return null;
        }
    }

    #endregion

    #region Expr

    private Expr Expression( ) {
        return Ternary( );
    }

    private Expr Ternary( ) {
        Expr expr = Comma( );

        if ( Match( QUESTION_MARK ) ) {
            _ = Ternary( );

            Consume( COLON , "Expect ':' after expression." );

            expr = Ternary( );
        }

        return expr;
    }

    private Expr Comma( ) {
        Expr expr = Assignment( );

        while ( Match( COMMA ) ) {
            expr = Assignment( );
        }

        return expr;
    }

    private Expr Assignment( ) {
        Expr expr = Equality( );

        if ( Match( EQUAL ) ) {
            Token equals = Previous( );
            Expr value = Assignment( );

            if ( expr is VariableExpression varExpr ) {
                Token name = varExpr.Name;

                return new AssignExpression( name , value );
            }

            Error( equals , "Invalid assignment target." );
        }

        return expr;
    }

    private Expr Equality( ) {
        if ( Peek( ).Type is BANG_EQUAL or EQUAL_EQUAL ) {
            Token @operator = Advance( );
            _ = Comparison( ); // right-hand operand

            throw Error( @operator , "Missing left-hand operand" );
        }

        Expr expr = Comparison( );

        while ( Match( BANG_EQUAL , EQUAL_EQUAL ) ) {
            Token @operator = Previous( );
            Expr right = Comparison( );
            expr = new BinaryExpression( expr , @operator , right );
        }

        return expr;
    }

    private Expr Comparison( ) {
        if ( Peek( ).Type is GREATER or GREATER_EQUAL or LESS or LESS_EQUAL ) {
            Token @operator = Advance( );
            _ = Comparison( ); // right-hand operand

            throw Error( @operator , "Missing left-hand operand" );
        }

        Expr expr = Term( );

        while ( Match( GREATER , GREATER_EQUAL , LESS , LESS_EQUAL ) ) {
            Token @operator = Previous( );
            Expr right = Term( );
            expr = new BinaryExpression( expr , @operator , right );
        }

        return expr;
    }

    private Expr Term( ) {
        if ( Peek( ).Type is MINUS or PLUS ) {
            Token @operator = Advance( );
            _ = Comparison( ); // right-hand operand

            throw Error( @operator , "Missing left-hand operand" );
        }

        Expr expr = Factor( );

        while ( Match( MINUS , PLUS ) ) {
            Token @operator = Previous( );
            Expr right = Factor( );
            expr = new BinaryExpression( expr , @operator , right );
        }

        return expr;
    }

    private Expr Factor( ) {
        if ( Peek( ).Type is SLASH or STAR ) {
            Token @operator = Advance( );
            _ = Comparison( ); // right-hand operand

            throw Error( @operator , "Missing left-hand operand" );
        }

        Expr expr = Unary( );

        while ( Match( SLASH , STAR ) ) {
            Token @operator = Previous( );
            Expr right = Unary( );
            expr = new BinaryExpression( expr , @operator , right );
        }

        return expr;
    }

    private Expr Unary( ) {
        if ( Match( BANG , MINUS ) ) {
            Token @operator = Previous( );
            Expr right = Unary( );

            return new UnaryExpression( @operator , right );
        }

        return Primary( );
    }

    private Expr Primary( ) {
        if ( Match( FALSE ) ) {
            return new LiteralExpression( false );
        }

        if ( Match( TRUE ) ) {
            return new LiteralExpression( true );
        }

        if ( Match( NIL ) ) {
            return new LiteralExpression( null );
        }

        if ( Match( NUMBER , STRING ) ) {
            return new LiteralExpression( Previous( ).Literal );
        }

        if ( Match( IDENTIFIER ) ) {
            return new VariableExpression( Previous( ) );
        }

        if ( Match( LEFT_PAREN ) ) {
            Expr expr = Expression( );
            Consume( RIGHT_PAREN , "Expect ')' after expression." );

            return new GroupingExpression( expr );
        }

        throw Error( Peek( ) , "Expect expression." );
    }

    #endregion

    private bool Match( params TokenType[ ] types ) {
        if ( !types.Any( Check ) ) {
            return false;
        }

        Advance( );

        return true;
    }

    private bool Check( TokenType type ) {
        if ( IsAtEnd( ) ) {
            return false;
        }

        return Peek( ).Type == type;
    }

    private Token Advance( ) {
        if ( !IsAtEnd( ) ) {
            _current++;
        }

        return Previous( );
    }

    private bool IsAtEnd( ) {
        return Peek( ).Type == EOF;
    }

    private Token Peek( ) {
        return tokens.ElementAt( _current );
    }

    private Token Previous( ) {
        return tokens.ElementAt( _current - 1 );
    }

    private Token Consume( TokenType type , string message ) {
        if ( Check( type ) ) {
            return Advance( );
        }

        throw Error( Peek( ) , message );
    }

    private static ParseError Error( Token token , string message ) {
        Lox.Error( token , message );

        return new ParseError( );
    }

    private void Synchronize( ) {
        Advance( );

        while ( !IsAtEnd( ) ) {
            if ( Previous( ).Type == SEMICOLON ) {
                return;
            }

            switch ( Peek( ).Type ) {
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

            Advance( );
        }
    }

    private class ParseError : Exception;
}
