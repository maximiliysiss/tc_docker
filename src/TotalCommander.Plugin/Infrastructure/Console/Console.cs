using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;

namespace TotalCommander.Plugin.Infrastructure.Console;

public sealed class Console(string command)
{
    private static readonly ILogger s_logger = new TraceLogger("console.log");
    private static readonly TimeSpan s_timeout = TimeSpan.FromMinutes(1);

    public string? Execute(string[] arguments, string? workingDirectory = null)
    {
        var argumentsOutput = string.Join(' ', arguments.DefaultIfEmpty("<no arguments>"));

        s_logger.LogInfo($"Begin execution of '{command} {argumentsOutput}'");

        using var process = new Process();

        process.EnableRaisingEvents = true;

        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        StringBuilder sb = new();

        process.OutputDataReceived += Output;
        process.ErrorDataReceived += Output;

        try
        {
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var isExited = process.WaitForExit(s_timeout);
            if (!isExited)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    s_logger.LogError($"Cannot kill '{command} {argumentsOutput}'");
                }
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            s_logger.LogError($"Cannot execute '{command}' because '{ex.Message}'");
        }
        finally
        {
            process.OutputDataReceived -= Output;
            process.ErrorDataReceived -= Output;
        }

        var output = sb.Length > 0 ? sb.ToString().Trim() : null;

        if (process.ExitCode != 0)
        {
            s_logger.LogError($"Execution of '{command} {argumentsOutput}' failed with output '{output ?? "<empty>"}'");
            return null;
        }

        s_logger.LogInfo($"Execution of '{command} {argumentsOutput}' is end with '{output ?? "<empty>"}'");

        return output ?? string.Empty;

        void Output(object _, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                sb.AppendLine(e.Data);
        }
    }
}
