using System.Diagnostics.CodeAnalysis;

namespace cslox;

using static TokenType;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Scanner
{
    private int current;
    private int line = 1;
    private int start;

    private readonly List<Token> tokens = new();
    private readonly string source;

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!isAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, string.Empty, null, line));

        return tokens;
    }

    private bool isAtEnd() => current >= source.Length;

    private void ScanToken()
    {
        char c = advance();

        switch (c)
        {
            case '(':
                addToken(LEFT_PAREN);
                break;
            case ')':
                addToken(RIGHT_PAREN);
                break;
            case '{':
                addToken(LEFT_BRACE);
                break;
            case '}':
                addToken(RIGHT_BRACE);
                break;
            case ',':
                addToken(COMMA);
                break;
            case '.':
                addToken(DOT);
                break;
            case '-':
                addToken(MINUS);
                break;
            case '+':
                addToken(PLUS);
                break;
            case ';':
                addToken(SEMICOLON);
                break;
            case '*':
                addToken(STAR);
                break;
            case '!':
                addToken(match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                addToken(match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                addToken(match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                addToken(match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                /*
                 * REVIEW: why don't we just increment _line? What do we gain from advancing through each character,
                 * which seems less efficient? Is it because of the IsAtEnd method? Maybe we should just set
                 * _current to the last character?
                 */
                // A comment goes until the end of the line
                if (match('/'))
                    while (Peek() != '\n' && !isAtEnd())
                        advance();
                else addToken(SLASH);

                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                line++;
                break;
            case '"':
                @string('"');
                break;
            case '\'':
                @string('\'');
                break;
            default:
                if (isDigit(c)) number();
                // TODO: handle consecutive unexpected chars as a single Error
                else Lox.Error(line, $"Unexpected character '{c}'.");
                break;
        }
    }

    private char advance() => source[current++];

    private void addToken(TokenType type) => addToken(type, null);

    private void addToken(TokenType type, object? literal)
    {
        string text = source.Substring(start, current + 1); // [start, current]

        tokens.Add(new Token(type, text, literal, line));
    }

    private bool match(char expected)
    {
        // NOTE: If there is a match, this method increments the value of <see cref="_current"/></remarks>
        if (isAtEnd()) return false;

        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private char Peek() => isAtEnd() ? '\0' : source[current];

    private void @string(char quoteType)
    {
        // Strings are multi-line and can be wrapped in single or double quotes
        while (Peek() != quoteType && !isAtEnd())
        {
            if (Peek() == '\n') line++;
            advance();
        }

        if (isAtEnd())
        {
            Lox.Error(line, "Unterminated string.");
            return;
        }

        // The closing " or '
        advance();

        // Trim the surrounding quotes.
        string value = source.Substring(start + 1, current - 1);
        addToken(STRING, value);
    }
}