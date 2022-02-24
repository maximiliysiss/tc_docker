using TotalCommander.DockerPlugin.DockerCommander.Elements.Builders;

namespace TotalCommander.DockerPlugin.DockerCommander.Elements;

public class ElementsFactory
{
    private enum ElementsFactoryType
    {
        Directory = '/',
        Executable = '*',
        Symbol = '@',
        Socket = '=',
        Door = '>',
        Pipe = '|'
    }

    public static ITreeElementBuilder GetBuilder(string name)
    {
        var lastChar = (ElementsFactoryType)name[name.Length - 1];

        switch (lastChar)
        {
            case ElementsFactoryType.Directory:
                return new DirectoryBuilder(name);
            case ElementsFactoryType.Executable:
            case ElementsFactoryType.Symbol:
            case ElementsFactoryType.Socket:
            case ElementsFactoryType.Door:
            case ElementsFactoryType.Pipe:
                return new FileBuilder(name.Substring(0, name.Length - 1));
            default:
                return new FileBuilder(name);
        }
    }
}
