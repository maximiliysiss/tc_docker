using System.Collections.Generic;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.DockerPlugin.Commander.Docker;

public interface IDockerExecutor
{
    IEnumerable<Container> EnumerateContainers();
    IEnumerable<Entry> EnumerateContainer(Container container, string path);
    void DeleteFile(Container container, string path);
    void DeleteDirectory(Container container, string path);
    void CreateDirectory(Container container, string path);
    ExecuteResult Execute(Container container, string path, string command);
    Models.CopyResult CopyOutFile(Container container, string path, string destination, bool overwrite);
    Models.CopyResult CopyInFile(Container container, string path, string destination, bool overwrite);
    Models.CopyResult Rename(Container container, string source, string destination, bool overwrite);
    bool IsExists(Container container, string path);
}
