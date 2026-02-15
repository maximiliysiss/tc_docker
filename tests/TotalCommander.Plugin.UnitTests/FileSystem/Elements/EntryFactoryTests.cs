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
        const string input = "symlink\0lrwxrwxrwx\015";

        // Act
        var entry = EntryFactory.Create(input, '\0');

        // Assert
        entry.Should().BeOfType<File>();
    }

    [Theory]
    [InlineData("executable.sh\0-rwxr-xr-x\030")]
    [InlineData("executable with space.sh\0-rwxr-xr-x\030")]
    [InlineData("executable  with  two  spaces.sh\0-rwxr-xr-x\030")]
    public void Create_ShouldCreateEntry_WhenItIsExecutable(string input)
    {
        // Arrange

        // Act
        var entry = EntryFactory.Create(input, '\0');

        // Assert
        entry.Should().BeOfType<File>();
    }

    [Theory]
    [InlineData("blockdev with space\0brw-r--r--\00", "blockdev with space", typeof(File))]
    [InlineData("blockdev  with  two  spaces\0brw-r--r--\00", "blockdev  with  two  spaces", typeof(File))]
    [InlineData("chardev with space\0crw-r--r--\00", "chardev with space", typeof(File))]
    [InlineData("chardev  with  two  spaces\0crw-r--r--\00", "chardev  with  two  spaces", typeof(File))]
    [InlineData("dir with space\0drwxr-xr-x\04096", "dir with space", typeof(Directory))]
    [InlineData("dir  with  two  spaces\0drwxr-xr-x\04096", "dir  with  two  spaces", typeof(Directory))]
    [InlineData("executable with space.sh\0-rwxr-xr-x\030", "executable with space.sh", typeof(File))]
    [InlineData("executable  with  two  spaces.sh\0-rwxr-xr-x\030", "executable  with  two  spaces.sh", typeof(File))]
    [InlineData("fifo with space\0prw-r--r--\00", "fifo with space", typeof(File))]
    [InlineData("fifo  with  two  spaces\0prw-r--r--\00", "fifo  with  two  spaces", typeof(File))]
    [InlineData("regular with space.txt\0-rw-r--r--\00", "regular with space.txt", typeof(File))]
    [InlineData("regular  with  two  spaces.txt\0-rw-r--r--\00", "regular  with  two  spaces.txt", typeof(File))]
    [InlineData("socket with space\0srwxr-xr-x\00", "socket with space", typeof(File))]
    [InlineData("socket  with  two  spaces\0srwxr-xr-x\00", "socket  with  two  spaces", typeof(File))]
    [InlineData("symlink with space\0lrwxrwxrwx\015", "symlink with space", typeof(File))]
    [InlineData("symlink  with  two  spaces\0lrwxrwxrwx\015", "symlink  with  two  spaces", typeof(File))]
    [InlineData("target file.txt\0-rw-r--r--\00", "target file.txt", typeof(File))]
    public void Create_ShouldCreateEntry_WhenNameHasSpace(string input, string name, Type type)
    {
        // Arrange

        // Act
        var entry = EntryFactory.Create(input, '\0');

        // Assert
        entry.Should().BeOfType(type);
        entry.Name.Should().Be(name);
    }
}
