using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TotalCommander.KubernetesPlugin.Commander.K8s;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;
using TotalCommander.KubernetesPlugin.Plugin.Extensions;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;
using Directory = System.IO.Directory;

namespace TotalCommander.KubernetesPlugin.Plugin;

public sealed class K8sPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub, IMoveHub
{
    private readonly IK8sExecutor _executor = new K8sExecutor();

    private readonly ILogger _logger = new TraceLogger("k8splugin.log");

    public string Name => "kubernetes";

    public void Init()
    {
#if DEBUG
        Trace.AutoFlush = true;
        Trace.Listeners.Add(new TextWriterTraceListener("k8s.log"));
#endif
    }

    public IEnumerable<Entry> EnumerateEntries(string path)
    {
        var parsed = Path.Parse(path);

        return parsed switch
        {
            { Context: null } => _executor.EnumerateContexts().ToArray(),
            { Namespace: null } => _executor.EnumerateNamespaces(parsed.Context).ToArray(),
            { Pod: null } => _executor.EnumeratePods(parsed.Context, parsed.Namespace).ToArray(),
            _ => _executor.EnumerateContainer(parsed),
        };
    }

    void IFileHub.Create(string path) => _executor.CreateFile(Path.Parse(path));
    void IDirectoryHub.Delete(string path) => _executor.DeleteDirectory(Path.Parse(path));
    void IDirectoryHub.Create(string path) => _executor.CreateDirectory(Path.Parse(path));
    void IFileHub.Delete(string path) => _executor.DeleteFile(Path.Parse(path));
    public ExecuteResult Execute(string path, string command) => _executor.Execute(Path.Parse(path), command);

    public void Open(string path)
    {
        _logger.LogInfo($"Opening {path}");

        var parsed = Path.Parse(path);
        if (!parsed.IsFull)
            return;

        var workingDirectory = System.IO.Path.GetTempPath();
        var relative = System.IO.Path.GetFileName(path);

        var absolute = System.IO.Path.Combine(workingDirectory, relative);

        var destination = new Path(Context: null, Namespace: null, Pod: null, LocalPath: relative);

        _executor.Copy(parsed, destination, overwrite: true, Direction.Out, workingDirectory);

        Process.Start(new ProcessStartInfo(absolute) { UseShellExecute = true });
    }

    public CopyResult Copy(string source, string destination, bool overwrite, Direction direction)
    {
        _logger.LogInfo($"Copying {source} to {destination} with overwrite {overwrite} and direction {direction}");

        var (sourcePath, sourceWorkingDirectory) = direction switch
        {
            Direction.In => (AsPath(source), GetWorkingDirectory(source)),
            Direction.Out or Direction.Inter => (Path.Parse(source), null),
            _ => (null, null),
        };

        var (destinationPath, destinationWorkingDirectory) = direction switch
        {
            Direction.In or Direction.Inter => (Path.Parse(destination), null),
            Direction.Out => (AsPath(destination), GetWorkingDirectory(destination)),
            _ => (null, null)
        };

        var workingDirectory = sourceWorkingDirectory ?? destinationWorkingDirectory;

        if (sourcePath is null || destinationPath is null || (workingDirectory is null && direction is not Direction.Inter))
        {
            _logger.LogError($"Failed to copy {source} to {destination} with overwrite {overwrite}");
            return CopyResult.Error;
        }

        return _executor.Copy(sourcePath, destinationPath, overwrite, direction, workingDirectory);
    }

    public CopyResult Move(string source, string destination, bool overwrite, Direction direction)
    {
        _logger.LogInfo($"Moving from {source} to {destination} with overwrite {overwrite} and direction {direction}");

        var (sourcePath, sourceWorkingDirectory) = direction switch
        {
            Direction.In => (AsPath(source), GetWorkingDirectory(source)),
            Direction.Out or Direction.Inter => (Path.Parse(source), null),
            _ => (null, null),
        };

        var (destinationPath, destinationWorkingDirectory) = direction switch
        {
            Direction.In or Direction.Inter => (Path.Parse(destination), null),
            Direction.Out => (AsPath(destination), GetWorkingDirectory(destination)),
            _ => (null, null)
        };

        var workingDirectory = sourceWorkingDirectory ?? destinationWorkingDirectory;

        if (sourcePath is null || destinationPath is null || (workingDirectory is null && direction is not Direction.Inter))
        {
            _logger.LogError($"Failed to copy {source} to {destination} with overwrite {overwrite}");
            return CopyResult.Error;
        }

        return _executor.Move(sourcePath, destinationPath, overwrite, direction, workingDirectory);
    }

    public CopyResult Rename(string source, string destination, bool overwrite)
    {
        _logger.LogInfo($"Renaming {source} to {destination} with overwrite {overwrite}");
        return _executor.Rename(Path.Parse(source), Path.Parse(destination), overwrite);
    }

    private static Path AsPath(string path) => System.IO.Path.GetFileName(path.TrimEndPath()).AsPath();
    private static string? GetWorkingDirectory(string path) => Directory.GetParent(path)?.FullName;
}
