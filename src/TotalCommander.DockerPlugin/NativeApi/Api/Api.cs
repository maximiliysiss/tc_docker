using System;
using System.Runtime.InteropServices;
using TotalCommander.Plugin.FileSystem.Native.Bridge;

namespace TotalCommander.DockerPlugin.NativeApi.Api;

public static class Api
{
    private static readonly Bridge s_bridge = new(new Plugin.DockerPlugin());

    [UnmanagedCallersOnly(EntryPoint = "FsInit")]
    public static int Init(int pluginNumber, IntPtr progressProc, IntPtr logProc, IntPtr requestProc) => 0;

    [UnmanagedCallersOnly(EntryPoint = "FsInitW")]
    public static int InitW(int pluginNumber, IntPtr progressProcW, IntPtr logProcW, IntPtr requestProcW) => 0;

    [UnmanagedCallersOnly(EntryPoint = "FsFindFirst")]
    public static IntPtr FindFirst(IntPtr path, IntPtr findFile) => FindFirstInternal(Marshal.PtrToStringAuto(path), findFile, false);

    [UnmanagedCallersOnly(EntryPoint = "FsFindFirstW")]
    public static IntPtr FindFirstW(IntPtr path, IntPtr findFile) => FindFirstInternal(Marshal.PtrToStringAuto(path), findFile, true);

    private static IntPtr FindFirstInternal(string? path, IntPtr findFile, bool isUnicode) => s_bridge.FindFirst(path, findFile, isUnicode);

    [UnmanagedCallersOnly(EntryPoint = "FsFindNext")]
    public static bool FindNext(IntPtr hdl, IntPtr findFile) => FindNextInternal(hdl, findFile, false);

    [UnmanagedCallersOnly(EntryPoint = "FsFindNextW")]
    public static bool FindNextW(IntPtr hdl, IntPtr findFile) => FindNextInternal(hdl, findFile, true);

    private static bool FindNextInternal(IntPtr hdl, IntPtr findFile, bool isUnicode) => s_bridge.FindNext(hdl, findFile, isUnicode);

    [UnmanagedCallersOnly(EntryPoint = "FsFindClose")]
    public static int FindClose(IntPtr hdl) => s_bridge.FindClose(hdl);

    [UnmanagedCallersOnly(EntryPoint = "FsContentGetSupportedField")]
    public static int GetSupportedField(int fieldIndex, IntPtr fieldName, IntPtr units, int maxLen) => 0;

    [UnmanagedCallersOnly(EntryPoint = "FsDeleteFile")]
    public static bool DeleteFile(IntPtr fileName) => s_bridge.DeleteFile(Marshal.PtrToStringAuto(fileName));

    [UnmanagedCallersOnly(EntryPoint = "FsDeleteFileW")]
    public static bool DeleteFileW(IntPtr fileName) => s_bridge.DeleteFile(Marshal.PtrToStringAuto(fileName));

    [UnmanagedCallersOnly(EntryPoint = "FsRemoveDir")]
    public static bool RemoveDir(IntPtr dirName) => s_bridge.RemoveDirectory(Marshal.PtrToStringAuto(dirName));

    [UnmanagedCallersOnly(EntryPoint = "FsRemoveDirW")]
    public static bool RemoveDirW(IntPtr dirName) => s_bridge.RemoveDirectory(Marshal.PtrToStringAuto(dirName));

    [UnmanagedCallersOnly(EntryPoint = "FsMkDir")]
    public static bool MkDir(IntPtr dirName) => s_bridge.CreateDirectory(Marshal.PtrToStringAuto(dirName));

    [UnmanagedCallersOnly(EntryPoint = "FsMkDirW")]
    public static bool MkDirW(IntPtr dirName) => s_bridge.CreateDirectory(Marshal.PtrToStringAuto(dirName));

    [UnmanagedCallersOnly(EntryPoint = "FsExecuteFile")]
    public static int ExecuteFile(IntPtr mainWin, IntPtr path, IntPtr command)
        => s_bridge.Execute(Marshal.PtrToStringAuto(path), Marshal.PtrToStringAuto(command));

    [UnmanagedCallersOnly(EntryPoint = "FsExecuteFileW")]
    public static int ExecuteFileW(IntPtr mainWin, IntPtr path, IntPtr command)
        => s_bridge.Execute(Marshal.PtrToStringAuto(path), Marshal.PtrToStringAuto(command));
}
