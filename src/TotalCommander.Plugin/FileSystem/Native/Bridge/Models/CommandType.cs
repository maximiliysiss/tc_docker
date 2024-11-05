using System;

namespace TotalCommander.Plugin.FileSystem.Native.Bridge.Models;

public readonly struct CommandType : IEquatable<CommandType>
{
    public static readonly CommandType Open = new("open");
    public static readonly CommandType Command = new("quote");

    private readonly string _value;

    public CommandType(string value) => _value = value;

    public bool Equals(CommandType other) => _value == other._value;

    public override bool Equals(object? obj) => obj is CommandType other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(CommandType t1, CommandType t2) => t1.Equals(t2);

    public static bool operator !=(CommandType t1, CommandType t2) => !(t1 == t2);
}
