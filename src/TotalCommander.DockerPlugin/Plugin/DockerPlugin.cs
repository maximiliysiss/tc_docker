using System;
using System.Collections.Generic;
using System.Diagnostics;
using TotalCommander.DockerPlugin.Commander.Docker;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using Path = TotalCommander.DockerPlugin.Infrastructure.Path.Path;

namespace TotalCommander.DockerPlugin.Plugin;

public sealed class DockerPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub, IMoveHub
{
    private readonly IDockerExecutor _dockerExecutor = new DockerExecutor();

    public string Name => "docker";

    public IEnumerable<Entry> EnumerateEntries(string path)
    {
        var parsed = Path.Parse(path);

        return parsed switch
        {
            { Container: null } => _dockerExecutor.EnumerateContainers(),
            _ => _dockerExecutor.EnumerateContainer(parsed)
        };
    }

    public void Init()
    {
#if DEBUG
        Trace.AutoFlush = true;
        Trace.Listeners.Add(new TextWriterTraceListener("docker.log"));
#endif
    }

    void IFileHub.Create(string path) => _dockerExecutor.CreateFile(Path.Parse(path));
    void IFileHub.Delete(string path) => _dockerExecutor.DeleteFile(Path.Parse(path));
    void IDirectoryHub.Delete(string path) => _dockerExecutor.DeleteDirectory(Path.Parse(path));
    void IDirectoryHub.Create(string path) => _dockerExecutor.CreateDirectory(Path.Parse(path));
    public ExecuteResult Execute(string path, string command) => _dockerExecutor.Execute(Path.Parse(path), command);

    void IFileHub.Open(string path)
    {
        var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(path));

        var source = Path.Parse(path);
        var destination = new Path(Container: null, LocalPath: localPath);

        var copyResult = _dockerExecutor.Copy(source, destination, overwrite: true, Direction.Out);
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

        return _dockerExecutor.Copy(sourcePath, destinationPath, overwrite, direction);
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

        return _dockerExecutor.Move(sourcePath, destinationPath, overwrite, direction);
    }

    public CopyResult Rename(string source, string destination, bool overwrite)
        => _dockerExecutor.Rename(Path.Parse(source), Path.Parse(destination), overwrite);
}
