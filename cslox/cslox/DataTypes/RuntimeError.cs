using cslox.DataTypes;

internal class RuntimeError( Token token, String message ) : Exception( message )
{
    public readonly Token Token = token;
}
