using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;

namespace TotalCommander.Plugin.FileSystem.Interface.Extensions;

public interface IExecutionHub
{
    ExecuteResult Execute(string path, string command);
}
