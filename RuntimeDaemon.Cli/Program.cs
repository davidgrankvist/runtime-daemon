using RuntimeDaemon.Cli;

internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        var cmd = args[0];
        switch (cmd)
        {
            case "exec":
                if (args.Length >= 2)
                {
                    var assemblyPath = Path.GetFullPath(args[1]);
                    var assemblyArgs = args.Length >= 3 ? args[2..] : [];
                    DaemonClient.RequestExecution(assemblyPath, assemblyArgs);
                }
                else
                {
                    PrintHelp();
                }
                break;
            default:
                PrintHelp();
                break;
        }
    }

    private static void PrintHelp()
    {
        const string helpText = @"
Launch the daemon:
runtime-daemon-d

Request daemon to execute an assembly:
runtime-daemon exec <assembly path> <optional args to assembly>
";

        Console.Write(helpText);
    }
}