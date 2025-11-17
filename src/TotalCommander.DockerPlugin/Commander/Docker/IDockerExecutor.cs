using System.Collections.Generic;
using TotalCommander.DockerPlugin.Infrastructure.Path;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

namespace TotalCommander.DockerPlugin.Commander.Docker;

public interface IDockerExecutor
{
    IEnumerable<Container> EnumerateContainers();
    IEnumerable<Entry> EnumerateContainer(Path path);
    void CreateFile(Path path);
    void DeleteFile(Path path);
    void DeleteDirectory(Path path);
    void CreateDirectory(Path path);
    ExecuteResult Execute(Path path, string command);
    CopyResult Copy(Path source, Path destination, bool overwrite, Direction direction);
    CopyResult Move(Path source, Path destination, bool overwrite, Direction direction);
    CopyResult Rename(Path source, Path destination, bool overwrite);
}
