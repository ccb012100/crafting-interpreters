﻿using cslox.Extensions;

namespace cslox.Analyzers;

internal class Scanner( string source ) {
    private static readonly Dictionary<string , TokenType> s_keywords = new( ) {
        { "and" , AND } ,
        { "break" , BREAK } ,
        { "class" , CLASS } ,
        { "else" , ELSE } ,
        { "false" , FALSE } ,
        { "for" , FOR } ,
        { "fun" , FUN } ,
        { "if" , IF } ,
        { "nil" , NIL } ,
        { "or" , OR } ,
        { "print" , PRINT } ,
        { "return" , RETURN } ,
        { "super" , SUPER } ,
        { "this" , THIS } ,
        {"trait", TRAIT},
        { "true" , TRUE } ,
        { "var" , VAR } ,
        { "while" , WHILE },
        {"with",    WITH}
    };

    private readonly List<char> _scanningErrors = [ ];
    private readonly List<Token> _tokens = [ ];
    private int _current;
    private int _line = 1;
    private int _start;

    public List<Token> ScanTokens( ) {
        while ( !IsAtEnd( ) ) {
            _start = _current;

            if ( ScanToken( ) ) {
                continue;
            }

            /* HACK: If no errors occurred, print out the previous errors (if any). This is an
             * ugly way of being able to track a string of bad characters, but works for now. */
            switch ( _scanningErrors.Count ) {
                // previous char wasn't an error
                case 0:
                    continue;
                // previous char was an error
                case 1:
                    Lox.Error( _line , $"Unexpected character '{_scanningErrors[0]}'." );

                    break;
                // string of characters up to the previous char were errors
                default:
                    Lox.Error( _line , $"Unexpected characters \"{string.Join( string.Empty , _scanningErrors )}\"." );

                    break;
            }

            _scanningErrors.Clear( );
        }

        _tokens.Add( new Token( EOF , string.Empty , null , _line ) );

        return _tokens;
    }

    private bool IsAtEnd( ) {
        return _current >= source.Length;
    }

    private bool ScanToken( ) {
        bool error = false;

        char c = Advance( );

        switch ( c ) {
            case '(':
                AddToken( LEFT_PAREN );

                break;
            case ')':
                AddToken( RIGHT_PAREN );

                break;
            case '{':
                AddToken( LEFT_BRACE );

                break;
            case '}':
                AddToken( RIGHT_BRACE );

                break;
            case ',':
                AddToken( COMMA );

                break;
            case '.':
                AddToken( DOT );

                break;
            case '-':
                AddToken( MINUS );

                break;
            case '+':
                AddToken( PLUS );

                break;
            case ':':
                AddToken( COLON );

                break;
            case ';':
                AddToken( SEMICOLON );

                break;
            case '*':
                AddToken( STAR );

                break;
            case '?':
                AddToken( QUESTION_MARK );

                break;
            case '!':
                AddToken( Match( '=' ) ? BANG_EQUAL : BANG );

                break;
            case '=':
                AddToken( Match( '=' ) ? EQUAL_EQUAL : EQUAL );

                break;
            case '<':
                AddToken( Match( '=' ) ? LESS_EQUAL : LESS );

                break;
            case '>':
                AddToken( Match( '=' ) ? GREATER_EQUAL : GREATER );

                break;
            case '/':
                /*
                 * REVIEW: why don't we just increment _line? What do we gain from advancing
                 * through each character, which seems less efficient? Is it because of the
                 * IsAtEnd method? Maybe we should just set _current to the last character?
                 */
                if ( Match( '/' ) ) {
                    // comment goes until the end of the line
                    while ( !IsAtEnd( ) && Peek( ) != '\n' ) {
                        Advance( );
                    }
                } else if ( Match( '*' ) ) {
                    /* C-style block comments can be multi-line and _do not_ support nested block comments */
                    string nextTwo = $"{Peek( )}{PeekNext( )}";

                    while ( !IsAtEnd( ) && nextTwo != "*/" ) {
                        Advance( );
                        nextTwo = $"{Peek( )}{PeekNext( )}";
                    }

                    if ( IsAtEnd( ) && nextTwo != "*/" ) {
                        Lox.Error( _line , "Block comment has no closing tag; reached EOF." );
                    } else {
                        // advance 2x to move past the "*/" that ends the comment
                        Advance( );
                        Advance( );
                    }
                } else {
                    AddToken( SLASH );
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
                AddString( '"' );

                break;
            case '\'':
                AddString( '\'' );

                break;
            default:
                if ( c.IsDigit( ) ) {
                    AddNumber( );
                } else if ( c.IsAlpha( ) ) {
                    AddIdentifier( );
                } else // Bad character encountered
                {
                    error = true;
                    _scanningErrors.Add( c );
                }

                break;
        }

        return error;
    }

    private char Advance( ) {
        return source[_current++];
    }

    private void AddToken( TokenType type , object literal = null ) {
        string text = source.Substring( _start , _current - _start ); // [start, current]

        _tokens.Add( new Token( type , text , literal , _line ) );
    }

    private bool Match( char expected ) {
        // NOTE: If there is a match, this method increments the value of _current
        if ( IsAtEnd( ) ) {
            return false;
        }

        if ( source[_current] != expected ) {
            return false;
        }

        _current++;

        return true;
    }

    private char Peek( ) {
        return IsAtEnd( ) ? '\0' : source[_current];
    }

    private char PeekNext( ) {
        return _current + 1 >= source.Length ? '\0' : source[_current + 1];
    }

    private void AddString( char quoteType ) {
        // Strings are multi-line and can be wrapped in single or double quotes
        while ( Peek( ) != quoteType && !IsAtEnd( ) ) {
            if ( Peek( ) == '\n' ) {
                _line++;
            }

            Advance( );
        }

        if ( IsAtEnd( ) ) {
            Lox.Error( _line , "Unterminated string." );

            return;
        }

        // The closing " or '
        Advance( );

        // Trim the surrounding quotes
        string value = source.Substring( _start + 1 , _current - _start - 2 );
        AddToken( STRING , value );
    }

    private void AddNumber( ) {
        while ( Peek( ).IsDigit( ) ) {
            Advance( );
        }

        // Look for a fractional part.
        if ( Peek( ) == '.' && PeekNext( ).IsDigit( ) ) {
            // Consume the "."
            Advance( );

            while ( Peek( ).IsDigit( ) ) {
                Advance( );
            }
        }

        AddToken( NUMBER , double.Parse( source.Substring( _start , _current - _start ) ) );
    }

    private void AddIdentifier( ) {
        while ( Peek( ).IsAlphaNumeric( ) ) {
            Advance( );
        }

        string text = source.Substring( _start , _current - _start );

        TokenType type = s_keywords.GetValueOrDefault( text , IDENTIFIER );

        AddToken( type );
    }
}
