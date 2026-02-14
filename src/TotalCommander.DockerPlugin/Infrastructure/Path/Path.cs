using System;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.Infrastructure.Path;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;

namespace TotalCommander.DockerPlugin.Infrastructure.Path;

public sealed record Path(Container? Container, string? LocalPath)
{
    public override string ToString() => $"{Container?.Name ?? "<empty container>"} -> {LocalPath ?? "<empty path>"}";

    private static readonly ILogger s_logger = new TraceLogger("path.log");

    private const StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    public static Path Parse(string path)
    {
        var parts = path.Split(System.IO.Path.DirectorySeparatorChar, Options);

        Container? container = null;

        if (parts is not [var name, .. var local])
        {
            s_logger.Log($"Path.Parse: No container name found in path '{path}'");
            return new Path(container, path);
        }

        container = new Container(name);

        var localPath = "/";
        if (local is not [])
        {
            var index = path.IndexOf(System.IO.Path.DirectorySeparatorChar, 1);
            localPath = LinuxOs.PathAs(path[index..]);
        }

        s_logger.Log($"Path.Parse: Local path '{localPath}' and container '{container.Name}' parsed from path '{path}'");
        return new Path(container, localPath);
    }
}
