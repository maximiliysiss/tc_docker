using FluentAssertions;
using TotalCommander.Plugin.Infrastructure.Path;
using Xunit;

namespace TotalCommander.Plugin.UnitTests.Infrastructure.Path;

public class LinuxOsTests
{
    [Fact]
    public void PathAs_ShouldReplaceWindowsSeparators()
    {
        // Act
        var path = LinuxOs.PathAs(@"var\log\app");

        // Assert
        path.Should().Be("var/log/app");
    }
}
