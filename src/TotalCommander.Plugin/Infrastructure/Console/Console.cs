using System;
using System.Diagnostics;
using System.Text;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;

namespace TotalCommander.Plugin.Infrastructure.Console;

public sealed class Console(string command)
{
    private static readonly ILogger s_logger = new TraceLogger("console.log");
    private static readonly TimeSpan s_timeout = TimeSpan.FromMinutes(1);

    public string? Execute(string arguments)
    {
        s_logger.Log($"Begin execution of '{command} {arguments}'");

        using var process = new Process();

        process.EnableRaisingEvents = true;

        process.StartInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

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
                    // ignored
                }
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            s_logger.Log($"Cannot execute '{command}' because '{ex.Message}'");
        }
        finally
        {
            process.OutputDataReceived -= Output;
            process.ErrorDataReceived -= Output;
        }

        if (process.ExitCode != 0)
            return null;

        var output = sb.Length > 0 ? sb.ToString() : null;

        s_logger.Log($"Execution of '{command} {arguments}' is end with '{output ?? "<no output>"}'");

        return output;

        void Output(object _, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                sb.AppendLine(e.Data);
        }
    }
}
