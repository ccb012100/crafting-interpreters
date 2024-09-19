namespace tool;

internal static class AstGenerator {
    public static void DefineAst( string outputDir , string baseName , List<string> types ) {
        const string tab = "    ";
        string path = $"{outputDir}/{baseName}.cs";

        using StreamWriter writer = File.CreateText( path );

        BlankLine( );

        writer.WriteLine( "namespace cslox;" );
        BlankLine( );

        writer.WriteLine( $"internal abstract class {baseName}" );
        writer.WriteLine( "{" );

        DefineVisitor( );

        // The AST classes
        foreach ( string type in types ) {
            string[ ] typeSplit = type.Split( ':' );

            string className = typeSplit[0].Trim( );
            string fields = typeSplit[1].Trim( );

            BlankLine( );
            DefineType( className , fields );
        }

        BlankLine( );

        // The base accept() method
        writer.WriteLine( $"{tab}public abstract T accept<T>(IVisitor<T> visitor);" );

        writer.WriteLine( "}" );
        writer.Close( );

        return;

        void BlankLine( ) {
            writer.WriteLine( );
        }

        void DefineType( string className , string fieldList ) {
            /*
                public class ClassName : BaseName
                {
                    public ClassName(field1, field2, ... fieldN)
                    {
                        this.field1 = field1;
                        this.field1 = field2;
                        ...
                        this.fieldN = fieldN;
                    }
                }
             */
            writer.WriteLine( $"{tab}public class {className} : {baseName}" );
            writer.WriteLine( $"{tab}{{" );

            // ctor
            writer.WriteLine( $"{tab}{tab}public {className} ({fieldList})" );
            writer.WriteLine( $"{tab}{tab}{{" );

            string[ ] fields = fieldList.Split( ", " );

            // store params in fields
            foreach ( string field in fields ) {
                string name = field.Split( " " )[1];
                writer.WriteLine( $"{tab}{tab}{tab}this.{name} = {name};" );
            }

            writer.WriteLine( $"{tab}{tab}}}" );

            /*
                Visitor pattern

                ```
                public override T Accept(Visitor<T> visitor) => return Visitor.VisitClassnameBasename(this);
                ```
             */
            BlankLine( );

            writer.WriteLine(
                $"{tab}{tab}public override T Accept<T>(IVisitor<T> visitor) => visitor.visit{className}{baseName}(this);"
            );

            BlankLine( );

            /*
                fields

                 ```
                    public field1;
                    public field2;
                    ...
                    public fieldN
                 ```
            */
            foreach ( string field in fields ) {
                writer.WriteLine( $"{tab}{tab}public {field};" );
            }

            writer.WriteLine( $"{tab}}}" );
        }

        void DefineVisitor( ) {
            /*
                ```
                internal interface Visitor<T>
                {
                    R visitTypename1Basename(TypenameBasename);
                    R visitTypename2Basename(TypenameBasename);
                    ...
                    R visitTypenameNBasename(TypenameBasename);
                }
                ```
            */
            writer.WriteLine( $"{tab}internal interface IVisitor<out T>" );
            writer.WriteLine( $"{tab}{{" );

            foreach ( string typeName in types.Select( type => type.Split( ':' )[0].Trim( ) ) ) {
                writer.WriteLine( $"{tab}{tab}T visit{typeName}{baseName}({typeName} {baseName.ToLowerInvariant( )});" );
            }

            writer.WriteLine( $"{tab}}}" );
        }
    }
}
