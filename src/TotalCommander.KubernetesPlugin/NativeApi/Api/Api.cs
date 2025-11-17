using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TotalCommander.KubernetesPlugin.Plugin;
using TotalCommander.Plugin.FileSystem.Native.Bridge;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

namespace TotalCommander.KubernetesPlugin.NativeApi.Api;

public static class Api
{
    private static readonly Bridge s_bridge = new(new K8sPlugin());

    [UnmanagedCallersOnly(EntryPoint = "FsInit")]
    public static int Init(int pluginNumber, IntPtr progressProc, IntPtr logProc, IntPtr requestProc) => InitInternal();

    [UnmanagedCallersOnly(EntryPoint = "FsInitW")]
    public static int InitW(int pluginNumber, IntPtr progressProcW, IntPtr logProcW, IntPtr requestProcW) => InitInternal();

    private static int InitInternal()
    {
        Trace.Listeners.Add(new TextWriterTraceListener("kubernetes.log") { TraceOutputOptions = TraceOptions.DateTime });

        Trace.AutoFlush = true;

        return 0;
    }

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

    [UnmanagedCallersOnly(EntryPoint = "FsGetFile")]
    public static int GetFile(IntPtr remoteName, IntPtr localName, int copyFlags, IntPtr ri)
        => s_bridge.CopyFile(Marshal.PtrToStringAuto(remoteName), Marshal.PtrToStringAuto(localName), (CopyFlag)copyFlags, Direction.Out);

    [UnmanagedCallersOnly(EntryPoint = "FsGetFileW")]
    public static int GetFileW(IntPtr remoteName, IntPtr localName, int copyFlags, IntPtr ri)
        => s_bridge.CopyFile(Marshal.PtrToStringAuto(remoteName), Marshal.PtrToStringAuto(localName), (CopyFlag)copyFlags, Direction.Out);

    [UnmanagedCallersOnly(EntryPoint = "FsPutFile")]
    public static int CreateFile(IntPtr localName, IntPtr remoteName, int copyFlags)
        => s_bridge.CopyFile(Marshal.PtrToStringAuto(localName), Marshal.PtrToStringAuto(remoteName), (CopyFlag)copyFlags, Direction.In);

    [UnmanagedCallersOnly(EntryPoint = "FsPutFileW")]
    public static int CreateFileW(IntPtr localName, IntPtr remoteName, int copyFlags)
        => s_bridge.CopyFile(Marshal.PtrToStringAuto(localName), Marshal.PtrToStringAuto(remoteName), (CopyFlag)copyFlags, Direction.In);

    [UnmanagedCallersOnly(EntryPoint = "FsRenMovFile")]
    public static int RenMoveFile(IntPtr oldName, IntPtr newName, bool move, bool overwrite, IntPtr ri)
        => RenMoveFileInternal(oldName, newName, move, overwrite);

    [UnmanagedCallersOnly(EntryPoint = "FsRenMovFileW")]
    public static int RenMoveFileW(IntPtr oldName, IntPtr newName, bool move, bool overwrite, IntPtr ri)
        => RenMoveFileInternal(oldName, newName, move, overwrite);

    private static int RenMoveFileInternal(IntPtr oldName, IntPtr newName, bool move, bool overwrite)
    {
        var copyFlag = (move ? CopyFlag.Move : CopyFlag.None) |
                       (overwrite ? CopyFlag.Overwrite : CopyFlag.None);

        return s_bridge.RenameOrMove(Marshal.PtrToStringAuto(oldName), Marshal.PtrToStringAuto(newName), copyFlag);
    }
}
