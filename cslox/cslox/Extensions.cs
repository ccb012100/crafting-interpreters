using System.Text.Json;

namespace cslox;

internal static class Extensions
{
    private static readonly JsonSerializerOptions s_sOptions = new() { WriteIndented = false };
    private static readonly JsonSerializerOptions s_sPrettyPrintOptions = new() { WriteIndented = true };

    public static string toJson( this object obj, bool prettyPrint = true )
    {
        return JsonSerializer.Serialize( obj, prettyPrint ? s_sPrettyPrintOptions : s_sOptions );
    }

    public static bool isDigit( this char c )
    {
        return c is >= '0' and <= '9';
    }

    public static bool isAlpha( this char c )
    {
        return c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';
    }

    public static bool isAlphaNumeric( this char c )
    {
        return c.isAlpha() || c.isDigit();
    }
}
