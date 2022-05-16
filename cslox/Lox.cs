namespace cslox;

public static class Lox
{
    private static bool _hadError;

    public static void RunFile(string path)
    {
        if (path is "")
        {
            Console.WriteLine("File parameter is empty; aborting...");
            Environment.Exit(66);
        }

        byte[] bytes = File.ReadAllBytes(path);
        // TODO: get encoding of path
        string source = System.Text.Encoding.Default.GetString(bytes);
        Run(source);

        if (_hadError) Environment.Exit(65);
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

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

    public static void RunPrompt()
    {
        while (Console.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line)) break;

            Run(line);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine($"[line {line}] Error{where}: {message}");
        _hadError = true;
    }
}