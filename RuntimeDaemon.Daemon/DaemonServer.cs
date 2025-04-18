using System.Diagnostics;
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

        using var reader = new StreamReader(server, Encoding.ASCII);
        var line = reader.ReadLine();
        Console.WriteLine($"Received: {line}");

        using var writer = new StreamWriter(server, Encoding.ASCII);
        if (line == null)
        {
            writer.WriteLine("Please provide an assembly to execute");
            writer.Flush();
        }
        else
        {
            var (path, args) = DeserializeArgs(line);
            var result = ExecuteAssemblyWithPerformanceLogging(path, args);

            var msg = result ? "OK" : "FAIL";
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

    private static bool ExecuteAssemblyWithWait(string path, string[] args)
    {
        var task = ExecuteAssembly(path, args);
        task.Wait();

        return task.Result;
    }

    private static bool ExecuteAssemblyWithPerformanceLogging(string path, string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();

        var result = ExecuteAssemblyWithWait(path, args);

        Console.WriteLine($"Executed assembly in {sw.ElapsedMilliseconds}ms");

        return result;
    }
}
