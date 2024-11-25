using System;
using TotalCommander.Plugin.FileSystem.Interface.Extensions.Models;

namespace TotalCommander.DockerPlugin.Plugin.Converter;

public static class CopyResultConverter
{
    public static CopyResult ToCopyResult(this Commander.Docker.Models.CopyResult copyResult)
    {
        return copyResult switch
        {
            Commander.Docker.Models.CopyResult.Success => CopyResult.Success,
            Commander.Docker.Models.CopyResult.Failure => CopyResult.Error,
            Commander.Docker.Models.CopyResult.Exists => CopyResult.Exists,
            _ => throw new ArgumentOutOfRangeException(nameof(copyResult), copyResult, null)
        };
    }
}
