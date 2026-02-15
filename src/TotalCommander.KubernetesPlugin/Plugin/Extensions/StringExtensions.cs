using System.IO;

namespace TotalCommander.KubernetesPlugin.Plugin.Extensions;

internal static class StringExtensions
{
    public static string TrimEndPath(this string path) => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    public static Infrastructure.Path.Path AsPath(this string localPath) => new(null, null, null, localPath);
}
