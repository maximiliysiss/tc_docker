using System;
using System.Collections.Generic;
using System.Linq;
using TotalCommander.KubernetesPlugin.Commander.Extensions;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Elements;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;
using Directory = System.IO.Directory;
using File = System.IO.File;
using OperatingSystem = TotalCommander.Plugin.Infrastructure.Path.OperatingSystem;

namespace TotalCommander.KubernetesPlugin.Commander.K8s;

public sealed class K8sExecutor : IK8sExecutor
{
    private const StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    private readonly TotalCommander.Plugin.Infrastructure.Console.Console _console = new("kubectl");

    private readonly ILogger _logger = new TraceLogger("executor.log");

    public IEnumerable<Context> EnumerateContexts()
    {
        _logger.LogInfo("Enumerating contexts...");

        string[] commands =
        [
            "config",
            "get-contexts",
            "-o",
            "name"
        ];

        var output = _console.Execute(commands);
        if (output is null)
            yield break;

        var contexts = output.Split('\n', Options);

        foreach (var context in contexts)
            yield return new Context(context);
    }

    public IEnumerable<Namespace> EnumerateNamespaces(Context context)
    {
        _logger.LogInfo($"Enumerating namespaces for context '{context.Name}'...");

        string[] commands =
        [
            "--context",
            context.Name,
            "get",
            "namespaces",
            "-o",
            "name"
        ];

        var output = _console.Execute(commands);
        if (output is null)
            yield break;

        var namespaces = output
            .Split('\n', Options)
            .Select(MapName);

        foreach (var ns in namespaces)
            yield return new Namespace(ns);
    }

    private static string MapName(string c) => c.Split('/')[1];

    public IEnumerable<Pod> EnumeratePods(Context context, Namespace ns)
    {
        _logger.LogInfo($"Enumerating pods for context '{context.Name}' and namespace '{ns.Name}' ...");

        string[] commands =
        [
            "--context",
            context.Name,
            "--namespace",
            ns.Name,
            "get",
            "pods",
            "--field-selector",
            "status.phase=Running",
            "-o",
            "name"
        ];

        var output = _console.Execute(commands);
        if (output is null)
            yield break;

        var pods = output
            .Split('\n', Options)
            .Select(MapName);

        foreach (var pod in pods)
            yield return new Pod(pod);
    }

    public IEnumerable<Entry> EnumerateContainer(Path path)
    {
        _logger.LogInfo($"Enumerating container for '{path}'...");

        if (path is not { Context: not null, Namespace: not null, Pod: not null, LocalPath: not null })
            return [];

        string[] commands =
        [
            .. path.Execute(),
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

    public void CreateDirectory(Path path)
    {
        _logger.LogInfo($"Creating directory '{path}'...");

        if (!path.IsFull)
            return;

        string[] commands =
        [
            .. path.Execute(),
            "mkdir",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public void DeleteDirectory(Path path)
    {
        _logger.LogInfo($"Deleting directory '{path}'...");

        if (!path.IsFull)
            return;

        string[] commands =
        [
            .. path.Execute(),
            "rm",
            "-r",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public void CreateFile(Path path)
    {
        _logger.LogInfo($"Creating file '{path}'...");

        if (!path.IsFull)
            return;

        string[] commands =
        [
            .. path.Execute(),
            "touch",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public void DeleteFile(Path path)
    {
        _logger.LogInfo($"Deleting file '{path}'...");

        if (!path.IsFull)
            return;

        string[] commands =
        [
            .. path.Execute(),
            "rm",
            path.LocalPath
        ];

        _console.Execute(commands);
    }

    public ExecuteResult Execute(Path path, string command)
    {
        _logger.LogInfo($"Executing command '{command}'...");

        if (!path.IsFull)
            return ExecuteResult.Error;

        string[] commands =
        [
            .. path.Execute(),
            "sh",
            "-c",
            $"cd \"{path.LocalPath}\" && {command}",
        ];

        _console.Execute(commands);

        return ExecuteResult.Success;
    }

    public CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction, string? workingDirectory)
    {
        _logger.LogInfo($"Copying from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        if (source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        if (overwrite is false)
        {
            var exists = File.Exists(destination.LocalPath) || Directory.Exists(destination.LocalPath);
            if (direction is Direction.Out && exists)
                return CopyResult.Exists;

            if (direction is Direction.In or Direction.Inter && IsExists(destination))
                return CopyResult.Exists;
        }

        if (direction is Direction.Inter)
        {
            var currentDirectory = System.IO.Path.GetTempPath();
            var interPath = new Path(null, null, null, System.IO.Path.GetRandomFileName());

            var copyResult = Copy(source, interPath, overwrite: true, Direction.Out, currentDirectory);
            if (copyResult is not CopyResult.Success)
            {
                _logger.LogError($"Failed to copy '{source}' to '{interPath}'");
                return copyResult;
            }

            copyResult = Copy(interPath, destination, overwrite, Direction.In, currentDirectory);

            return copyResult;
        }

        string[]? commands = direction switch
        {
            Direction.Out when source.IsFull =>
            [
                "--context",
                source.Context.Name,
                "--namespace",
                source.Namespace.Name,
                "cp",
                $"{source.Pod.Name}:{source.LocalPath}",
                destination.LocalPath,
            ],
            Direction.In when destination.IsFull =>
            [
                "--context",
                destination.Context.Name,
                "--namespace",
                destination.Namespace.Name,
                "cp",
                source.LocalPath,
                $"{destination.Pod.Name}:{destination.LocalPath}",
            ],
            _ => null,
        };

        if (commands is null)
        {
            _logger.LogError($"Failed copy '{source}' to '{destination}'");
            return CopyResult.Error;
        }

        var output = _console.Execute(commands, workingDirectory);

        return output is null ? CopyResult.Error : CopyResult.Success;
    }

    private bool IsExists(Path path)
    {
        if (!path.IsFull)
            return false;

        string[] commands =
        [
            .. path.Execute(),
            "sh",
            "-c",
            $"[ -e \"{path.LocalPath}\" ] && echo exists"
        ];

        var output = _console.Execute(commands);

        return output is "exists";
    }

    public CopyResult Rename(Path source, Path destination, bool overwrite)
    {
        _logger.LogInfo($"Renaming file '{source}' to '{destination}', overwrite: {overwrite}");

        if (!source.IsFull || !destination.IsFull)
            return CopyResult.Error;

        string[] commands =
        [
            .. source.Execute(),
            "mv",
            overwrite ? "-f" : "-n",
            source.LocalPath,
            destination.LocalPath,
        ];

        var execute = _console.Execute(commands);

        return execute is null ? CopyResult.Error : CopyResult.Success;
    }

    public CopyResult Move(Path source, Path destination, bool overwrite, Direction direction, string? workingDirectory)
    {
        _logger.LogInfo($"Moving from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        var result = Copy(source, destination, overwrite, direction, workingDirectory);
        if (result is not CopyResult.Success)
            return result;

        if (direction is Direction.In && source.LocalPath is not null)
            OperatingSystem.Delete(System.IO.Path.Combine(workingDirectory ?? string.Empty, source.LocalPath));

        if (direction is Direction.Out or Direction.Inter)
        {
            DeleteDirectory(source);
            DeleteFile(source);
        }

        return result;
    }
}
