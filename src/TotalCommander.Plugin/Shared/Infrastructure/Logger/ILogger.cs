namespace TotalCommander.Plugin.Shared.Infrastructure.Logger;

public interface ILogger
{
    void Log(LogLevel logLevel, string message);
}
