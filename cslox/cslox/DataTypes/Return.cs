namespace cslox.DataTypes;

internal class Return( object value ) : Exception {
    public readonly object Value = value;
}
