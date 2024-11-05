using System;
using System.IO;
using System.Runtime.InteropServices;
using TotalCommander.Plugin.FileSystem.Native.Converter;

namespace TotalCommander.Plugin.FileSystem.Native.Models;

public readonly record struct Entry
{
    private readonly string _name;

    private readonly FileAttributes _attributes;

    private readonly ulong _size;

    private readonly DateTime? _creationTime;
    private readonly DateTime? _lastAccessTime;
    private readonly DateTime? _lastWriteTime;

    public Entry(
        string name,
        FileAttributes attributes,
        ulong size,
        DateTime? creationTime,
        DateTime? lastAccessTime,
        DateTime? lastWriteTime)
    {
        _name = name;
        _attributes = attributes;
        _size = size;
        _creationTime = creationTime;
        _lastAccessTime = lastAccessTime;
        _lastWriteTime = lastWriteTime;
    }

    public Entry(string name, FileAttributes attributes)
    {
        _name = name;
        _attributes = attributes;
    }

    public Entry(string name, FileAttributes attributes, ulong size)
    {
        _name = name;
        _attributes = attributes;
        _size = size;
    }

    public void CopyTo(IntPtr findFile, bool isUnicode)
    {
        if (findFile == IntPtr.Zero)
            return;

        if (isUnicode)
            AsUnicode(findFile);
        else
            AsAnsi(findFile);
    }

    private void AsAnsi(IntPtr findFile)
    {
        var findData = new AnsiTcFindData
        {
            CreationTime = _creationTime.ToFileTime(),
            FileAttributes = (int)_attributes,
            FileName = _name,
            AlternateFileName = string.Empty,
            FileSizeHigh = NumberConverter.GetHigh(_size),
            FileSizeLow = NumberConverter.GetLow(_size),
            LastAccessTime = _lastAccessTime.ToFileTime(),
            LastWriteTime = _lastWriteTime.ToFileTime()
        };

        Marshal.StructureToPtr(
            structure: findData,
            ptr: findFile,
            fDeleteOld: false);
    }

    private void AsUnicode(IntPtr findFile)
    {
        var findData = new UnicodeTcFindData
        {
            CreationTime = _creationTime.ToFileTime(),
            FileAttributes = (int)_attributes,
            FileName = _name,
            AlternateFileName = string.Empty,
            FileSizeHigh = NumberConverter.GetHigh(_size),
            FileSizeLow = NumberConverter.GetLow(_size),
            LastAccessTime = _lastAccessTime.ToFileTime(),
            LastWriteTime = _lastWriteTime.ToFileTime()
        };

        Marshal.StructureToPtr(
            structure: findData,
            ptr: findFile,
            fDeleteOld: false);
    }
}
