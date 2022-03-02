using System;
using System.Diagnostics;
using System.Linq;
using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.DockerCommander;

public class DockerUtils
{
    public static string DockerProcessCommand(string arguments)
    {
        using var dockerProcess = new Process();

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
}
