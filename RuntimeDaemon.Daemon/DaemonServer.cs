using System.IO.Pipes;
using System.Text;

using RuntimeDaemon.Application;

namespace RuntimeDaemon.Daemon;

internal static class DaemonServer
{
    public static void Serve()
    {
        while (true)
        {
            HandleRequest();
        }
    }

    private static void HandleRequest()
    {
        Console.WriteLine("Listening..");
        using var server = new NamedPipeServerStream("runtime-daemon", PipeDirection.InOut);
        server.WaitForConnection();

        using var reader = new StreamReader(server, Encoding.UTF8);
        var line = reader.ReadLine();
        Console.WriteLine($"Received: {line}");

        using var writer = new StreamWriter(server, Encoding.UTF8);
        if (line == null)
        {
            writer.WriteLine("Please provide an assembly to execute");
            writer.Flush();
        }
        else
        {
            var (path, args) = DeserializeArgs(line);
            var task = ExecuteAssembly(path, args);
            task.Wait();

            var msg = task.Result ? "Executed assembly" : "Unable to execute assembly";
            writer.WriteLine(msg);
            writer.Flush();
        }
    }

    private static (string Path, string[] Args) DeserializeArgs(string line)
    {
        var pathAndArgs = line.Split("|");
        var path = pathAndArgs[0];
        var args = pathAndArgs.Length > 1 ? pathAndArgs[1..] : [];

        return (path, args);
    }

    private static async Task<bool> ExecuteAssembly(string path, string[] args)
    {
        var ctx = new DaemonAssemblyLoadContext();
        var asm = ctx.LoadFromAssemblyPath(path);
        var entryPoint = asm.EntryPoint;
        if (entryPoint == null)
        {
            return false;
        }

        var result = entryPoint.Invoke(null, [args]);

        if (result is Task resultTask)
        {
            await resultTask;
        }

        return true;
    }
}
