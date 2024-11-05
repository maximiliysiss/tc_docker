using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using TotalCommander.Plugin.FileSystem.Native.Infrastructure;

namespace TotalCommander.Plugin.FileSystem.Native.Models;

[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct AnsiTcFindData
{
    public int FileAttributes;

    public FILETIME CreationTime;
    public FILETIME LastAccessTime;
    public FILETIME LastWriteTime;

    public uint FileSizeHigh;
    public uint FileSizeLow;

    public uint Reserved0;
    public uint Reserved1;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MaxPathAnsi)]
    public string FileName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
    public string AlternateFileName;
}
