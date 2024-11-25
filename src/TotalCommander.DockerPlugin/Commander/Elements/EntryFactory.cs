using System;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.DockerPlugin.Commander.Elements;

public static class EntryFactory
{
    public static Entry Create(string entry)
    {
        const int nameIndex = 8;
        const int sizeIndex = 4;

        var parts = entry.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var key = (ElementsType)parts[nameIndex][^1];

        var isDefined = Convert.ToInt32(Enum.IsDefined(key));

        var name = parts[nameIndex][..^isDefined];

        return key switch
        {
            ElementsType.Directory => new Directory(name: name),
            _ => new File(name: name, size: ulong.Parse(parts[sizeIndex]))
        };
    }

    private enum ElementsType
    {
        Directory = '/',
        Executable = '*',
        Symbol = '@',
        Socket = '=',
        Door = '>',
        Pipe = '|'
    }
}
