using System.Text;

namespace cslox.Extensions;

internal static class PrintExtensions {
    public static string ToPrintString<TKey, TValue>( this Dictionary<TKey , TValue> dict ) where TKey : notnull {
        StringBuilder sb = new( "dict { " );

        foreach ( (TKey key, TValue value) in dict ) {
            sb.Append( $"[ {key} ]: {{ {value} }} " );
        }

        return sb.Append( '}' ).ToString( );
    }

    public static string ToPrintString<T>( this ICollection<T> collection ) {
        return string.Join( " , " , collection );
    }
}
