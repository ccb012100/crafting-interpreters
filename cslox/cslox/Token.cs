namespace cslox;

public class Token
{
    public readonly string Lexeme;
    public readonly int Line;
    public readonly object Literal;
    public readonly TokenType Type;

    public Token( TokenType type, string lexeme, object literal, int line )
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
