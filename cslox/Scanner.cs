namespace cslox;

using static TokenType;

public class Scanner
{
    private int _current;
    private int _line = 1;
    private int _start;

    private readonly List<Token> _tokens = new();
    private readonly string _source;

    public Scanner(string source)
    {
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(EOF, string.Empty, null, _line));

        return _tokens;
    }

    private bool IsAtEnd() => _current >= _source.Length;

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(':
                AddToken(LEFT_PAREN);
                break;
            case ')':
                AddToken(RIGHT_PAREN);
                break;
            case '{':
                AddToken(LEFT_BRACE);
                break;
            case '}':
                AddToken(RIGHT_BRACE);
                break;
            case ',':
                AddToken(COMMA);
                break;
            case '.':
                AddToken(DOT);
                break;
            case '-':
                AddToken(MINUS);
                break;
            case '+':
                AddToken(PLUS);
                break;
            case ';':
                AddToken(SEMICOLON);
                break;
            case '*':
                AddToken(STAR);
                break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            default:
                Lox.Error(_line, $"Unexpected character '{c}'.");
                break;
        }
    }

    private char Advance() => _source[_current++];

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object literal)
    {
        string text = _source.Substring(_start, _current + 1); // [start, current]
        _tokens.Add(new Token(type, text, literal, _line));
    }

    /// <summary>
    ///  See if _current matches <paramref name="expected"/>.
    ///  Used to help us handle multiple-character tokens.
    /// </summary>
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;

        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }
}