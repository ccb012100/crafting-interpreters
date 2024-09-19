namespace cslox.Extensions;

internal static class CharExtensions {
    public static bool isDigit( this char c ) {
        return c is >= '0' and <= '9';
    }

    public static bool isAlpha( this char c ) {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    public static bool isAlphaNumeric( this char c ) {
        return c.isAlpha( ) || c.isDigit( );
    }
}
