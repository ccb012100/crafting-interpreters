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
            case '/':
                if (Match('/'))
                {
                    /*
                     * REVIEW: why don't we just increment _line? What do we gain from advancing through each character,
                     * which seems less efficient? Is it because of the IsAtEnd method? Maybe we should just set
                     * _current to the last character?
                     */
                    // A comment goes until the end of the line
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                _line++;
                break;
            case '"':
                String('"');
                break;
            case '\'':
                String('\'');
                break;
            default:
                // TODO: handle consecutive unexpected chars as a single Error
                Lox.Error(_line, $"Unexpected character '{c}'.");
                break;
        }
    }

    private char Advance() => _source[_current++];

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object? literal)
    {
        string text = _source.Substring(_start, _current + 1); // [start, current]

        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool Match(char expected)
    {
        // NOTE: If there is a match, this method increments the value of <see cref="_current"/></remarks>
        if (IsAtEnd()) return false;

        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek() => IsAtEnd() ? '\0' : _source[_current];

    private void String(char quoteType)
    {
        // Strings are multi-line and can be wrapped in single or double quotes
        while (Peek() != quoteType && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Lox.Error(_line, "Unterminated string.");
            return;
        }

        // The closing " or '
        Advance();

        // Trim the surrounding quotes.
        string value = _source.Substring(_start + 1, _current - 1);
        AddToken(STRING, value);
    }
}