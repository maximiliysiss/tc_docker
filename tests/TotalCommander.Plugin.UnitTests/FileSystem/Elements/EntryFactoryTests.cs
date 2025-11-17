using FluentAssertions;
using TotalCommander.Plugin.FileSystem.Elements;
using TotalCommander.Plugin.FileSystem.Models;
using Xunit;

namespace TotalCommander.Plugin.UnitTests.FileSystem.Elements;

public class EntryFactoryTests
{
    [Fact]
    public void Create_ShouldCreateEntry_WhenItIsSymlink()
    {
        // Arrange
        const string input = "lrwxrwxrwx    1 root     root            12 Oct  8 09:29 acpid -> /bin/busybox*";

        // Act
        var entry = EntryFactory.Create(input);

        // Assert
        entry.Should().BeOfType<File>();
    }

    [Theory]
    [InlineData("-rwxr-xr-x    1 root     root           393 Feb 13  2025 ldconfig*")]
    [InlineData("-rwxr-xr-x    1 root     root         69648 Feb 13  2025 apk*")]
    public void Create_ShouldCreateEntry_WhenItIsExecutable(string input)
    {
        // Arrange

        // Act
        var entry = EntryFactory.Create(input);

        // Assert
        entry.Should().BeOfType<File>();
    }
}
