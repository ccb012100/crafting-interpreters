namespace cslox;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();

    private bool IsAtEnd => _current >= _source.Length;

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public Scanner(string source)
    {
        this._source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, string.Empty, null, _line));

        return _tokens;
    }

    private void ScanToken()
    {
    }
}