using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.DockerCommander.Elements.Builders;

public class DirectoryBuilder : ITreeElementBuilder
{
    private readonly string _name;

    public DirectoryBuilder(string name) => _name = name.Substring(0, name.Length - 1);

    public TreeElement Build() => new Directory(_name);
}
