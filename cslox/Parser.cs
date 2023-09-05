namespace cslox
{
    internal class Parser
    {
        private readonly List<Token> _tokens;

        public int _current;

        public Parser(List<Token> tokens)
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

            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token @operator = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private bool match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private bool check(TokenType type)
        {
            if (isAtEnd())
                return false;

            return peek().Type == type;
        }

        private Token advance()
        {
            if (!isAtEnd())
                _current++;

            return previous();
        }

        private bool isAtEnd() => peek().Type == TokenType.EOF;

        private Token peek() => _tokens.ElementAt(_current);

        private Token previous() => _tokens.ElementAt(_current - 1);

        private Expr comparison()
        {
            Expr expr = term();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token @operator = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr term()
        {
            Expr expr = factor();

            while (match(TokenType.MINUS, TokenType.PLUS))
            {
                Token @operator = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr factor()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token @operator = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token @operator = previous();
                Expr right = unary();

                return new Expr.Unary(@operator, right);
            }

            return primary();
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE))
                return new Expr.Literal(false);
            if (match(TokenType.TRUE))
                return new Expr.Literal(true);
            if (match(TokenType.NIL))
                return new Expr.Literal(null);

            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().Literal);
            }

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");

                return new Expr.Grouping(expr);
            }

            throw new NotImplementedException("This comes later in the chapter");
        }

        private void consume(TokenType type, string message)
        {
            throw new NotImplementedException("This comes laters in the chapter");
        }
    }
}
