using System;
using System.Runtime.InteropServices.ComTypes;

namespace TotalCommander.Plugin.FileSystem.Native.Converter;

public static class FileTimeConverter
{
    public static FILETIME ToFileTime(this DateTime? dateTime)
    {
        var longTime =
            dateTime.HasValue && dateTime.Value != DateTime.MinValue
                ? dateTime.Value.ToFileTime()
                : long.MaxValue << 1;

        return new FILETIME
        {
            dwHighDateTime = NumberConverter.GetHigh(longTime),
            dwLowDateTime = NumberConverter.GetLow(longTime)
        };
    }
}
