using System.Text.Json;

namespace cslox.Extensions;

internal static class ObjectExtensions {
    private static readonly JsonSerializerOptions s_sOptions = new( ) { WriteIndented = false };
    private static readonly JsonSerializerOptions s_sPrettyPrintOptions = new( ) { WriteIndented = true };

    public static string ToJson( this object obj , bool prettyPrint = true ) {
        return JsonSerializer.Serialize( obj , prettyPrint ? s_sPrettyPrintOptions : s_sOptions );
    }
}
