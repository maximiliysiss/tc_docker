using System;
using System.Collections.Generic;
using System.Linq;
using TotalCommander.DockerPlugin.Infrastructure.Extensions;
using TotalCommander.DockerPlugin.Infrastructure.Path;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Elements;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;
using Console = TotalCommander.Plugin.Infrastructure.Console.Console;
using Directory = System.IO.Directory;
using File = System.IO.File;
using OperatingSystem = TotalCommander.Plugin.Infrastructure.Path.OperatingSystem;

namespace TotalCommander.DockerPlugin.Commander.Docker;

public sealed class DockerExecutor : IDockerExecutor
{
    private const StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    private readonly Console _console = new("docker");

    private readonly ILogger _logger = new TraceLogger("plugin.log");

    public IEnumerable<Container> EnumerateContainers()
    {
        _logger.Log("EnumerateContainers: Start enumerating containers");

        var output = _console.Execute("ps --format \"{{.Names}}\"");
        if (output is null)
            return [];

        return output
            .Split('\n', Options)
            .Select(c => new Container(c));
    }

    public IEnumerable<Entry> EnumerateContainer(Path path)
    {
        _logger.Log($"EnumerateContainer: Start enumerating container '{path}'");

        if (path.Container is null)
            return [];

        var output = _console.Execute($"exec {path.Container.Name} stat -c \"%n\\0%A\\0%s\" {path.LocalPath}");
        if (output is null)
            return [];

        return output
            .Split('\n', Options)
            .SkipWhile(c => c.StartsWith("total"))
            .Select(EntryFactory.Create);
    }

    public void CreateFile(Path path)
    {
        _logger.Log($"CreateFile: Start creating file in container '{path}'");

        if (path.Container is null)
            return;

        _console.Execute($"exec {path.Container.Name} touch {path.LocalPath}");
    }

    public void DeleteFile(Path path)
    {
        _logger.Log($"DeleteFile: Start deleting file in container '{path}'");

        if (path.Container is null)
            return;

        _console.Execute($"exec {path.Container.Name} rm {path.LocalPath}");
    }

    public void DeleteDirectory(Path path)
    {
        _logger.Log($"DeleteDirectory: Start deleting directory in container '{path}'");

        if (path.Container is null)
            return;

        _console.Execute($"exec {path.Container.Name} rm -r {path.LocalPath}");
    }

    public void CreateDirectory(Path path)
    {
        _logger.Log($"CreateDirectory: Start creating directory in container '{path}'");

        if (path.Container is null)
            return;

        _console.Execute($"exec {path.Container.Name} mkdir {path.LocalPath}");
    }

    public ExecuteResult Execute(Path path, string command)
    {
        _logger.Log($"Execute: Start executing command in container '{path}': {command}");

        if (path.Container is null)
            return ExecuteResult.Error;

        string[] commands =
        [
            $"cd {path.LocalPath}",
            command,
            "exit"
        ];

        _console.Execute($"exec -i {path.Container.Name} bash", commands);

        return ExecuteResult.Success;
    }

    public CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.Log($"Copy: Start copying from '{source}' to '{destination}', overwrite: {overwrite}, direction: {direction}");

        if (source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        if (overwrite is false)
        {
            if (direction == Direction.In && IsExists(destination))
                return CopyResult.Exists;

            var exists = File.Exists(destination.LocalPath) || Directory.Exists(destination.LocalPath);
            if (direction == Direction.Out && exists)
                return CopyResult.Exists;
        }

        var sourcePath = $"{source.Container?.Name.Append(":")}{source.LocalPath}";
        var destinationPath = $"{destination.Container?.Name.Append(":")}{destination.LocalPath}";

        _console.Execute($"cp {sourcePath} {destinationPath}");

        return CopyResult.Success;
    }

    public CopyResult Move(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.Log($"Move: Start moving from '{source}' to '{destination}', overwrite: {overwrite}, direction: {direction}");

        var copyResult = Copy(source, destination, overwrite, direction);
        if (copyResult is not CopyResult.Success)
            return copyResult;

        if (direction == Direction.In && source.LocalPath is not null)
            OperatingSystem.Delete(source.LocalPath);

        if (direction == Direction.Out)
        {
            DeleteFile(source);
            DeleteDirectory(source);
        }

        return CopyResult.Success;
    }

    public CopyResult Rename(Path source, Path destination, bool overwrite)
    {
        _logger.Log($"Rename: Start renaming from '{source}' to '{destination}', overwrite: {overwrite}");

        if (source.Container is null || destination.Container is null)
            return CopyResult.Error;

        if (!overwrite && IsExists(destination))
            return CopyResult.Exists;

        _console.Execute($"exec {source.Container.Name} mv {source.LocalPath} {destination.LocalPath}");

        return CopyResult.Success;
    }

    private bool IsExists(Path path)
    {
        if (path.Container is null)
            return false;

        const string expectedOutput = "exists";

        string[] commands = [

        ];

        var execute = _console.Execute($"exec -i {path.Container.Name} bash");

        _logger.Log($"IsExists: Checking existence of '{path}', result: '{execute}'");

        return execute?.Trim() is expectedOutput;
    }
}
