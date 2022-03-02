using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using OY.TotalCommander.TcPluginInterface;
using OY.TotalCommander.TcPluginInterface.FileSystem;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.Commander;

namespace TotalCommander.DockerPlugin;

public class DockerTotalCommanderPlugin : FsPlugin
{
    private const string PluginTitle = "docker";
    private const string EmptyStub = "_empty.stub";

    private readonly ICommanderExecutor _commandExecutor;

    public DockerTotalCommanderPlugin(StringDictionary pluginSettings) : base(pluginSettings)
    {
        Title = PluginTitle;

        _commandExecutor = CommanderBuilder.Build(CommanderType.Linux);
    }

    private static string AsRealPath(string path)
    {
        var startImageText = path.IndexOf(" [", StringComparison.Ordinal);
        var endImageText = path.IndexOf(']');

        if (startImageText != -1 && endImageText != -1)
            path = path.Remove(startImageText, endImageText - startImageText + 1);

        return path;
    }

    public override object FindFirst(string path, out FindData findData)
    {
        findData = null;

        path = AsRealPath(path);

        var elementEnumerator = path == "\\"
            ? _commandExecutor.GetAllContainers().GetEnumerator()
            : _commandExecutor.GetDirectoryContent(path).GetEnumerator();

        if (elementEnumerator.MoveNext() && elementEnumerator.Current != null)
        {
            findData = new FindData(elementEnumerator.Current.ToString(), elementEnumerator.Current.Attributes);
            return elementEnumerator;
        }

        findData = new FindData(EmptyStub, FileAttributes.Hidden | FileAttributes.ReadOnly);
        return Array.Empty<TreeElement>().GetEnumerator();
    }

    public override bool FindNext(ref object o, out FindData findData)
    {
        findData = null;
        if (o is not IEnumerator enumerator)
            return false;

        if (!enumerator.MoveNext())
            return false;

        if (enumerator.Current is not TreeElement treeElement)
            return false;

        findData = new FindData(treeElement.ToString(), treeElement.Attributes);

        return true;
    }

    public override bool DeleteFile(string fileName)
    {
        if (fileName.EndsWith(EmptyStub))
            return true;

        return _commandExecutor.DeleteFile(AsRealPath(fileName));
    }

    public override bool RemoveDir(string dirName) => _commandExecutor.RemoveDir(AsRealPath(dirName));

    public override bool MkDir(string dir) => _commandExecutor.MkDir(AsRealPath(dir));

    public override FileSystemExitCode RenMovFile(string oldName, string newName, bool move, bool overwrite, RemoteInfo remoteInfo)
    {
        _commandExecutor.RenMovFile(AsRealPath(oldName), AsRealPath(newName), move, overwrite);
        return FileSystemExitCode.OK;
    }

    public override ExecResult ExecuteCommand(TcWindow mainWin, ref string remoteName, string command)
    {
        _commandExecutor.ExecuteCommand(AsRealPath(remoteName), command);
        return ExecResult.OkReread;
    }

    public override ExecResult ExecuteOpen(TcWindow mainWin, ref string remoteName)
    {
        _commandExecutor.OpenFile(AsRealPath(remoteName));
        return ExecResult.OK;
    }

    public override PreviewBitmapResult GetPreviewBitmap(ref string remoteName, int width, int height, out Bitmap returnedBitmap)
    {
        return base.GetPreviewBitmap(ref remoteName, width, height, out returnedBitmap);
    }
}
