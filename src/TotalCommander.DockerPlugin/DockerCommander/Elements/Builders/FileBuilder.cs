using TotalCommander.DockerPlugin.Adapter.Models;

namespace TotalCommander.DockerPlugin.DockerCommander.Elements.Builders;

public class FileBuilder : ITreeElementBuilder
{
    private readonly string _name;

    public FileBuilder(string name) => _name = name;

    public TreeElement Build() => new File(_name);
}
