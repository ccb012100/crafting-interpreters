namespace cslox.Analyzers;

internal class Parser( List<Token> tokens ) {
    private int _current;
    private int _loopDepth;

    public List<Stmt> Parse( ) {
        List<Stmt> statements = [ ];

        while ( !IsAtEnd( ) ) {
            statements.Add( Declaration( ) );
        }

        return statements;
    }

    public Expr ParseExpression( ) {
        return Expression( );
    }

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

    private bool CheckNext( TokenType type ) {
        if ( IsAtEnd( ) ) {
            return false;
        }

        return tokens[_current + 1].Type switch {
            EOF => false,
            _ => tokens[_current + 1].Type == type
        };
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

    public class ParseError : Exception;

    #region Stmt

    private Stmt Statement( ) {
        if ( Match( BREAK ) ) {
            return BreakStatement( );
        }

        if ( Match( FOR ) ) {
            return ForStatement( );
        }

        if ( Match( IF ) ) {
            return IfStatement( );
        }

        if ( Match( PRINT ) ) {
            return PrintStatement( );
        }

        if ( Match( RETURN ) ) {
            return ReturnStatement( );
        }

        if ( Match( WHILE ) ) {
            return WhileStatement( );
        }

        if ( Match( LEFT_BRACE ) ) {
            return new Stmt.Block( Block( ) );
        }

        return ExpressionStatement( );
    }

    private Stmt.Break BreakStatement( ) {
        if ( _loopDepth == 0 ) {
            throw Error( Previous( ) , "Must be inside a loop to use 'break'." );
        }

        Consume( SEMICOLON , "Expect ';' after 'break'." );

        return new Stmt.Break( );
    }

    private Stmt ForStatement( ) {
        Consume( LEFT_PAREN , "Expect '(' after 'for'." );
        Stmt initializer;

        if ( Match( SEMICOLON ) ) {
            initializer = null;
        } else if ( Match( VAR ) ) {
            initializer = VarDeclaration( );
        } else {
            initializer = ExpressionStatement( );
        }

        Expr condition = Expression( );

        if ( !Check( SEMICOLON ) ) {
            condition = Expression( );
        }

        Consume( SEMICOLON , "Expect ';' after loop condition." );

        Expr increment = null;

        if ( !Check( RIGHT_PAREN ) ) {
            increment = Expression( );
        }

        Consume( RIGHT_PAREN , "Expect ')' after for clauses." );

        try {
            _loopDepth++;
            Stmt body = Statement( );

            if ( increment is not null ) {
                body = new Stmt.Block( [body , new Stmt.ExpressionStmt( increment )] );
            }

            condition ??= new Expr.Literal( true );

            body = new Stmt.While( condition , body );

            if ( initializer is not null ) {
                body = new Stmt.Block( [initializer , body] );
            }

            return body;
        } finally {
            _loopDepth--;
        }
    }

    private Stmt.If IfStatement( ) {
        Consume( LEFT_PAREN , "Expect '(' after 'if'." );
        Expr condition = Expression( );
        Consume( RIGHT_PAREN , "Expect ')' after if condition." );

        Stmt thenBranch = Statement( );
        Stmt elseBranch = null;

        if ( Match( ELSE ) ) {
            elseBranch = Statement( );
        }

        return new Stmt.If( condition , thenBranch , elseBranch );
    }

    private Stmt.Print PrintStatement( ) {
        Expr value = Expression( );

        Consume( SEMICOLON , "Expect ';' after value." );

        return new Stmt.Print( value );
    }

    private Stmt ReturnStatement( ) {
        Token keyword = Previous( );
        Expr value = null;

        if ( !Check( SEMICOLON ) ) {
            value = Expression( );
        }

        Consume( SEMICOLON , "Expect ';' after return value." );

        return new Stmt.Return( keyword , value );
    }

    private Stmt.While WhileStatement( ) {
        Consume( LEFT_PAREN , "Expect '(' after 'while'." );
        Expr condition = Expression( );
        Consume( RIGHT_PAREN , "Expect ')' after condition." );

        try {
            _loopDepth++;
            Stmt body = Statement( );

            return new Stmt.While( condition , body );
        } finally {
            _loopDepth--;
        }
    }

    private Stmt.Var VarDeclaration( ) {
        Token name = Consume( IDENTIFIER , "Expect variable name." );

        Expr initializer = null;

        if ( Match( EQUAL ) ) {
            initializer = Expression( );
        }

        Consume( SEMICOLON , "Expect ';' after variable declaration." );

        return new Stmt.Var( name , initializer );
    }

    private Stmt.ExpressionStmt ExpressionStatement( ) {
        Expr expr = Expression( );

        Consume( SEMICOLON , "Expect ';' after expression." );

        return new Stmt.ExpressionStmt( expr );
    }

    private List<Stmt> Block( ) {
        List<Stmt> statements = [ ];

        while ( !Check( RIGHT_BRACE ) && !IsAtEnd( ) ) {
            statements.Add( Declaration( ) );
        }

        Consume( RIGHT_BRACE , "Expect '}' after block" );

        return statements;
    }

    private Stmt Declaration( ) {
        try {
            if ( Match( CLASS ) ) {
                return ClassDeclaration( );
            }

            if ( Check( FUN ) && CheckNext( IDENTIFIER ) ) {
                Consume( FUN , null );

                return Function( "function" );
            }

            if ( Match( VAR ) ) {
                return VarDeclaration( );
            }

            return Statement( );
        } catch ( ParseError ) {
            Synchronize( );

            return null;
        }
    }

    private Stmt.Class ClassDeclaration( ) {
        Token name = Consume( IDENTIFIER , "Expect class name." );

        Expr.Variable superclass = null;

        if ( Match( LESS ) ) {
            Consume( IDENTIFIER , "Expect superclass name." );
            superclass = new Expr.Variable( Previous( ) );
        }

        Consume( LEFT_BRACE , "Expect '{' before class body." );

        List<Stmt.FunctionStmt> methods = [ ];

        while ( !Check( RIGHT_BRACE ) && !IsAtEnd( ) ) {
            methods.Add( Function( "method" ) );
        }

        Consume( RIGHT_BRACE , "Expect '}' after class body." );

        return new Stmt.Class( name , superclass , methods );
    }

    private Stmt.FunctionStmt Function( string kind ) {
        Token name = Consume( IDENTIFIER , $"Expect {kind} name." );

        return new Stmt.FunctionStmt( name , FunctionBody( kind ) );
    }

    private Expr.Function FunctionBody( string kind ) {
        Consume( LEFT_PAREN , $"Expect '(' after {kind} name." );

        List<Token> parameters = [ ];

        if ( !Check( RIGHT_PAREN ) ) {
            do {
                if ( parameters.Count >= 255 ) {
                    Error( Peek( ) , "Can't have more than 255 parameters." );
                }

                parameters.Add( Consume( IDENTIFIER , "Expect parameter name." ) );
            } while ( Match( COMMA ) );
        }

        Consume( RIGHT_PAREN , "Expect ')' after parameters." );
        Consume( LEFT_BRACE , $"Expect '{{' before {kind} body." );

        List<Stmt> body = Block( );

        return new Expr.Function( parameters , body );
    }

    #endregion

    #region Expr

    private Expr Expression( ) {
        return Comma( );
    }

    private Expr Comma( ) {
        Expr expr = Assignment( );

        while ( Match( COMMA ) ) {
            Token @operator = Previous( );
            Expr right = Assignment( );

            expr = new Expr.Binary( expr , @operator , right );
        }

        return expr;
    }

    private Expr Assignment( ) {
        Expr expr = Conditional( );

        if ( Match( EQUAL ) ) {
            Token equals = Previous( );
            Expr value = Assignment( );

            switch ( expr ) {
                case Expr.Variable varExpr: {
                        Token name = varExpr.Name;

                        return new Expr.Assign( name , value );
                    }
                case Expr.Get getExpr: {
                        return new Expr.Set( getExpr.Object , getExpr.Name , value );
                    }
                default: {
                        Error( equals , "Invalid assignment target." );

                        break;
                    }
            }
        }

        return expr;
    }

    private Expr Conditional( ) {
        Expr expr = Or( );

        if ( Match( QUESTION_MARK ) ) {
            Expr thenBranch = Expression( );

            Consume( COLON , "Expect ':' after expression." );

            Expr elseBranch = Conditional( );

            expr = new Expr.Conditional( expr , thenBranch , elseBranch );
        }

        return expr;
    }

    private Expr Or( ) {
        Expr expr = And( );

        while ( Match( OR ) ) {
            Token @operator = Previous( );
            Expr right = And( );

            expr = new Expr.Logical( expr , @operator , right );
        }

        return expr;
    }

    private Expr And( ) {
        Expr expr = Equality( );

        while ( Match( AND ) ) {
            Token @operator = Previous( );
            Expr right = Equality( );

            expr = new Expr.Logical( expr , @operator , right );
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
            expr = new Expr.Binary( expr , @operator , right );
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
            expr = new Expr.Binary( expr , @operator , right );
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
            expr = new Expr.Binary( expr , @operator , right );
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
            expr = new Expr.Binary( expr , @operator , right );
        }

        return expr;
    }

    private Expr Unary( ) {
        if ( Match( BANG , MINUS ) ) {
            Token @operator = Previous( );
            Expr right = Unary( );

            return new Expr.Unary( @operator , right );
        }

        return Call( );
    }

    private Expr Call( ) {
        Expr expr = Primary( );

        while ( true ) {
            if ( Match( LEFT_PAREN ) ) {
                expr = FinishCall( expr );
            } else if ( Match( DOT ) ) {
                Token name = Consume( IDENTIFIER , "Expect property name after '.'." );
                expr = new Expr.Get( expr , name );
            } else {
                break;
            }
        }

        return expr;
    }

    private Expr.Call FinishCall( Expr callee ) {
        List<Expr> arguments = [ ];

        if ( !Check( RIGHT_PAREN ) ) {
            do {
                if ( arguments.Count >= 255 ) {
                    Error( Peek( ) , "Can't have more than 255 arguments." );
                }

                arguments.Add( Assignment( ) );
            } while ( Match( COMMA ) );
        }

        Token paren = Consume( RIGHT_PAREN , "Expect ')' after arguments." );

        return new Expr.Call( callee , paren , arguments );
    }

    private Expr Primary( ) {
        if ( Match( FUN ) ) {
            return FunctionBody( "function" );
        }

        if ( Match( FALSE ) ) {
            return new Expr.Literal( false );
        }

        if ( Match( TRUE ) ) {
            return new Expr.Literal( true );
        }

        if ( Match( NIL ) ) {
            return new Expr.Literal( null );
        }

        if ( Match( NUMBER , STRING ) ) {
            return new Expr.Literal( Previous( ).Literal );
        }

        if ( Match( THIS ) ) {
            return new Expr.This( Previous( ) );
        }

        if ( Match( IDENTIFIER ) ) {
            return new Expr.Variable( Previous( ) );
        }

        if ( Match( LEFT_PAREN ) ) {
            Expr expr = Expression( );
            Consume( RIGHT_PAREN , "Expect ')' after expression." );

            return new Expr.Grouping( expr );
        }

        throw Error( Peek( ) , "Expect expression." );
    }

    #endregion
}
