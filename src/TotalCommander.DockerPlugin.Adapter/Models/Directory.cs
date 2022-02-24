using System;
using System.IO;

namespace TotalCommander.DockerPlugin.Adapter.Models;

[Serializable]
public class Directory : TreeElement
{
    public Directory()
    {
    }

    public Directory(string name) : base(name)
    {
    }

    public override FileAttributes Attributes => FileAttributes.Directory;
}
