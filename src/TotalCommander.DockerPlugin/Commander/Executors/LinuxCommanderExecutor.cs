using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.DockerCommander;
using TotalCommander.DockerPlugin.DockerCommander.Elements;
using TotalCommander.DockerPlugin.Infrastructure;

namespace TotalCommander.DockerPlugin.Commander.Executors;

public class LinuxCommanderExecutor : ICommanderExecutor
{
    public IEnumerable<TreeElement> GetAllContainers() => DockerUtils.GetContainers();

    public IEnumerable<TreeElement> GetDirectoryContent(string path)
    {
        var systemPath = SystemPath.AsLinux(path);
        var container = systemPath[0];
        systemPath = systemPath.Replace(systemPath[0], string.Empty);

        var dockerProcessCommandOutput = DockerUtils.DockerProcessCommand($"exec {container} ls -F {systemPath}");
        var allRows = dockerProcessCommandOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return allRows.Select(line => ElementsFactory.GetBuilder(line).Build()).ToArray();
    }

    public bool DeleteFile(string fileName)
    {
        var path = SystemPath.AsLinux(fileName);
        var container = path[0];
        path = path.Replace(path[0], string.Empty);

        DockerUtils.DockerProcessCommand($"exec {container} rm {path}");

        return true;
    }

    public bool MkDir(string path)
    {
        var systemPath = SystemPath.AsLinux(path);
        var container = systemPath[0];
        systemPath = systemPath.Replace(systemPath[0], string.Empty);

        DockerUtils.DockerProcessCommand($"exec {container} mkdir {systemPath}");

        return true;
    }

    public bool RemoveDir(string path)
    {
        var systemPath = SystemPath.AsLinux(path);
        var container = systemPath[0];
        systemPath = systemPath.Replace(systemPath[0], string.Empty);

        DockerUtils.DockerProcessCommand($"exec {container} rm -r {systemPath}");

        return true;
    }

    public void RenMovFile(string oldPath, string newPath, bool isMove, bool isOverwrite)
    {
        var oldSystemPath = SystemPath.AsLinux(oldPath);
        var newSystemPath = SystemPath.AsLinux(newPath);
    }

    public void ExecuteCommand(string remoteName, string command)
    {
        var path = SystemPath.AsLinux(remoteName);
        var container = path[0];
        path = path.Replace(path[0], string.Empty);

        DockerUtils.DockerProcessCommand($"exec {container} bash -c \"cd {path} && {command}\"");
    }

    public void OpenFile(string path)
    {
        var systemPath = SystemPath.AsLinux(path);
        var container = systemPath[0];
        systemPath = systemPath.Replace(systemPath[0], string.Empty);

        var hostDirectory = Path.GetTempPath();
        var fileName = systemPath[systemPath.Lenght - 1];
        var hostPath = $"{hostDirectory}{fileName}";

        DockerUtils.DockerProcessCommand($"cp {container}:{systemPath} {hostPath}");

        Process.Start(hostPath);
    }
}
