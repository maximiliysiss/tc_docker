using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;

namespace TotalCommander.Plugin.FileSystem.Interface.Extensions;

public interface IMoveHub
{
    CopyResult Copy(string source, string destination, bool overwrite);
    CopyResult Move(string source, string destination, bool overwrite);
    CopyResult Rename(string source, string destination, bool overwrite);
}
