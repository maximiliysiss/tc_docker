using System;
using System.Collections.Generic;
using System.Linq;
using TotalCommander.DockerPlugin.Commander.Elements;
using TotalCommander.DockerPlugin.Plugin.Models;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Models;
using Console = TotalCommander.DockerPlugin.Infrastructure.Console.Console;
using CopyResult = TotalCommander.DockerPlugin.Commander.Docker.Models.CopyResult;
using File = System.IO.File;

namespace TotalCommander.DockerPlugin.Commander.Docker;

public sealed class DockerDockerExecutor : IDockerExecutor
{
    public IEnumerable<Container> EnumerateContainers()
    {
        var output = Console.Execute("ps --format \"{{.ID}} {{.Names}} {{.Image}}\"");

        if (output is null)
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => Map(c.Split(' ')));

        static Container Map(string[] fields)
        {
            return new Container(
                id: fields[0],
                name: fields[1],
                image: fields[2]);
        }
    }

    public IEnumerable<Entry> EnumerateContainer(Container container, string path)
    {
        var output = Console.Execute($"exec {container.Id} ls -lF {path}");

        if (output is null)
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Skip(1)
            .Select(EntryFactory.Create);
    }

    public void DeleteFile(Container container, string path) => Console.Execute($"exec {container.Id} rm {path}");
    public void DeleteDirectory(Container container, string path) => Console.Execute($"exec {container.Id} rm -r {path}");
    public void CreateDirectory(Container container, string path) => Console.Execute($"exec {container.Id} mkdir {path}");

    public ExecuteResult Execute(Container container, string path, string command)
    {
        var execute = Console.Execute($"exec {container.Id} bash -c \"cd {path} && {command}\"");

        return execute is null
            ? ExecuteResult.Error
            : ExecuteResult.Success;
    }

    public CopyResult CopyOutFile(Container container, string path, string destination, bool overwrite)
    {
        if (File.Exists(destination) && overwrite is false)
            return CopyResult.Exists;

        var execute = Console.Execute($"cp {container.Id}:{path} {destination}");

        return execute is not null
            ? CopyResult.Success
            : CopyResult.Failure;
    }

    public CopyResult CopyInFile(Container container, string path, string destination, bool overwrite)
    {
        if (IsExists(container, destination) && overwrite is false)
            return CopyResult.Exists;

        var execute = Console.Execute($"cp {path} {container.Id}:{destination}");

        return execute is not null
            ? CopyResult.Success
            : CopyResult.Failure;
    }

    public CopyResult Rename(Container container, string source, string destination, bool overwrite)
    {
        if (IsExists(container, destination) && overwrite is false)
            return CopyResult.Exists;

        var execute = Console.Execute($"exec {container.Id} bash -c \"mv -f {source} {destination}\"");

        return execute is not null
            ? CopyResult.Success
            : CopyResult.Failure;
    }

    public bool IsExists(Container container, string path)
    {
        const string expectedOutput = "exists";

        var execute = Console.Execute($"exec {container.Id} bash -c \"[ -f {path} ] && echo '{expectedOutput}'\"");

        return execute is expectedOutput;
    }
}
