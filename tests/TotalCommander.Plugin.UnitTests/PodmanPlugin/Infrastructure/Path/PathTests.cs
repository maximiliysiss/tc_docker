using FluentAssertions;
using Xunit;
using PodmanPath = TotalCommander.PodmanPlugin.Infrastructure.Path.Path;

namespace TotalCommander.Plugin.UnitTests.PodmanPlugin.Infrastructure.Path;

public class PathTests
{
    [Fact]
    public void Parse_ShouldReturnContainerRoot_WhenOnlyContainerIsProvided()
    {
        // Act
        var path = PodmanPath.Parse(@"\postgres");

        // Assert
        path.Container.Should().NotBeNull();
        path.Container!.Name.Should().Be("postgres");
        path.LocalPath.Should().Be("/");
        path.IsRoot.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldReturnContainerAndLinuxPath_WhenLocalPathIsProvided()
    {
        // Act
        var path = PodmanPath.Parse(@"\postgres\var\lib\data");

        // Assert
        path.Container.Should().NotBeNull();
        path.Container!.Name.Should().Be("postgres");
        path.LocalPath.Should().Be("/var/lib/data");
        path.IsRoot.Should().BeFalse();
        path.ToString().Should().Be("postgres -> /var/lib/data");
    }
}
