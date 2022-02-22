using System.Collections.Specialized;
using System.Windows.Forms;
using OY.TotalCommander.TcPluginInterface.FileSystem;
using TotalCommander.DockerPlugin.Commander;

namespace TotalCommander.DockerPlugin
{
    public class DockerTotalCommanderPlugin : FsPlugin
    {
        private const string _title = "Docker & KB";
        private readonly ICommanderExecutor _commandExecutor;

        public DockerTotalCommanderPlugin(StringDictionary pluginSettings) : base(pluginSettings)
        {
            Title = _title;
            _commandExecutor = CommanderBuilder.Build(CommanderType.Linux);
        }

        public override object FindFirst(string path, out FindData findData)
        {
            MessageBox.Show(path, "Find first path");

            findData = null;
            return null;
        }

        public override bool FindNext(ref object o, out FindData findData)
        {
            MessageBox.Show(o.ToString(), "Find first path");

            findData = null;
            return false;
        }
    }
}
