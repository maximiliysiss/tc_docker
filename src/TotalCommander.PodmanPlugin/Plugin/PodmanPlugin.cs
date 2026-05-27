using System;
using System.Collections.Generic;
using System.Diagnostics;
using TotalCommander.PodmanPlugin.Commander.Podman;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using Path = TotalCommander.PodmanPlugin.Infrastructure.Path.Path;

namespace TotalCommander.PodmanPlugin.Plugin;

public sealed class PodmanPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub, IMoveHub
{
    private readonly IPodmanExecutor _podmanExecutor = new PodmanExecutor();

    public string Name => "podman";

    public IEnumerable<Entry> EnumerateEntries(string path)
    {
        var parsed = Path.Parse(path);

        return parsed switch
        {
            { Container: null } => _podmanExecutor.EnumerateContainers(),
            _ => _podmanExecutor.EnumerateContainer(parsed)
        };
    }

    public void Init()
    {
#if DEBUG
        Trace.AutoFlush = true;
        Trace.Listeners.Add(new TextWriterTraceListener("podman.log"));
#endif
    }

    void IFileHub.Create(string path) => _podmanExecutor.CreateFile(Path.Parse(path));
    void IFileHub.Delete(string path) => _podmanExecutor.DeleteFile(Path.Parse(path));
    void IDirectoryHub.Delete(string path) => _podmanExecutor.DeleteDirectory(Path.Parse(path));
    void IDirectoryHub.Create(string path) => _podmanExecutor.CreateDirectory(Path.Parse(path));
    public ExecuteResult Execute(string path, string command) => _podmanExecutor.Execute(Path.Parse(path), command);

    void IFileHub.Open(string path)
    {
        var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(path));

        var source = Path.Parse(path);
        var destination = new Path(Container: null, LocalPath: localPath);

        var copyResult = _podmanExecutor.Copy(source, destination, overwrite: true, Direction.Out);
        if (copyResult is CopyResult.Error)
            return;

        Process.Start(new ProcessStartInfo(localPath) { UseShellExecute = true });
    }

    public CopyResult Copy(string source, string destination, bool overwrite, Direction direction)
    {
        var sourcePath = direction switch
        {
            Direction.In => new Path(Container: null, LocalPath: source),
            Direction.Out or Direction.Inter => Path.Parse(source),
            _ => null,
        };

        var destinationPath = direction switch
        {
            Direction.In or Direction.Inter => Path.Parse(destination),
            Direction.Out => new Path(Container: null, LocalPath: destination),
            _ => null,
        };

        if (sourcePath is null || destinationPath is null)
            return CopyResult.Error;

        return _podmanExecutor.Copy(sourcePath, destinationPath, overwrite, direction);
    }

    public CopyResult Move(string source, string destination, bool overwrite, Direction direction)
    {
        var sourcePath = direction switch
        {
            Direction.In => new Path(Container: null, LocalPath: source),
            Direction.Out or Direction.Inter => Path.Parse(source),
            _ => null,
        };

        var destinationPath = direction switch
        {
            Direction.In or Direction.Inter => Path.Parse(destination),
            Direction.Out => new Path(Container: null, LocalPath: destination),
            _ => null,
        };

        if (sourcePath is null || destinationPath is null)
            return CopyResult.Error;

        return _podmanExecutor.Move(sourcePath, destinationPath, overwrite, direction);
    }

    public CopyResult Rename(string source, string destination, bool overwrite)
        => _podmanExecutor.Rename(Path.Parse(source), Path.Parse(destination), overwrite);
}
