namespace cslox;

public class Lox
{
    static bool _hadError;

    public static void RunFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        if (bytes is null)
        {
            Console.WriteLine($"File {path} is empty");
            Environment.Exit(66);
        }

        Run(bytes.ToString());

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

        throw new NotImplementedException();
    }

    public static void RunPrompt()
    {
        while (Console.ReadLine() is string line)
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