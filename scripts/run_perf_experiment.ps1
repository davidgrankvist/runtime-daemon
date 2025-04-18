# This performance test compares three different executions:
# 1. A standalone sample exe.
# 2. A C# client that requests the daemon to run the sample DLL. This client is built with AOT.
# 3. A C client that requests the daemon to run the sample DLL.
#
# Binaries are copied to testTemp in order to:
# 1. avoid daemon file locks when loading sample DLLs
# 2. limit effects of caching in general

# original build paths and names
$DAEMON_EXE=".\RuntimeDaemon.Daemon\bin\Release\net9.0\RuntimeDaemon.Daemon.exe"
$CS_CLIENT_PATH=".\RuntimeDaemon.Cli\bin\Release\net9.0\publish"
$CS_CLIENT_EXE="RuntimeDaemon.Cli.exe"
$SAMPLE_CLIENT_PATH=".\RuntimeDaemon.SampleClient\bin\Release\net9.0"
$SAMPLE_CLIENT_DLL="RuntimeDaemon.SampleClient.dll"
$SAMPLE_CLIENT_EXE="RuntimeDaemon.SampleClient.exe"
$C_CLIENT=".\bin\runtime-daemon.exe"

# Sample client executable
$SAMPLE_CLIENT_PATH_STANDALONE="testTemp\sample_standalone"
$STANDALONE_EXE="$SAMPLE_CLIENT_PATH_STANDALONE\$SAMPLE_CLIENT_EXE"

# C# client executable
$CS_CLIENT_PATH_T="testTemp\csharp_cli"
$CS_CLIENT="$CS_CLIENT_PATH_T\$CS_CLIENT_EXE"

# C# client - sample assembly to load
$SAMPLE_CLIENT_PATH_CS="testTemp\sample_csharp_cli"
$CS_DLL="$SAMPLE_CLIENT_PATH_CS\$SAMPLE_CLIENT_DLL"

# C client - sample assembly to load
$SAMPLE_CLIENT_PATH_C="testTemp\sample_c_cli"
$C_DLL="$SAMPLE_CLIENT_PATH_C\$SAMPLE_CLIENT_DLL"

# Daeomon STDOUT / STDERR
$DAEMON_LOG="testTemp\daemon_log.txt"
$DAEMON_LOG_ERR="testTemp\daemon_log_err.txt"

function PrepareArtifacts {
    # Build release artifacts
    .\scripts\build_client.bat
    dotnet build -c Release
    dotnet publish .\RuntimeDaemon.Cli\ -c Release

    # Set up testTemp copies

    Remove-Item -Force -Recurse testTemp

    mkdir testTemp
    mkdir $SAMPLE_CLIENT_PATH_STANDALONE
    mkdir $SAMPLE_CLIENT_PATH_CS
    mkdir $SAMPLE_CLIENT_PATH_C
    mkdir $CS_CLIENT_PATH_T

    Copy-Item "$SAMPLE_CLIENT_PATH\*" "$SAMPLE_CLIENT_PATH_STANDALONE\"
    Copy-Item "$SAMPLE_CLIENT_PATH\$SAMPLE_CLIENT_DLL" "$SAMPLE_CLIENT_PATH_CS\"
    Copy-Item "$SAMPLE_CLIENT_PATH\$SAMPLE_CLIENT_DLL" "$SAMPLE_CLIENT_PATH_C\"
    Copy-Item "$CS_CLIENT_PATH\*" "$CS_CLIENT_PATH_T\"
}

function RunExperiment {
    param (
        [string] $Message,
        [string] $Command,
        [string] $CommandArgs
    )

    echo ""
    echo "Experiment: $Message"
    echo "Command: $Command"
    echo "Arguments: $CommandArgs"
    echo ""

    Measure-Command {
        $process = Start-Process `
            -FilePath $Command `
            -ArgumentList $CommandArgs `
            -NoNewWindow `
            -PassThru
        $process.WaitForExit();
    }

    echo "==================================================="
}

echo ""
echo "%%%%%%%%%%%% PREPARING ARTIFACTS %%%%%%%%%%%%%%%%"
echo ""

PrepareArtifacts

echo ""
echo "%%%%%%%%%%%% RUNNING EXPERIMENTS %%%%%%%%%%%%%%%%"
echo ""

$daemonProcess = Start-Process `
    -FilePath $DAEMON_EXE `
    -RedirectStandardOut $DAEMON_LOG `
    -RedirectStandardError $DAEMON_LOG_ERR `
    -NoNewWindow `
    -PassThru

RunExperiment `
    -Message "Running sample client standalone" `
    -Command $STANDALONE_EXE `
    -CommandArgs "asdf"
RunExperiment `
    -Message "Running CS client" `
    -Command $CS_CLIENT `
    -CommandArgs "exec $CS_DLL asdf"
RunExperiment `
    -Message "Running C client" `
    -Command $C_CLIENT `
    -CommandArgs "exec $C_DLL asdf"

Stop-Process -Id $daemonProcess.Id -Force

echo "%%%%%%%%%%%% DAEMON LOGS %%%%%%%%%%%%%%%%"
echo "=================================================== STDOUT"

cat $DAEMON_LOG

echo "=================================================== STDERR"

cat $DAEMON_LOG_ERR

echo ""
echo "%%%%%%%%%%%% DONE %%%%%%%%%%%%%%%%"
