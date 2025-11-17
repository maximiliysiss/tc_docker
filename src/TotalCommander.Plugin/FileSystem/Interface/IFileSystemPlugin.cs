using System.Collections.Generic;
using TotalCommander.Plugin.FileSystem.Models;

namespace TotalCommander.Plugin.FileSystem.Interface;

public interface IFileSystemPlugin
{
    string Name { get; }

    IEnumerable<Entry> EnumerateEntries(string path);

    void Init();
}
