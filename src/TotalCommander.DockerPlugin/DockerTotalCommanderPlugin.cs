using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using OY.TotalCommander.TcPluginInterface.FileSystem;
using TotalCommander.DockerPlugin.Adapter.Models;
using TotalCommander.DockerPlugin.Commander;

namespace TotalCommander.DockerPlugin;

public class DockerTotalCommanderPlugin : FsPlugin
{
    private const string _title = "docker";
    private readonly ICommanderExecutor _commandExecutor;

    public DockerTotalCommanderPlugin(StringDictionary pluginSettings) : base(pluginSettings)
    {
        Title = _title;
        _commandExecutor = CommanderBuilder.Build(CommanderType.Linux);
    }

    public override object FindFirst(string path, out FindData findData)
    {
        findData = null;

        var startImageText = path.IndexOf(" [", StringComparison.Ordinal);
        var endImageText = path.IndexOf(']');

        if (startImageText != -1 && endImageText != -1)
            path = path.Remove(startImageText, endImageText - startImageText + 1);

        IEnumerator<TreeElement> elementEnumerator = path == "\\"
            ? _commandExecutor.GetAllContainers().GetEnumerator()
            : _commandExecutor.GetDirectoryContent(path).GetEnumerator();

        if (elementEnumerator.MoveNext() && elementEnumerator.Current != null)
        {
            findData = new FindData(elementEnumerator.Current.ToString(), elementEnumerator.Current.Attributes);
            return elementEnumerator;
        }

        // TODO rebuild TSInterface
        MessageBox.Show("Empty directory", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
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

    public override bool DeleteFile(string fileName) => _commandExecutor.DeleteFile(fileName);

}
