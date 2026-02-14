using System.IO;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.DockerPlugin.Plugin.Models;

public sealed class Container(string name) : Entry
{
    public override string Name { get; } = name;

    public override TotalCommander.Plugin.FileSystem.Native.Models.Entry AsNative() => new(Name, FileAttributes.Directory);
}
