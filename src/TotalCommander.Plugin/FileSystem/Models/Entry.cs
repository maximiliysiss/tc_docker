namespace TotalCommander.Plugin.FileSystem.Models;

public abstract class Entry
{
    public abstract string Name { get; }
    public abstract Native.Models.Entry AsNative();
}
