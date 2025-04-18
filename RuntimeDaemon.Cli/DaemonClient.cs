using System.IO.Pipes;
using System.Text;

namespace RuntimeDaemon.Cli;

internal class DaemonClient
{
    public static void RequestExecution(string assemblyPath, string[] assemblyArgs)
    {
        Console.WriteLine("Connecting..");
        var client = new NamedPipeClientStream(".", "runtime-daemon", PipeDirection.InOut);
        client.Connect();

        var writer = new StreamWriter(client, Encoding.UTF8);
        var payload = SerializeArgs(assemblyPath, assemblyArgs);
        writer.WriteLine(payload);
        writer.Flush();

        var reader = new StreamReader(client, Encoding.UTF8);
        var line = reader.ReadLine();

        Console.WriteLine($"Server responded with: {line}");

        writer.Dispose();
        reader.Dispose();
        client.Dispose();
    }

    private static string SerializeArgs(string assemblyPath, string[] assemblyArgs)
    {
        return assemblyPath + "|" + string.Join("|", assemblyArgs);
    }
}
