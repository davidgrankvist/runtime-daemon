using System.Runtime.Loader;

namespace RuntimeDaemon.Application;

/// <summary>
/// Default implementation of AssemblyLoadContext. This is neccary because AssemblyLoadContext has no public constructor.
/// </summary>
internal class DaemonAssemblyLoadContext : AssemblyLoadContext
{
}
