internal static class AstGenerator
{
    public static void defineAst(string outputDir, string baseName, List<string> types)
    {
        string path = $"{outputDir}/{baseName}.cs";

        using StreamWriter writer = File.CreateText(path);
        writer.WriteLine("namespace cslox;");
        writer.WriteLine();
        writer.WriteLine($"internal abstract class {baseName}");
        writer.WriteLine("{");

        foreach (string type in types)
        {
            string[] typeSplit = type.Split(':');

            string className = typeSplit[0].Trim();
            string fields = typeSplit[1].Trim();

            defineType(className, fields);
        }

        writer.WriteLine("}");
        writer.Close();

        void defineType(string className, string fieldList)
        {
            const string tab = "    ";

            writer.WriteLine($"{tab}public class {className} : {baseName}");
            writer.WriteLine($"{tab}{{");

            // ctor
            writer.WriteLine($"{tab}{tab}public {className} ({fieldList})");
            writer.WriteLine($"{tab}{tab}{{");

            string[] fields = fieldList.Split(", ");

            // store params in fields
            foreach (string field in fields)
            {
                string name = field.Split(" ")[1];
                writer.WriteLine($"{tab}{tab}{tab}this.{name} = {name};");
            }

            writer.WriteLine($"{tab}{tab}}}");
            writer.WriteLine();

            // fields
            foreach (string field in fields)
            {
                writer.WriteLine($"{tab}{tab}public {field};");
            }

            writer.WriteLine($"{tab}}}");
        }
    }
}