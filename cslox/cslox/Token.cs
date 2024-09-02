namespace cslox;

internal class Token
{
    public readonly string Lexeme;
    public readonly object? Literal;
    public readonly TokenType Type;
    public int Line;

    public Token( TokenType type, string lexeme, object? literal, int line )
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString()
    {
        return $"{Type} {Lexeme} {Literal}";
    }
}
