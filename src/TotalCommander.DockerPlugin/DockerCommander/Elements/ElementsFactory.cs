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

    public static ITreeElementBuilder GetBuilder(string line)
    {
        line = line.Trim();
        var lastChar = (ElementsFactoryType)line[line.Length - 1];

        switch (lastChar)
        {
            case ElementsFactoryType.Directory:
                return new DirectoryBuilder(line);
            case ElementsFactoryType.Executable:
            case ElementsFactoryType.Symbol:
            case ElementsFactoryType.Socket:
            case ElementsFactoryType.Door:
            case ElementsFactoryType.Pipe:
                return new FileBuilder(line.Substring(0, line.Length - 1));
            default:
                return new FileBuilder(line);
        }
    }
}
