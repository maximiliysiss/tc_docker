using System.Diagnostics;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;

namespace TotalCommander.DockerPlugin.Infrastructure.Console;

public static class Console
{
    private static readonly ILogger s_logger = new TraceLogger("executor.log");

    public static string? Execute(string arguments)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        process.Start();

        process.WaitForExit();

        var output = process.ExitCode is 0
            ? process.StandardOutput.ReadToEnd()
            : null;

        s_logger.Log($"Execution of 'docker {arguments}' is end with '{output ?? "nothing"}'");

        return output;
    }
}
