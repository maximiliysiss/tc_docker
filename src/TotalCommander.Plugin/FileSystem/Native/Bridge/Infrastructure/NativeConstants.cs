using System;

namespace TotalCommander.Plugin.FileSystem.Native.Bridge.Infrastructure;

public static class NativeConstants
{
    public static readonly IntPtr InvalidHandle = new(-1);

    public const int NoContent = 18;

    public const int FileExists = 1;
}
