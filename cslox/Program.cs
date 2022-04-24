// See https://aka.ms/new-console-template for more information

namespace cslox;

internal class Program
{
    static bool hadError;

    private static void Main(string[] args)
    {
        // using exit code convention from UNIX “sysexits.h” header https://www.freebsd.org/cgi/man.cgi?query=sysexits
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

        if (bytes is null)
        {
            Console.WriteLine($"File {path} is empty");
            Environment.Exit(66);
        }

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
