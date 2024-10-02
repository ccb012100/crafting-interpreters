namespace cslox.Extensions;

internal static class CharExtensions {
    public static bool IsDigit( this char c ) {
        return c is >= '0' and <= '9';
    }

    public static bool IsAlpha( this char c ) {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    public static bool IsAlphaNumeric( this char c ) {
        return c.IsAlpha( ) || c.IsDigit( );
    }
}
