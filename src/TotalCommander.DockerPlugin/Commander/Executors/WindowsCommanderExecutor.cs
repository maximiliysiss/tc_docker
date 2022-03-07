using System;
using System.Collections.Generic;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.DockerCommander;

namespace TotalCommander.DockerPlugin.Commander.Executors;

public class WindowsCommanderExecutor : ICommanderExecutor
{
    public IEnumerable<TreeElement> GetAllContainers() => DockerUtils.GetContainers();
    public IEnumerable<TreeElement> GetDirectoryContent(string path) => throw new System.NotImplementedException();
    public bool DeleteFile(string fileName) => throw new System.NotImplementedException();
    public bool MkDir(string path) => throw new System.NotImplementedException();
    public bool RemoveDir(string path) => throw new System.NotImplementedException();
    public void ExecuteCommand(string remoteName, string command) => throw new NotImplementedException();
    public void OpenFile(string path) => throw new NotImplementedException();
}
