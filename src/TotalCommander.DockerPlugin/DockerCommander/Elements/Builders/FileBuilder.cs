using System;
using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.DockerCommander.Elements.Builders;

public class FileBuilder : ITreeElementBuilder
{
    private readonly string _line;

    public FileBuilder(string line) => _line = line;

    public TreeElement Build()
    {
        var parts = _line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        return new File(parts[1], ulong.Parse(parts[0]));
    }
}
