using System;
using FluentAssertions;
using TotalCommander.Plugin.FileSystem.Native.Converter;
using Xunit;

namespace TotalCommander.Plugin.UnitTests.FileSystem.Native.Converter;

public class FileTimeConverterTests
{
    [Fact]
    public void ToFileTime_ShouldConvertDateTime_WhenValueIsProvided()
    {
        // Arrange
        var dateTime = new DateTime(2026, 5, 27, 12, 34, 56, DateTimeKind.Utc);

        // Act
        var fileTime = ((DateTime?)dateTime).ToFileTime();

        // Assert
        fileTime.dwHighDateTime.Should().Be(NumberConverter.GetHigh(dateTime.ToFileTime()));
        fileTime.dwLowDateTime.Should().Be(NumberConverter.GetLow(dateTime.ToFileTime()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0001-01-01T00:00:00.0000000")]
    public void ToFileTime_ShouldUseEmptyFileTime_WhenValueIsMissingOrMinValue(string? value)
    {
        // Arrange
        var dateTime = value is null ? (DateTime?)null : DateTime.Parse(value);
        var emptyFileTime = long.MaxValue << 1;

        // Act
        var fileTime = dateTime.ToFileTime();

        // Assert
        fileTime.dwHighDateTime.Should().Be(NumberConverter.GetHigh(emptyFileTime));
        fileTime.dwLowDateTime.Should().Be(NumberConverter.GetLow(emptyFileTime));
    }
}
