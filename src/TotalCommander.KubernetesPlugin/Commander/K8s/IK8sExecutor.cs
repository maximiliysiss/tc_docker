using System.Collections.Generic;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

namespace TotalCommander.KubernetesPlugin.Commander.K8s;

public interface IK8sExecutor
{
    IEnumerable<Context> EnumerateContexts();
    IEnumerable<Namespace> EnumerateNamespaces(Context context);
    IEnumerable<Pod> EnumeratePods(Context context, Namespace ns);
    IEnumerable<Entry> EnumerateContainer(Path path);
    void CreateDirectory(Path path);
    void DeleteDirectory(Path path);
    void CreateFile(Path path);
    void DeleteFile(Path path);
    ExecuteResult Execute(Path path, string command);
    CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction);
    CopyResult Rename(Path source, Path destination, bool overwrite);
    CopyResult Move(Path source, Path destination, bool overwrite, Direction direction);
}
