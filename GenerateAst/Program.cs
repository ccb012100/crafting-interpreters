// ReSharper disable once ClassNeverInstantiated.Global

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length is not 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];

        // NOTE: using `oper` because `operator` is a keyword in C#
        AstGenerator.defineAst(outputDir, "Expr",
            new List<string>
            {
                "Binary   : Expr left, Token oper, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token oper, Expr right"
            });
    }
}