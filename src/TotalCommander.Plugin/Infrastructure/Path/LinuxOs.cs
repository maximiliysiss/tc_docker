namespace TotalCommander.Plugin.Infrastructure.Path;

public static class LinuxOs
{
    public static string PathAs(string path)
    {
        const char unixSeparator = '/';
        const char windowsSeparator = '\\';

        return path.Replace(windowsSeparator, unixSeparator);
    }
}
