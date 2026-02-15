using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly TotalCommander.Plugin.Infrastructure.Console.Console _console = new("kubectl");

    private readonly ILogger _logger = new TraceLogger("executor.log");

    public IEnumerable<Context> EnumerateContexts()
    {
        _logger.Log("Enumerating contexts...");

        var output = _console.Execute("config get-contexts -o name");
        if (output is null)
            yield break;

        var contexts = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var context in contexts)
            yield return new Context(context);
    }

    public IEnumerable<Namespace> EnumerateNamespaces(Context context)
    {
        _logger.Log($"Enumerating namespaces for context '{context.Name}'...");

        var output = _console.Execute($"--context {context.Name} get namespaces -o name");
        if (output is null)
            yield break;

        var namespaces = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(MapName);

        foreach (var ns in namespaces)
            yield return new Namespace(ns);
    }

    private static string MapName(string c) => c.Split('/')[1];

    public IEnumerable<Pod> EnumeratePods(Context context, Namespace ns)
    {
        _logger.Log($"Enumerating pods for context '{context.Name}' and namespace '{ns.Name}' ...");

        var command = $"--context {context.Name} --namespace {ns.Name} get pods --field-selector=status.phase=Running -o name";

        var output = _console.Execute(command);
        if (output is null)
            yield break;

        var pods = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(MapName);

        foreach (var pod in pods)
            yield return new Pod(pod);
    }

    public IEnumerable<Entry> EnumerateContainer(Path path)
    {
        _logger.Log($"Enumerating container for '{path}'...");

        if (path is not { Context: not null, Namespace: not null, Pod: not null, LocalPath: not null })
            return [];

        var command = $"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- ls -lF {path.LocalPath}";

        var output = _console.Execute(command);
        if (output is null)
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SkipWhile(c => c.StartsWith("total"))
            .Select(c => EntryFactory.Create(c));
    }

    public void CreateDirectory(Path path)
    {
        _logger.Log($"Creating directory '{path}'...");

        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- mkdir {path.LocalPath}");
    }

    public void DeleteDirectory(Path path)
    {
        _logger.Log($"Deleting directory '{path}'...");

        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- rm -r {path.LocalPath}");
    }

    public void CreateFile(Path path)
    {
        _logger.Log($"Creating file '{path}'...");

        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- touch {path.LocalPath}");
    }

    public void DeleteFile(Path path)
    {
        _logger.Log($"Deleting file '{path}'...");

        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- rm {path.LocalPath}");
    }

    public ExecuteResult Execute(Path path, string command)
    {
        _logger.Log($"Executing command '{command}'...");

        if (!path.IsFull)
            return ExecuteResult.Error;

        var arguments =
            $"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- sh -c \"cd {path.LocalPath} && {command}\"";
        _console.Execute(arguments);

        return ExecuteResult.Success;
    }

    public CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.Log($"Copying from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        if (source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        if (overwrite is false)
        {
            var exists = File.Exists(destination.LocalPath) || Directory.Exists(destination.LocalPath);
            if (direction is Direction.Out && exists)
                return CopyResult.Exists;

            if (direction is Direction.In && IsExists(destination))
                return CopyResult.Exists;
        }

        string? command = direction switch
        {
            Direction.Out when source.IsFull =>
                $"--context {source.Context.Name} --namespace {source.Namespace.Name} cp {source.Pod.Name}:{source.LocalPath} {destination.LocalPath}",
            Direction.In when destination.IsFull =>
                $"--context {destination.Context.Name} --namespace {destination.Namespace.Name} cp {source.LocalPath} {destination.Pod.Name}:{destination.LocalPath}",
            _ => null
        };

        if (command is null)
            return CopyResult.Error;

        var execute = _console.Execute(command);

        return execute is null ? CopyResult.Error : CopyResult.Success;
    }

    private bool IsExists(Path path)
    {
        if (!path.IsFull)
            return false;

        var command =
            $"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- sh -c \"[ -f {path.LocalPath} ] && echo 'exists'\"";

        var output = _console.Execute(command);

        return output is "exists";
    }

    public CopyResult Rename(Path source, Path destination, bool overwrite)
    {
        _logger.Log($"Renaming file '{source}' to '{destination}', overwrite: {overwrite}");

        if (!source.IsFull || !destination.IsFull)
            return CopyResult.Error;

        _logger.Log($"Renaming from '{source}' to '{destination}' Overwrite: {overwrite}");

        var command = $"--context {source.Context.Name} --namespace {source.Namespace.Name} exec {source.Pod.Name} -- mv ";

        if (overwrite)
            command += $"-f {source.LocalPath} {destination.LocalPath}";
        else
            command += $"-n {source.LocalPath} {destination.LocalPath}";

        var execute = _console.Execute(command);

        return execute is null ? CopyResult.Error : CopyResult.Success;
    }

    public CopyResult Move(Path source, Path destination, bool overwrite, Direction direction)
    {
        _logger.Log($"Moving from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        var result = Copy(source, destination, overwrite, direction);
        if (result is not CopyResult.Success)
            return result;

        if (direction is Direction.In && source.LocalPath is not null)
            OperatingSystem.Delete(source.LocalPath);

        if (direction is Direction.Out)
        {
            DeleteDirectory(source);
            DeleteFile(source);
        }

        return result;
    }
}
