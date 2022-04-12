// See https://aka.ms/new-console-template for more information

internal class Program
{
    static bool hadError;

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        switch (args.Length)
        {
            case 0:
                RunPrompt();
                return;
            case 1:
                RunFile(args[0]);
                return;
            default:
                Console.WriteLine("Usage: dotnet run [script]");
                Environment.Exit(64);
                break;
        }
    }

    private static void RunFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        Run(bytes.ToString());

        if (hadError) Environment.Exit(65);
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

    private static void RunPrompt()
    {
        while (Console.ReadLine() is string line)
        {
            if (string.IsNullOrEmpty(line)) break;

            Run(line);
        }
    }

    public class Scanner
    {
        public Scanner(string source)
        {
            Console.WriteLine($"source: {source}");
        }

        public List<Token> ScanTokens()
        {
            return new List<Token>();
        }
    }

    public class Token
    {
    }


    static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        TextWriter errorWriter = Console.Error;
        errorWriter.WriteLine($"[line {line}] Error{where}: {message}");
        hadError = true;
    }
}
