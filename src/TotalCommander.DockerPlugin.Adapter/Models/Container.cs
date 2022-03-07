using System;
using System.IO;

namespace TotalCommander.DockerPlugin.Adapter.Models;

[Serializable]
public class Container : TreeElement
{
    public Container()
    {
    }

    public Container(string id, string name, string imageName) : base(name, 0)
    {
        Id = id;
        Name = name;
        ImageName = imageName;
    }

    public string Id { get; set; }
    public string ImageName { get; set; }
    public override FileAttributes Attributes => FileAttributes.Directory;

    public override string ToString() => $"{Name} [{ImageName}]";
}
