using System;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.Plugin.FileSystem.Elements;

public static class EntryFactory
{
    public static Entry Create(string entry)
    {
        var nameIndex = 8;
        var sizeIndex = 4;
        const int rightsIndex = 0;

        var parts = entry.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var rights = parts[rightsIndex];

        if (rights.IndexOfAny(['c', 'b']) is not -1)
        {
            nameIndex++;
            sizeIndex++;
        }

        var fullName = string.Join(' ', parts[nameIndex..]);

        var key = (ElementsType)fullName[^1];
        var isDefined = Convert.ToInt32(Enum.IsDefined(key));

        if (rights.Contains('l'))
            key = (ElementsType)parts[^1][^1];

        var name = fullName[..^isDefined].Trim('\'');

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
