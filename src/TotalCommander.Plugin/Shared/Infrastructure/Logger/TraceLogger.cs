using System.Diagnostics;

namespace TotalCommander.Plugin.Shared.Infrastructure.Logger;

public sealed class TraceLogger(string path) : ILogger
{
    public void Log(LogLevel logLevel, string message)
    {
        var level = logLevel.ToString().ToUpperInvariant();
        Trace.WriteLine($"[{level}] {message}", path);
    }
}
