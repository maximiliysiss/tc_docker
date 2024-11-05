using System;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.DockerPlugin.Commander.Elements;

public static class EntryFactory
{
    public static Entry Create(string entry)
    {
        var parts = entry.Split(' ');

        var key = (ElementsType)parts[1][^1];

        var isDefined = Convert.ToInt32(Enum.IsDefined(key));

        var name = parts[1][..^isDefined];

        return key switch
        {
            ElementsType.Directory => new Directory(name: name),
            _ => new File(name: name, size: ulong.Parse(parts[0]))
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
