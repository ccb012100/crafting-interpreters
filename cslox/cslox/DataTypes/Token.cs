namespace cslox.DataTypes;

public class Token( TokenType type , string lexeme , object literal , int line ) {
    public readonly string Lexeme = lexeme;
    public readonly int Line = line;
    public readonly object Literal = literal;
    public readonly TokenType Type = type;

    public override string ToString( ) {
        return $"{Type} {Lexeme} {Literal}".TrimEnd();
    }
}
