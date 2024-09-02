namespace tool;

internal static class AstGenerator
{
    public static void defineAst( string outputDir, string baseName, List<string> types )
    {
        const string tab = "    ";
        string path = $"{outputDir}/{baseName}.cs";

        using StreamWriter writer = File.CreateText( path );

        blankLine();

        writer.WriteLine( "namespace cslox;" );
        blankLine();

        writer.WriteLine( $"internal abstract class {baseName}" );
        writer.WriteLine( "{" );

        defineVisitor();

        // The AST classes
        foreach (string type in types)
        {
            string[] typeSplit = type.Split( ':' );

            string className = typeSplit[0].Trim();
            string fields = typeSplit[1].Trim();

            blankLine();
            defineType( className, fields );
        }

        blankLine();

        // The base accept() method
        writer.WriteLine( $"{tab}public abstract R accept<R>(Visitor<R> visitor);" );

        writer.WriteLine( "}" );
        writer.Close();

        void blankLine()
        {
            writer.WriteLine();
        }

        // ReSharper disable once InconsistentNaming
        void defineType( string className, string fieldList )
        {
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

            string[] fields = fieldList.Split( ", " );

            // store params in fields
            foreach (string field in fields)
            {
                string name = field.Split( " " )[1];
                writer.WriteLine( $"{tab}{tab}{tab}this.{name} = {name};" );
            }

            writer.WriteLine( $"{tab}{tab}}}" );

            /*
                Visitor pattern

                ```
                public override R Accept(Visitor<R> visitor) => return visitor.visitClassnameBasename(this);
                ```
             */
            blankLine();

            writer.WriteLine(
                $"{tab}{tab}public override R accept<R>(Visitor<R> visitor) => visitor.visit{className}{baseName}(this);"
            );

            blankLine();

            /*
                fields

                 ```
                    public field1;
                    public field2;
                    ...
                    public fieldN
                 ```
            */
            foreach (string field in fields)
            {
                writer.WriteLine( $"{tab}{tab}public {field};" );
            }

            writer.WriteLine( $"{tab}}}" );
        }

        // ReSharper disable once InconsistentNaming
        void defineVisitor()
        {
            /*
                ```
                internal interface Visitor<R>
                {
                    R visitTypename1Basename(TypenameBasename);
                    R visitTypename2Basename(TypenameBasename);
                    ...
                    R visitTypenameNBasename(TypenameBasename);
                }
                ```
            */
            writer.WriteLine( $"{tab}internal interface Visitor<out R>" );
            writer.WriteLine( $"{tab}{{" );

            foreach (string typeName in types.Select( type => type.Split( ':' )[0].Trim() ))
            {
                writer.WriteLine( $"{tab}{tab}R visit{typeName}{baseName}({typeName} {baseName.ToLowerInvariant()});" );
            }

            writer.WriteLine( $"{tab}}}" );
        }
    }
}
