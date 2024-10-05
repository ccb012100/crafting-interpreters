using cslox.LoxCallables;

namespace cslox.DataTypes;

public class LoxTrait( Token name , Dictionary<string , LoxFunction> methods ) {
    public readonly Token Name = name;
    public readonly Dictionary<string , LoxFunction> Methods = methods;

    public override string ToString( ) {
        return Name.Lexeme;
    }
}
