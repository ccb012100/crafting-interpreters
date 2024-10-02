namespace cslox.DataTypes;

public class LoxInstance( LoxClass klass ) {
    private readonly LoxClass _klass = klass;

    public override string ToString( ) {
        return $"{_klass.Name} instance";
    }
}
