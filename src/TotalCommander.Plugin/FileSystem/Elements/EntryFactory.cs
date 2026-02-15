using System;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.Plugin.FileSystem.Elements;

public static class EntryFactory
{
    public static Entry Create(string entry, char separator = '\u001f')
    {
        const int nameIndex = 0;
        const int rightsIndex = 1;
        const int sizeIndex = 2;

        var parts = entry.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var name = parts[nameIndex];
        var key = (ElementsType)parts[rightsIndex][0];

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
