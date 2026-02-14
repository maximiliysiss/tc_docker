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

    public string? Execute(string arguments, params string[] commands)
    {
        var commandsOutput = string.Join(", ", commands.DefaultIfEmpty("<no commands>"));
        s_logger.Log($"Begin execution of '{command} {arguments} with inner commands '{commandsOutput}'");

        using var process = new Process();

        process.EnableRaisingEvents = true;

        process.StartInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = commands is not [],
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

            foreach (var innerCommand in commands)
            {
                process.StandardInput.Write($"{innerCommand}\n");
                process.StandardInput.Flush();
            }

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

        var output = sb.Length > 0 ? sb.ToString() : null;

        if (process.ExitCode != 0)
        {
            s_logger.Log($"Execution of '{command} {arguments}' failed with output '{output ?? "<empty>"}'");
            return null;
        }

        s_logger.Log($"Execution of '{command} {arguments}' is end with '{output ?? "<empty>"}'");

        return output ?? string.Empty;

        void Output(object _, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                sb.AppendLine(e.Data);
        }
    }
}
