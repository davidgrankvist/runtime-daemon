# runtime-daemon

A runtime daemon.

## About

Starting a .NET application and setting up the runtime comes with some overhead. The idea of this project is to reduce the overhead by
pre-initializing a process that acts as a runtime host that can execute DLLs on demand.

### Usage

The daemon is started ahead of time.
```
runtime-daemon-d
```

The CLI sends requests to execute a given DLL.
```
runtime-daemon exec <assembly path> <optional args to assembly>
```

### How does it work?

The processes communicate over a [named pipe](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication).
In the daemon, DLL execution is hosted in-process using [AssemblyLoadContext](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext).
