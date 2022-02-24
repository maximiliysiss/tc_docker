using System.Collections.Specialized;
using OY.TotalCommander.TcPluginInterface.FileSystem;

namespace TotalCommander.KubernetesPlugin;

public class KubernetesTotalCommanderPlugin : FsPlugin
{
    public KubernetesTotalCommanderPlugin(StringDictionary pluginSettings) : base(pluginSettings)
    {
    }

    public override object FindFirst(string path, out FindData findData) => base.FindFirst(path, out findData);
    public override bool FindNext(ref object o, out FindData findData) => base.FindNext(ref o, out findData);
}
