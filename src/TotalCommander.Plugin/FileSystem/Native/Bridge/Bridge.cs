using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TotalCommander.Plugin.FileSystem.Interface;
using TotalCommander.Plugin.FileSystem.Interface.Extensions;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Infrastructure;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;
using TotalCommander.Plugin.Shared.Infrastructure.Logger;

namespace TotalCommander.Plugin.FileSystem.Native.Bridge;

public sealed class Bridge(IFileSystemPlugin plugin)
{
    private readonly ILogger _logger = new TraceLogger("default.log");

    private readonly ConcurrentDictionary<IntPtr, IEnumerator<FileSystem.Models.Entry>> _handles = new();

    public IntPtr FindFirst(string? path, IntPtr findFile, bool isUnicode)
    {
        if (string.IsNullOrEmpty(path))
            return IntPtr.Zero;

        _logger.Log($"Try to find in '{path}'");

        var enumerator = plugin
            .EnumerateEntries(path)
            .GetEnumerator();

        if (enumerator.MoveNext() is false)
        {
            NativeMethods.SetLastError(NativeConstants.NoContent);
            return NativeConstants.InvalidHandle;
        }

        var handle = new IntPtr(enumerator.GetHashCode());

        _handles[handle] = enumerator;

        enumerator.Current.AsNative().CopyTo(findFile, isUnicode);

        return handle;
    }

    public bool FindNext(IntPtr hdl, IntPtr findFile, bool isUnicode)
    {
        if (_handles.TryGetValue(hdl, out var enumerator) is false)
            return false;

        if (enumerator.MoveNext() is false)
            return false;

        enumerator.Current.AsNative().CopyTo(findFile, isUnicode);

        return true;
    }

    public int FindClose(IntPtr hdl)
    {
        if (_handles.TryRemove(hdl, out var enumerator))
            enumerator.Dispose();

        return 0;
    }

    public bool DeleteFile(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        if (plugin is not IFileHub hub)
            return false;

        _logger.Log($"Deleting file '{fileName}'");

        hub.Delete(fileName);

        return true;
    }

    public bool RemoveDirectory(string? dirPath)
    {
        if (string.IsNullOrEmpty(dirPath))
            return false;

        if (plugin is not IDirectoryHub hub)
            return false;

        _logger.Log($"Deleting directory '{dirPath}'");

        hub.Delete(dirPath);

        return true;
    }

    public bool CreateDirectory(string? dirPath)
    {
        if (string.IsNullOrEmpty(dirPath))
            return false;

        if (plugin is not IDirectoryHub hub)
            return false;

        _logger.Log($"Creating directory '{dirPath}'");

        hub.Create(dirPath);

        return true;
    }

    public int Execute(string? path, string? verb)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(verb))
            return 0;

        _logger.Log($"Execution of '{verb}' in '{path}'");

        var parts = verb.Split(
            separator: " ",
            count: 2,
            options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var commandType = new CommandType(parts[0]);
        var command = parts[^1];

        return commandType switch
        {
            _ when commandType == CommandType.Command => (int?)(plugin as IExecutionHub)?.Execute(path, command) ?? 0,
            _ when commandType == CommandType.Open => Open(),
            _ => 0
        };

        int Open()
        {
            (plugin as IFileHub)?.Open(path);
            return 0;
        }
    }
}
