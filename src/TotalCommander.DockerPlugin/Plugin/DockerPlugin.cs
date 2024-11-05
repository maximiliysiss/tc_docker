using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TotalCommander.DockerPlugin.Commander.Docker;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using Path = TotalCommander.DockerPlugin.Infrastructure.Path.Path;

namespace TotalCommander.DockerPlugin.Plugin;

public sealed class DockerPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub
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

        var tempPath = _dockerExecutor.CopyFileFromContainer(container, path);

        var process = new Process { StartInfo = new ProcessStartInfo(tempPath) { UseShellExecute = true } };
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
}
