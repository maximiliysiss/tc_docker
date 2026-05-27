using System;
using System.IO;
using System.Runtime.InteropServices;
using FluentAssertions;
using TotalCommander.Plugin.FileSystem.Native.Models;
using Xunit;

namespace TotalCommander.Plugin.UnitTests.FileSystem.Native.Models;

public class EntryTests
{
    [Fact]
    public void CopyTo_ShouldDoNothing_WhenPointerIsZero()
    {
        // Arrange
        var entry = new Entry("file.txt", FileAttributes.Normal, 10);

        // Act
        var act = () => entry.CopyTo(IntPtr.Zero, isUnicode: true);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CopyTo_ShouldCopyUnicodeFindData_WhenUnicodeRequested()
    {
        // Arrange
        var lastWriteTime = new DateTime(2026, 5, 27, 10, 11, 12, DateTimeKind.Utc);
        var entry = new Entry(
            "unicode-file.txt",
            FileAttributes.Archive,
            0x12345678_90ABCDEF,
            creationTime: null,
            lastAccessTime: null,
            lastWriteTime: lastWriteTime);
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<UnicodeTcFindData>());

        try
        {
            // Act
            entry.CopyTo(pointer, isUnicode: true);
            var findData = Marshal.PtrToStructure<UnicodeTcFindData>(pointer);

            // Assert
            findData.FileName.Should().Be("unicode-file.txt");
            findData.AlternateFileName.Should().BeEmpty();
            findData.FileAttributes.Should().Be((int)FileAttributes.Archive);
            findData.FileSizeHigh.Should().Be(0x12345678);
            findData.FileSizeLow.Should().Be(0x90ABCDEF);
            findData.LastWriteTime.dwHighDateTime.Should().NotBe(0);
            findData.LastWriteTime.dwLowDateTime.Should().NotBe(0);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void CopyTo_ShouldCopyAnsiFindData_WhenAnsiRequested()
    {
        // Arrange
        var entry = new Entry("ansi-file.txt", FileAttributes.Normal, 512);
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<AnsiTcFindData>());

        try
        {
            // Act
            entry.CopyTo(pointer, isUnicode: false);
            var findData = Marshal.PtrToStructure<AnsiTcFindData>(pointer);

            // Assert
            findData.FileName.Should().Be("ansi-file.txt");
            findData.AlternateFileName.Should().BeEmpty();
            findData.FileAttributes.Should().Be((int)FileAttributes.Normal);
            findData.FileSizeHigh.Should().Be(0);
            findData.FileSizeLow.Should().Be(512);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }
}
