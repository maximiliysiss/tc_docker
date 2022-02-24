using System;
using System.IO;

namespace TotalCommander.DockerPlugin.Adapter.Models;

[Serializable]
public abstract class TreeElement
{
    public TreeElement()
    {
    }

    protected TreeElement(string name) => Name = name;

    public abstract FileAttributes Attributes { get; }
    public string Name { get; set; }

    public override string ToString() => Name;
}
