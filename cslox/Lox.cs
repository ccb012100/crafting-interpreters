namespace cslox;

public static class Lox
{
    private static bool _hadError;

    public static void runFile(string path)
    {
        if (path is "")
        {
            Console.WriteLine("File parameter is empty; aborting...");
            Environment.Exit(66);
        }

        byte[] bytes = File.ReadAllBytes(path);
        // TODO: get encoding of path
        string source = System.Text.Encoding.Default.GetString(bytes);
        run(source);

        if (_hadError) Environment.Exit(65);
    }

    private static void run(string source)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.scanTokens();

        Console.WriteLine(
            "\n**************************************************\n"
            + "                 BEGIN Source\n"
            + "**************************************************");
        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
        Console.WriteLine(
            "\n**************************************************\n"
            + "                 END   Source\n"
            + "**************************************************");
    }

    public static void runPrompt()
    {
        while (Console.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line)) break;

            run(line);
        }
    }

    public static void error(int line, string message)
    {
        report(line, "", message);
    }

    private static void report(int line, string where, string message)
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine($"[line {line}] Error{where}: {message}");
        _hadError = true;
    }
}
