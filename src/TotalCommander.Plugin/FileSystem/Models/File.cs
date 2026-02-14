using System.IO;

namespace TotalCommander.Plugin.FileSystem.Models;

public sealed class File(string name, ulong size) : Entry
{
    public override string Name => name;
    public override Native.Models.Entry AsNative() => new(name, FileAttributes.Normal, size);
}
