namespace TotalCommander.Plugin.Shared.Infrastructure.Logger;

public static class LoggerExtensions
{
    public static void LogError(this ILogger logger, string message) => logger.Log(LogLevel.Error, message);
    public static void LogInfo(this ILogger logger, string message) => logger.Log(LogLevel.Information, message);
}
