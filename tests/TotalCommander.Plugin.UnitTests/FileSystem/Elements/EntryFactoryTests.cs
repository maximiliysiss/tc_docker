using System;
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

    [Theory]
    [InlineData("drwxr-xr-x 2 root root 4096 Dec 25 10:41 dir_regular/", "dir_regular", typeof(Directory))]
    [InlineData("drwxr-xr-x 2 root root 4096 Dec 25 10:41 'dir regular'/", "dir regular", typeof(Directory))]
    [InlineData("prw-r--r-- 1 root root    0 Dec 25 10:42 'fifo regular'|", "fifo regular", typeof(File))]
    [InlineData("prw-r--r-- 1 root root    0 Dec 25 10:42 fifo_regular|", "fifo_regular", typeof(File))]
    [InlineData("-rw-r--r-- 1 root root    0 Dec 25 10:40 'file regular'", "file regular", typeof(File))]
    [InlineData("-rw-r--r-- 1 root root    0 Dec 25 10:40 file_regular", "file_regular", typeof(File))]
    [InlineData("-rw-r--r-- 3 root root    0 Dec 25 10:40 'hardlink regular'", "hardlink regular", typeof(File))]
    [InlineData("-rw-r--r-- 3 root root    0 Dec 25 10:40  hardlink_regular", "hardlink_regular", typeof(File))]
    [InlineData("-rw-r--r-- 3 root root    0 Dec 25 10:40 'symlink regular'", "symlink regular", typeof(File))]
    [InlineData("-rw-r--r-- 3 root root    0 Dec 25 10:40  symlink_regular", "symlink_regular", typeof(File))]
    public void Create_ShouldCreateEntry_WhenNameHasSpace(string input, string name, Type type)
    {
        // Arrange

        // Act
        var entry = EntryFactory.Create(input);

        // Assert
        entry.Should().BeOfType(type);
        entry.Name.Should().Be(name);
    }
}
