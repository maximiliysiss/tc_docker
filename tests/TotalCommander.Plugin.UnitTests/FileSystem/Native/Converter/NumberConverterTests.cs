using FluentAssertions;
using TotalCommander.Plugin.FileSystem.Native.Converter;
using Xunit;

namespace TotalCommander.Plugin.UnitTests.FileSystem.Native.Converter;

public class NumberConverterTests
{
    [Fact]
    public void GetHighAndGetLow_ShouldSplitSignedLong()
    {
        // Arrange
        const long value = unchecked((long)0x12345678_90ABCDEF);

        // Act
        var high = NumberConverter.GetHigh(value);
        var low = NumberConverter.GetLow(value);

        // Assert
        high.Should().Be(0x12345678);
        low.Should().Be(unchecked((int)0x90ABCDEF));
    }

    [Fact]
    public void GetHighAndGetLow_ShouldSplitUnsignedLong()
    {
        // Arrange
        const ulong value = 0xFEDCBA98_76543210;

        // Act
        var high = NumberConverter.GetHigh(value);
        var low = NumberConverter.GetLow(value);

        // Assert
        high.Should().Be(0xFEDCBA98);
        low.Should().Be(0x76543210);
    }
}
