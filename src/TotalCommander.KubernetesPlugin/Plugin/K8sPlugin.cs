using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TotalCommander.KubernetesPlugin.Commander.K8s;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using Directory = System.IO.Directory;

namespace TotalCommander.KubernetesPlugin.Plugin;

public sealed class K8sPlugin : IFileSystemPlugin, IFileHub, IDirectoryHub, IExecutionHub, IMoveHub
{
    private readonly IK8sExecutor _executor = new K8sExecutor();

    private Context[] _contexts = [];
    private Namespace[] _namespaces = [];
    private Pod[] _pods = [];

    public string Name => "kubernetes";

    public IEnumerable<Entry> EnumerateEntries(string path)
    {
        var parsed = Path.Parse(path);

        return parsed switch
        {
            { Context: null } => _contexts = _executor.EnumerateContexts().ToArray(),
            { Namespace: null } => _namespaces = _executor.EnumerateNamespaces(parsed.Context).ToArray(),
            { Pod: null } => _pods = _executor.EnumeratePods(parsed.Context, parsed.Namespace).ToArray(),
            _ => _executor.EnumerateContainer(parsed),
        };
    }

    public void Init()
    {
    }

    void IFileHub.Create(string path) => _executor.CreateFile(Path.Parse(path));
    void IDirectoryHub.Delete(string path) => _executor.DeleteDirectory(Path.Parse(path));
    void IDirectoryHub.Create(string path) => _executor.CreateDirectory(Path.Parse(path));
    void IFileHub.Delete(string path) => _executor.DeleteFile(Path.Parse(path));
    public ExecuteResult Execute(string path, string command) => _executor.Execute(Path.Parse(path), command);

    public void Open(string path)
    {
        var parsed = Path.Parse(path);
        if (!parsed.IsFull)
            return;

        var absolute = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(path));
        var relative = System.IO.Path.GetRelativePath(Directory.GetCurrentDirectory(), absolute);

        var destination = new Path(Context: null, Namespace: null, Pod: null, LocalPath: relative);

        _executor.Copy(parsed, destination, overwrite: true);

        Process.Start(new ProcessStartInfo(relative) { UseShellExecute = true });
    }

    public CopyResult Copy(string source, string destination, bool overwrite, Direction direction)
    {
        var environment = new Environment(_contexts, _namespaces, _pods);
        return _executor.Copy(Path.Parse(source, environment), Path.Parse(destination, environment), overwrite);
    }

    public CopyResult Move(string source, string destination, bool overwrite, Direction direction)
    {
        var environment = new Environment(_contexts, _namespaces, _pods);
        return _executor.Move(Path.Parse(source, environment), Path.Parse(destination, environment), overwrite);
    }

    public CopyResult Rename(string source, string destination, bool overwrite)
        => _executor.Rename(Path.Parse(source), Path.Parse(destination), overwrite);
}
