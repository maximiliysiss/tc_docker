using System;
using System.Linq;

namespace TotalCommander.DockerPlugin.Infrastructure.Path;

public static class Path
{
    public static bool IsRoot(string path) => path == $"{System.IO.Path.DirectorySeparatorChar}";

    public static string GetRootDirectory(string path)
    {
        return path
            .Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
            .First();
    }

    public static string GetRootlessPath(string path)
    {
        var separator = System.IO.Path.DirectorySeparatorChar;

        var rootlessParts = path
            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1);

        return $"{separator}{string.Join(separator, rootlessParts)}";
    }

    public static string AsLinux(string path) => path.Replace('\\', '/');
    public static string AsWindows(string path) => path.Replace('/', '\\');
}
