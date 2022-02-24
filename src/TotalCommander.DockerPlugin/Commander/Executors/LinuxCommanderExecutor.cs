using System.Collections.Generic;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.Infrastructure;

namespace TotalCommander.DockerPlugin.Commander.Executors;

public class LinuxCommanderExecutor : ICommanderExecutor
{
    public IEnumerable<TreeElement> GetAllContainers() => DockerCommander.DockerCommander.GetContainers();

    public IEnumerable<TreeElement> GetDirectoryContent(string path)
        => DockerCommander.DockerCommander.GetDirectoryContent(SystemPath.AsLinux(path));

    public bool DeleteFile(string fileName) => DockerCommander.DockerCommander.DeleteFile(SystemPath.AsLinux(fileName));
}
