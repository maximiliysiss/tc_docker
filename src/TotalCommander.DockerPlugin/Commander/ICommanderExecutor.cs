using System.Collections.Generic;
using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.Commander;

public interface ICommanderExecutor
{
    IEnumerable<TreeElement> GetAllContainers();
    IEnumerable<TreeElement> GetDirectoryContent(string path);
    bool DeleteFile(string fileName);
}
