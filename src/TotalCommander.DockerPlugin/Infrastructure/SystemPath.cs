using System;

namespace TotalCommander.DockerPlugin.Infrastructure;

public class SystemPath
{
    private readonly string _path;
    private readonly string[] _sections;
    private readonly SystemPathType _systemPathType;

    private SystemPath(string path, SystemPathType systemPathType)
    {
        _systemPathType = systemPathType;

        var pathDelimiter = (char)systemPathType;
        _path = path
            .Replace('\\', pathDelimiter)
            .Replace('/', pathDelimiter)
            .Replace($"{pathDelimiter}{pathDelimiter}", $"{pathDelimiter}");

        _sections = _path.Split(new[] { (char)systemPathType }, StringSplitOptions.RemoveEmptyEntries);
    }

    public SystemPath Replace(string oldValue, string newValue) => new(_path.Replace(oldValue, newValue), _systemPathType);

    public string this[int i] => _sections[i];
    public int Lenght => _sections.Length;

    public override string ToString() => _path;

    public static SystemPath AsLinux(string path) => new(path, SystemPathType.Linux);
    public static SystemPath AsWindows(string path) => new(path, SystemPathType.Windows);

    private enum SystemPathType
    {
        Linux = '/',
        Windows = '\\'
    }
}
