using System.Text.Json;

namespace cslox;

internal static class Extensions
{
    public static string ToJson(this object obj, bool prettyPrint = true) =>
        JsonSerializer.Serialize(obj, new JsonSerializerOptions {WriteIndented = prettyPrint});

    public static bool isDigit(this char c) => c is >= '0' and <= '9';

    public static bool isAlpha(this char c) =>
        c is >= 'a' and <= 'z' ||
        c is >= 'A' and <= 'Z' ||
        c is '_';

     public static bool isAlphaNumeric(this char c) => c.isAlpha() || c.isDigit();
}