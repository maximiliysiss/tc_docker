using System.Collections.Generic;
using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.Commander.Executors;

public class WindowsCommanderExecutor : ICommanderExecutor
{
    public IEnumerable<TreeElement> GetAllContainers() => throw new System.NotImplementedException();
    public IEnumerable<TreeElement> GetDirectoryContent(string path) => throw new System.NotImplementedException();
    public bool DeleteFile(string fileName) => throw new System.NotImplementedException();
}
