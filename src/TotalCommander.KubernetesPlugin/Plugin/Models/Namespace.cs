using System.IO;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.KubernetesPlugin.Plugin.Models;

public sealed class Namespace(string name) : Entry
{
    public string Name => name;
    public override TotalCommander.Plugin.FileSystem.Native.Models.Entry AsNative() => new(Name, FileAttributes.Directory);
}
