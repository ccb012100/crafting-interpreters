using cslox.DataTypes;

internal class RuntimeError( Token token, String message ) : Exception( message )
{
    private readonly Token _token = token;
}
