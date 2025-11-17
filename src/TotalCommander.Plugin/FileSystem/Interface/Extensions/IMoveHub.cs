using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;
using TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

namespace TotalCommander.Plugin.FileSystem.Interface.Extensions;

public interface IMoveHub
{
    CopyResult Copy(string source, string destination, bool overwrite, Direction direction);
    CopyResult Move(string source, string destination, bool overwrite, Direction direction);
    CopyResult Rename(string source, string destination, bool overwrite);
}
