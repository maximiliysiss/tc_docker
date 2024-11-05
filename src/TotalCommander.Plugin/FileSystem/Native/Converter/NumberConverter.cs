namespace TotalCommander.Plugin.FileSystem.Native.Converter;

public static class NumberConverter
{
    public static int GetHigh(long value) => (int)(value >> 32);

    public static int GetLow(long value) => (int)(value & uint.MaxValue);

    public static uint GetHigh(ulong value) => (uint)(value >> 32);

    public static uint GetLow(ulong value) => (uint)(value & uint.MaxValue);
}
