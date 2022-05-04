namespace cslox;

public class Lox
{
    private static bool _hadError;

    public static void RunFile(string path)
    {
        if (path is "")
        {
            Console.WriteLine($"File {path} is empty");
            Environment.Exit(66);
        }

        byte[] bytes = File.ReadAllBytes(path);

        string? source = bytes.ToString();

        if (source is not null)
        {
            Run(source);
        }
        else
        {
            Error(0, "Invalid source contents - null string.");
        }

        if (_hadError) Environment.Exit(65);
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
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