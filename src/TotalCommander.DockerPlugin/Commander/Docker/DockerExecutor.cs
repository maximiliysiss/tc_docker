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
        _logger.LogInfo("EnumerateContainers: Start enumerating containers");

        string[] commands =
        [
            "ps",
            "--format",
            "{{.Names}}"
        ];

        var output = _console.Execute(commands);
        if (output is null)
            return [];

        return output
            .Split('\n', Options)
            .Select(c => new Container(c));
    }

    public IEnumerable<Entry> EnumerateContainer(Path path)
    {
        _logger.LogInfo($"EnumerateContainer: Start enumerating container '{path}'");

        if (path.Container is null || path.LocalPath is null)
            return [];

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "sh",
            "-c",
            $"find \"{path.LocalPath}\" -maxdepth 1 -mindepth 1 -exec stat -c \"%n\u001f%A\u001f%s\" -- {{}} +"
        ];

        var output = _console.Execute(commands);
        if (output is null)
            return [];

        var offset = path.LocalPath.Length + Convert.ToInt32(!path.IsRoot);

        return output
            .Split('\n', Options)
            .Select(e => EntryFactory.Create(e[offset..]));
    }

    public void CreateFile(Path path)
    {
        _logger.LogInfo($"CreateFile: Start creating file in container '{path}'");

        if (path.Container is null || path.LocalPath is null)
            return;

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "touch",
            path.LocalPath,
        ];

        _console.Execute(commands);
    }

    public void DeleteFile(Path path)
    {
        _logger.LogInfo($"DeleteFile: Start deleting file in container '{path}'");

        if (path.Container is null || path.LocalPath is null)
            return;

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "rm",
            path.LocalPath,
        ];

        _console.Execute(commands);
    }

    public void DeleteDirectory(Path path)
    {
        _logger.LogInfo($"DeleteDirectory: Start deleting directory in container '{path}'");

        if (path.Container is null || path.LocalPath is null)
            return;

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "rm",
            "-r",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public void CreateDirectory(Path path)
    {
        _logger.LogInfo($"CreateDirectory: Start creating directory in container '{path}'");

        if (path.Container is null || path.LocalPath is null)
            return;

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "mkdir",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public ExecuteResult Execute(Path path, string command)
    {
        _logger.LogInfo($"Execute: Start executing command in container '{path}': {command}");

        if (path.Container is null || path.LocalPath is null)
            return ExecuteResult.Error;

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "sh",
            "-c",
            $"cd \"{path.LocalPath}\" && {command}",
        ];

        _console.Execute(commands);

        return ExecuteResult.Success;
    }

    public CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.LogInfo($"Copy: Start copying from '{source}' to '{destination}', overwrite: {overwrite}, direction: {direction}");

        if (source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        if (overwrite is false)
        {
            if (direction is Direction.In or Direction.Inter && IsExists(destination))
                return CopyResult.Exists;

            var exists = File.Exists(destination.LocalPath) || Directory.Exists(destination.LocalPath);
            if (direction == Direction.Out && exists)
                return CopyResult.Exists;
        }

        if (direction is Direction.Inter)
        {
            var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            var interPath = new Path(Container: null, localPath);

            var copyResult = Copy(source, interPath, overwrite: true, Direction.Out);
            if (copyResult is not CopyResult.Success)
                return copyResult;

            copyResult = Copy(interPath, destination, overwrite, Direction.In);

            return copyResult;
        }

        var sourcePath = $"{source.Container?.Name.Append(":")}{source.LocalPath}";
        var destinationPath = $"{destination.Container?.Name.Append(":")}{destination.LocalPath}";

        string[] commands =
        [
            "cp",
            sourcePath,
            destinationPath
        ];

        _console.Execute(commands);

        return CopyResult.Success;
    }

    public CopyResult Move(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.LogInfo($"Move: Start moving from '{source}' to '{destination}', overwrite: {overwrite}, direction: {direction}");

        var copyResult = Copy(source, destination, overwrite, direction);
        if (copyResult is not CopyResult.Success)
            return copyResult;

        if (direction == Direction.In && source.LocalPath is not null)
            OperatingSystem.Delete(source.LocalPath);

        if (direction is Direction.Out or Direction.Inter)
        {
            DeleteFile(source);
            DeleteDirectory(source);
        }

        return CopyResult.Success;
    }

    public CopyResult Rename(Path source, Path destination, bool overwrite)
    {
        _logger.LogInfo($"Rename: Start renaming from '{source}' to '{destination}', overwrite: {overwrite}");

        if (source.Container is null || destination.Container is null || source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        if (!overwrite && IsExists(destination))
            return CopyResult.Exists;

        string[] commands =
        [
            "exec",
            source.Container.Name,
            "mv",
            overwrite ? "-f" : "-n",
            source.LocalPath,
            destination.LocalPath,
        ];

        _console.Execute(commands);

        return CopyResult.Success;
    }

    private bool IsExists(Path path)
    {
        if (path.Container is null || path.LocalPath is null)
            return false;

        const string expectedOutput = "exists";

        string[] commands =
        [
            "exec",
            path.Container.Name,
            "sh",
            "-c",
            "[ -e \"$1\" ] && echo exists",
            "--",
            path.LocalPath
        ];

        var execute = _console.Execute(commands);

        _logger.LogInfo($"IsExists: Checking existence of '{path}', result: '{execute}'");

        return execute?.Trim() is expectedOutput;
    }
}
