using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.DockerCommander.Elements.Builders;

public class DirectoryBuilder : ITreeElementBuilder
{
    private readonly string _line;

    public DirectoryBuilder(string line) => _line = line;

    public TreeElement Build()
    {
        var name = _line.Substring(0, _line.Length - 1).Split(new[] { ' ' }, 2)[1];
        return new Directory(name);
    }
}
