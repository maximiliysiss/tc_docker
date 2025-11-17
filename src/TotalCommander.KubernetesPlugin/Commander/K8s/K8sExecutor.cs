using System;
using System.Collections.Generic;
using System.Linq;
using TotalCommander.KubernetesPlugin.Commander.Models;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Elements;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace TotalCommander.KubernetesPlugin.Commander.K8s;

public sealed class K8sExecutor : IK8sExecutor
{
    private readonly TotalCommander.Plugin.Infrastructure.Console.Console _console = new("kubectl");

    private readonly ILogger _logger = new TraceLogger("executor.log");

    public IEnumerable<Context> EnumerateContexts()
    {
        var output = _console.Execute("config get-contexts -o name");
        if (output is null)
            yield break;

        var contexts = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var context in contexts)
            yield return new Context(context);
    }

    public IEnumerable<Namespace> EnumerateNamespaces(Context context)
    {
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
        if (path is not { Context: not null, Namespace: not null, Pod: not null, LocalPath: not null })
            return [];

        var command = $"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- ls -lF {path.LocalPath}";

        var output = _console.Execute(command);
        if (output is null)
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SkipWhile(c => c.StartsWith("total"))
            .Select(EntryFactory.Create);
    }

    public void CreateDirectory(Path path)
    {
        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- mkdir {path.LocalPath}");
    }

    public void DeleteDirectory(Path path)
    {
        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- rm -r {path.LocalPath}");
    }

    public void CreateFile(Path path)
    {
        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- touch {path.LocalPath}");
    }

    public void DeleteFile(Path path)
    {
        if (!path.IsFull)
            return;

        _console.Execute($"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- rm {path.LocalPath}");
    }

    public ExecuteResult Execute(Path path, string command)
    {
        if (!path.IsFull)
            return ExecuteResult.Error;

        var arguments =
            $"--context {path.Context.Name} --namespace {path.Namespace.Name} exec {path.Pod.Name} -- sh -c \"cd {path.LocalPath} && {command}\"";
        _console.Execute(arguments);

        return ExecuteResult.Success;
    }

    public CopyResult Copy(Path source, Path destination, bool overwrite)
    {
        if (source.LocalPath is null || destination.LocalPath is null)
            return CopyResult.Error;

        var direction = (source, destination) switch
        {
            ({ IsFull: true }, { IsFull: false }) => Direction.Out,
            ({ IsFull: false }, { IsFull: true }) => Direction.In,
            _ => Direction.Inter
        };

        _logger.Log($"Copying from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        if (direction is Direction.Inter && source.Segment is not null)
        {
            var tmpPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString(), source.Segment);
            var tmpDestination = new Path(Context: null, Namespace: null, Pod: null, LocalPath: tmpPath);

            Copy(source, tmpDestination, true);
            return Copy(tmpDestination, destination, overwrite);
        }

        if (direction is Direction.Out && IsExists(destination) && overwrite is false)
            return CopyResult.Exists;

        if (direction is Direction.In && IsExists(destination) && overwrite is false)
            return CopyResult.Exists;

        var command = direction switch
        {
            Direction.Out when source.IsFull =>
                $"--context {source.Context.Name} --namespace {source.Namespace.Name} cp {source.Pod.Name}:{source.LocalPath} {destination.LocalPath}",
            Direction.In when destination.IsFull =>
                $"--context {destination.Context.Name} --namespace {destination.Namespace.Name} cp {source.LocalPath} {destination.Pod.Name}:{destination.LocalPath}",
            _ => throw new InvalidOperationException("Invalid copy direction.")
        };

        _console.Execute(command);

        return CopyResult.Success;
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
        if (!source.IsFull || !destination.IsFull)
            return CopyResult.Error;

        _logger.Log($"Renaming from '{source}' to '{destination}' Overwrite: {overwrite}");

        var command = $"--context {source.Context.Name} --namespace {source.Namespace.Name} exec {source.Pod.Name} -- mv ";

        if (overwrite)
            command += $"-f {source.LocalPath} {destination.LocalPath}";
        else
            command += $"-n {source.LocalPath} {destination.LocalPath}";

        _console.Execute(command);

        return CopyResult.Success;
    }

    public CopyResult Move(Path source, Path destination, bool overwrite)
    {
        var result = Copy(source, destination, overwrite);
        if (result is not CopyResult.Success)
            return result;

        var direction = (source, destination) switch
        {
            ({ IsFull: true }, { IsFull: false }) => Direction.Out,
            ({ IsFull: false }, { IsFull: true }) => Direction.In,
            _ => Direction.Inter
        };

        _logger.Log($"Moving from '{source}' to '{destination}' Direction: {direction} Overwrite: {overwrite}");

        if (direction is Direction.In)
        {
            if (File.Exists(source.LocalPath))
                File.Delete(source.LocalPath);

            if (Directory.Exists(source.LocalPath))
                Directory.Delete(source.LocalPath);

            return result;
        }

        DeleteDirectory(source);
        DeleteFile(source);

        return result;
    }
}
