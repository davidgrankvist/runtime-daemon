# runtime-daemon

A runtime daemon.

## About

Starting a .NET application and setting up the runtime comes with some overhead. The idea of this project is to reduce the overhead by
pre-initializing a process that acts as a runtime host that can execute DLLs on demand.

The intended usage is running processes locally. For example, you may want to speed up the execution of a CLI tool. This is not a general purpose tool
to use in a serverless execution kind of situation, as that would require a higher degree of isolation to be secure.

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

The CLI sends a request with the DLL path using a [named pipe](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication).
In the daemon, the request is received and the DLL is then loaded and executed in-process using [AssemblyLoadContext](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext). When the DLL has finished executing, the server uses the named pipe to report whether it succeeded. For more details, see [docs/appendix.md](./docs/appendix.md#named-pipe-protocol).

### Code overview

In this repo there are a few applications to be able to experiment with different approaches:
- RuntimeDaemon.Daemon is the daemon
- RuntimeDaemon.SampleClient is a C# sample application to execute using the daemon
- RuntimeDaemon.Cli is a C# client that connects to the daemon (built with AOT for less startup overhead)
- runtime-daemon-client-c is a more lightweight C client

### Findings

There is a simple benchmarking script in [scripts/run_perf_experiment.ps1](./scripts/run_perf_experiment.ps1) that compares running the sample application standalone versus using the clients and daemon.
Actual benchmarking would require more rigor, so take the numbers here with a grain of salt.

Some observations are:
- the daemon executes the DLL much quicker (20-100x faster) than a cold start standalone execution
- using named pipes adds overhead, which makes the C# client total execution time comparable to the slower standalone execution (sometimes even slower)
- the C client also suffers from named pipe overhead, but it is lightweight enough to outperform the other options (roughly 2-2.5x faster)

It would be interesting to compare this with a different Inter-Process Communication (IPC) method than named pipes.
