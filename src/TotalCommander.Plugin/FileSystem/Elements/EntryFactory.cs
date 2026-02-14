using System;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.Plugin.FileSystem.Elements;

public static class EntryFactory
{
    public static Entry Create(string entry)
    {
        const int nameIndex = 0;
        const int rightsIndex = 1;
        const int sizeIndex = 2;

        var parts = entry.Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var name = parts[nameIndex];

        var rights = parts[rightsIndex];
        var key = (ElementsType)rights[0];

        return key switch
        {
            ElementsType.Directory => new Directory(name: name),
            _ => new File(name: name, size: ulong.Parse(parts[sizeIndex]))
        };
    }

    private enum ElementsType
    {
        File = '-',
        Directory = 'd',
        Link = 'l',
        Character = 'c',
        Block = 'b',
        Socket = 's',
        Pipe = 'p',
        Unknown = '?',
    }
}
