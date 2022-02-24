using System;
using TotalCommander.DockerPlugin.Commander.Executors;

namespace TotalCommander.DockerPlugin.Commander;

public enum CommanderType
{
    Linux,
    Windows
}

public class CommanderBuilder
{
    public static ICommanderExecutor Build(CommanderType commanderType)
    {
        switch (commanderType)
        {
            case CommanderType.Linux:
                return new LinuxCommanderExecutor();
            case CommanderType.Windows:
                return new WindowsCommanderExecutor();
            default:
                throw new ArgumentOutOfRangeException(nameof(commanderType), commanderType, null);
        }
    }
}