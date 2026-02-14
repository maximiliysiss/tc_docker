using System;
using System.Linq;

namespace TotalCommander.Plugin.Infrastructure.Path;

public static class LinuxOs
{
    public static string PathAs(string path)
    {
        const char unixSeparator = '/';
        const char windowsSeparator = '\\';
        const char space = ' ';

        path = path.Replace(windowsSeparator, unixSeparator);

        if (!path.Contains(space))
            return path;

        var parts = path
            .Split(unixSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Contains(space) ? $"\"{c}\"" : c);

        return string.Join(unixSeparator, parts);
    }
}
