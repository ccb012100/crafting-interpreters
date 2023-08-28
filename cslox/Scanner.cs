namespace cslox;

using static TokenType;

public class Scanner
{
    private int current;
    private int line = 1;
    private int start;

    private readonly List<char> scanningErrors = new();
    private readonly List<Token> tokens = new();
    private readonly string source;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        {"and", AND},
        {"class", CLASS},
        {"else", ELSE},
        {"false", FALSE},
        {"for", FOR},
        {"fun", FUN},
        {"if", IF},
        {"nil", NIL},
        {"or", OR},
        {"print", PRINT},
        {"return", RETURN},
        {"super", SUPER},
        {"this", THIS},
        {"true", TRUE},
        {"var", VAR},
        {"while", WHILE}
    };

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> scanTokens()
    {
        while (!isAtEnd())
        {
            start = current;

            if (scanToken()) continue;

            // HACK: If no errors occurred, print out the previous errors (if any). This is an
            // ugly way of being able to track a string of bad characters, but works for now.
            switch (scanningErrors.Count)
            {
                // previous char wasn't an error
                case 0:
                    continue;
                // previous char was an error
                case 1:
                    Lox.error(line, $"Unexpected character '{scanningErrors[0]}'.");
                    break;
                // string of characters up to the previous char were errors
                default:
                    Lox.error(line, $"Unexpected characters \"{string.Join(string.Empty, scanningErrors)}\".");
                    break;
            }

            scanningErrors.Clear();
        }

        tokens.Add(new Token(EOF, string.Empty, null, line));

        return tokens;
    }

    private bool isAtEnd() => current >= source.Length;

    private bool scanToken()
    {
        bool error = false;

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
                {
                    /*
                     * REVIEW: why don't we just increment _line? What do we gain from advancing
                     * through each character, which seems less efficient? Is it because of the
                     * IsAtEnd method? Maybe we should just set _current to the last character?
                     */
                    if (match('/'))
                    {
                        // comment goes until the end of the line
                        while (!isAtEnd() && peek() != '\n')
                            advance();
                    }
                    else if (match('*'))
                    {
                        /*
                         * C-style block comments can be multi-line and _do not_ support nested block comments
                         */
                        string nextTwo = $"{peek()}{peekNext()}";
                        while (!isAtEnd() && nextTwo != "*/")
                        {
                            advance();
                            nextTwo = $"{peek()}{peekNext()}";
                        }

                        if (isAtEnd() && nextTwo != "*/")
                        {
                            Lox.error(line, "Block comment has no closing tag; reached EOF.");
                        }
                        else
                        {
                            // advance 2x to move past the "*/" that ends the comment
                            advance();
                            advance();
                        }
                    }
                    else
                    {
                        addToken(SLASH);
                    }

                    break;
                }

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                line++;
                break;
            case '"':
                addString('"');
                break;
            case '\'':
                addString('\'');
                break;
            default:
                {
                    if (c.isDigit())
                    {
                        addNumber();
                    }
                    else if (c.isAlpha())
                    {
                        addIdentifier();
                    }
                    // Bad character encountered
                    else
                    {
                        error = true;
                        scanningErrors.Add(c);
                    }

                    break;
                }
        }

        return error;
    }

    private char advance() => source[current++];

    private void addToken(TokenType type) => addToken(type, null);

    private void addToken(TokenType type, object? literal)
    {
        string text = source.Substring(start, current - start); // [start, current]

        tokens.Add(new Token(type, text, literal, line));
    }

    private bool match(char expected)
    {
        // NOTE: If there is a match, this method increments the value of _current
        if (isAtEnd()) return false;

        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private char peek() => isAtEnd() ? '\0' : source[current];

    private char peekNext() => (current + 1 >= source.Length) ? '\0' : source[current + 1];

    private void addString(char quoteType)
    {
        // Strings are multi-line and can be wrapped in single or double quotes
        while (peek() != quoteType && !isAtEnd())
        {
            if (peek() == '\n') line++;
            advance();
        }

        if (isAtEnd())
        {
            Lox.error(line, "Unterminated string.");
            return;
        }

        // The closing " or '
        advance();

        // Trim the surrounding quotes
        string value = source.Substring(start + 1, current - start - 2);
        addToken(STRING, value);
    }

    private void addNumber()
    {
        while (peek().isDigit()) advance();

        // Look for a fractional part.
        if (peek() == '.' && peekNext().isDigit())
        {
            // Consume the "."
            advance();

            while (peek().isDigit()) advance();
        }

        addToken(NUMBER, double.Parse(source.Substring(start, current - start)));
    }

    private void addIdentifier()
    {
        while (peek().isAlphaNumeric()) advance();

        string text = source.Substring(start, current - start);

        if (!keywords.TryGetValue(text, out TokenType type)) type = IDENTIFIER;

        addToken(type);
    }
}
