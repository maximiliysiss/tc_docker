using System;
using System.IO;

namespace TotalCommander.DockerPlugin.Adapter.Models;

[Serializable]
public class Directory : TreeElement
{
    public Directory()
    {
    }

    public Directory(string name) : base(name, 0)
    {
    }

    public override FileAttributes Attributes => FileAttributes.Directory;
}
