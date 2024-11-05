using System.Runtime.InteropServices;

namespace TotalCommander.Plugin.FileSystem.Native.Bridge.Infrastructure;

public static class NativeMethods
{
    [DllImport("kernel32.dll")]
    public static extern void SetLastError(uint errCode);
}
