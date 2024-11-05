using System.IO;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.DockerPlugin.Plugin.Models;

public sealed class Container(string id, string name, string image) : Entry
{
    public string Id { get; } = id;
    public string Name { get; } = $"{name} [{image}]";

    public override TotalCommander.Plugin.FileSystem.Native.Models.Entry AsNative() => new(Name, FileAttributes.Directory);
}
