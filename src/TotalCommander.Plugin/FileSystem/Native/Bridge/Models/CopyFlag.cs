using System;

namespace TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

[Flags]
public enum CopyFlag
{
    None = 0,
    Overwrite = 1,
    Move = 4,
}
