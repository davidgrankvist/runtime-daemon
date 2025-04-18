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
            case "start":
                Console.WriteLine("Start is not implemented yet");
                break;
            case "stop":
                Console.WriteLine("Stop is not implemented yet");
                break;
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
Start the daemon:
runtime-daemon start

Stop the daemon:
runtime-daemon stop

Request daemon to execute an assembly:
runtime-daemon exec <assembly path> <optional args to assembly>
";

        Console.Write(helpText);
    }
}