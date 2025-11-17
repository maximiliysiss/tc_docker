using System.Diagnostics;

namespace TotalCommander.Plugin.Shared.Infrastructure.Logger;

public sealed class TraceLogger(string path) : ILogger
{
    public void Log(string message) => Trace.WriteLine(message, path);
}
