using System;
using System.Diagnostics;
using System.Linq;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.DockerCommander.Elements;
using TotalCommander.DockerPlugin.Infrastructure;

namespace TotalCommander.DockerPlugin.DockerCommander;

public class DockerCommander
{
    private static string DockerProcessCommand(string arguments)
    {
        Process dockerProcess = new Process();

        dockerProcess.StartInfo.FileName = "docker";
        dockerProcess.StartInfo.Arguments = arguments;
        dockerProcess.StartInfo.RedirectStandardOutput = true;
        dockerProcess.StartInfo.CreateNoWindow = true;
        dockerProcess.StartInfo.UseShellExecute = false;

        dockerProcess.Start();
        dockerProcess.WaitForExit();

        return dockerProcess.StandardOutput.ReadToEnd();
    }

    public static Container[] GetContainers()
    {
        var dockerProcessCommandOutput = DockerProcessCommand("ps --format \"{{.ID}} {{.Names}} {{.Image}}\"");
        var allRows = dockerProcessCommandOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Split(' '));
        return allRows.Select(r => new Container(r[0], r[1], r[2])).ToArray();
    }

    public static TreeElement[] GetDirectoryContent(SystemPath path)
    {
        var container = path[0];
        path = path.Replace(path[0], string.Empty);

        var dockerProcessCommandOutput = DockerProcessCommand($"exec {container} ls -F {path}");
        var allRows = dockerProcessCommandOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return allRows.Select(line => ElementsFactory.GetBuilder(line).Build()).ToArray();
    }

    public static bool DeleteFile(SystemPath path)
    {
        var container = path[0];
        path = path.Replace(path[0], string.Empty);

        DockerProcessCommand($"exec {container} rm {path}");
        
        return true;
    }
}
