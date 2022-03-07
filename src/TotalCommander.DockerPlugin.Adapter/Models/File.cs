using System;
using System.IO;

namespace TotalCommander.DockerPlugin.Adapter.Models;

[Serializable]
public class File: TreeElement
{
    public File()
    {
    }

    public File(string name, ulong size) : base(name, size)
    {
    }

    public override FileAttributes Attributes => FileAttributes.Normal;
}
