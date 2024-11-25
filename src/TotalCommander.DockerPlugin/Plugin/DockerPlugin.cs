using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TotalCommander.DockerPlugin.Commander.Docker;
using TotalCommander.DockerPlugin.Plugin.Converter;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using File = System.IO.File;
using Path = TotalCommander.DockerPlugin.Infrastructure.Path.Path;

namespace TotalCommander.DockerPlugin.Plugin;

public sealed class DockerPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub, IMoveHub
{
    private readonly IDockerExecutor _dockerExecutor = new DockerDockerExecutor();

    private Dictionary<string, Container> _containers = new();

    public string Name => "docker";

    public IEnumerable<Entry> EnumerateEntries(string path)
    {
        return path switch
        {
            _ when Path.IsRoot(path) => _dockerExecutor.EnumerateContainers(),
            _ => EnumerateContainer(path)
        };
    }

    private IEnumerable<Entry> EnumerateContainer(string path)
    {
        var container = GetContainer(path);
        if (container is null)
            return [];

        path = Path.AsLinux(Path.GetRootlessPath(path));

        return _dockerExecutor.EnumerateContainer(container, path);
    }

    private Container? GetContainer(string path)
    {
        var root = Path.GetRootDirectory(path);

        if (_containers.TryGetValue(root, out var container))
            return container;

        _containers = _dockerExecutor
            .EnumerateContainers()
            .ToDictionary(c => c.Name);

        return _containers.GetValueOrDefault(root);
    }

    void IFileHub.Create(string path) => throw new System.NotImplementedException();

    void IFileHub.Delete(string path)
    {
        var container = GetContainer(path);
        if (container is null)
            return;

        path = Path.AsLinux(Path.GetRootlessPath(path));

        _dockerExecutor.DeleteFile(container, path);
    }

    void IFileHub.Open(string path)
    {
        var container = GetContainer(path);
        if (container is null)
            return;

        path = Path.AsLinux(Path.GetRootlessPath(path));

        var destination = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(path));

        _dockerExecutor.CopyOutFile(container, path, destination, overwrite: true);

        var process = new Process { StartInfo = new ProcessStartInfo(destination) { UseShellExecute = true } };
        process.Start();
    }

    void IDirectoryHub.Delete(string path)
    {
        var container = GetContainer(path);
        if (container is null)
            return;

        path = Path.AsLinux(Path.GetRootlessPath(path));

        _dockerExecutor.DeleteDirectory(container, path);
    }

    void IDirectoryHub.Create(string path)
    {
        var container = GetContainer(path);
        if (container is null)
            return;

        path = Path.AsLinux(Path.GetRootlessPath(path));

        _dockerExecutor.CreateDirectory(container, path);
    }

    public ExecuteResult Execute(string path, string command)
    {
        var container = GetContainer(path);
        if (container is null)
            return ExecuteResult.Error;

        path = Path.AsLinux(Path.GetRootlessPath(path));

        return _dockerExecutor.Execute(container, path, command);
    }

    public CopyResult Copy(string source, string destination, bool overwrite)
    {
        var direction = Direction.Out;

        var container = GetContainer(source);
        if (container is null)
        {
            container = GetContainer(destination);
            if (container is null)
                return CopyResult.Error;

            direction = Direction.In;
        }

        var result = Commander.Docker.Models.CopyResult.Success;

        if (direction is Direction.Out)
        {
            source = Path.AsLinux(Path.GetRootlessPath(source));
            result = _dockerExecutor.CopyOutFile(container, source, destination, overwrite);
        }

        if (direction is Direction.In)
        {
            destination = Path.AsLinux(Path.GetRootlessPath(destination));
            result = _dockerExecutor.CopyInFile(container, source, destination, overwrite);
        }

        return result.ToCopyResult();
    }

    public CopyResult Move(string source, string destination, bool overwrite)
    {
        var direction = Direction.Out;

        var container = GetContainer(source);
        if (container is null)
        {
            container = GetContainer(destination);
            if (container is null)
                return CopyResult.Error;

            direction = Direction.In;
        }

        var result = Commander.Docker.Models.CopyResult.Success;

        if (direction is Direction.Out)
        {
            source = Path.AsLinux(Path.GetRootlessPath(source));
            result = _dockerExecutor.CopyOutFile(container, source, destination, overwrite);
        }

        if (direction is Direction.In)
        {
            destination = Path.AsLinux(Path.GetRootlessPath(destination));
            result = _dockerExecutor.CopyInFile(container, source, destination, overwrite);
        }

        if (result is Commander.Docker.Models.CopyResult.Success)
        {
            if (direction is Direction.Out)
                _dockerExecutor.DeleteFile(container, source);
            else
                File.Delete(source);
        }

        return result.ToCopyResult();
    }

    public CopyResult Rename(string source, string destination, bool overwrite)
    {
        var container = GetContainer(source);
        if (container is null)
            return CopyResult.Error;

        source = Path.AsLinux(Path.GetRootlessPath(source));
        destination = Path.AsLinux(Path.GetRootlessPath(destination));

        var copyResult = _dockerExecutor.Rename(container, source, destination, overwrite);
        return copyResult.ToCopyResult();
    }
}
