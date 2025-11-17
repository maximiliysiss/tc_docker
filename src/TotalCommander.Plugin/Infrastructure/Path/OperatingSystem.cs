using System.IO;

namespace TotalCommander.Plugin.Infrastructure.Path;

public static class OperatingSystem
{
    public static void Delete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Ignore
        }
    }
}
