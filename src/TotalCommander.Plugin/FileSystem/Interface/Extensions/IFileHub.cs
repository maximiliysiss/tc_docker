namespace TotalCommander.Plugin.FileSystem.Interface.Extensions;

public interface IFileHub
{
    void Create(string path);
    void Delete(string path);
    void Open(string path);
}
