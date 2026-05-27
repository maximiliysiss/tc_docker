using FluentAssertions;
using Xunit;
using DockerPath = TotalCommander.DockerPlugin.Infrastructure.Path.Path;

namespace TotalCommander.Plugin.UnitTests.DockerPlugin.Infrastructure.Path;

public class PathTests
{
    [Fact]
    public void Parse_ShouldReturnContainerRoot_WhenOnlyContainerIsProvided()
    {
        // Act
        var path = DockerPath.Parse(@"\nginx");

        // Assert
        path.Container.Should().NotBeNull();
        path.Container!.Name.Should().Be("nginx");
        path.LocalPath.Should().Be("/");
        path.IsRoot.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldReturnContainerAndLinuxPath_WhenLocalPathIsProvided()
    {
        // Act
        var path = DockerPath.Parse(@"\nginx\etc\hosts");

        // Assert
        path.Container.Should().NotBeNull();
        path.Container!.Name.Should().Be("nginx");
        path.LocalPath.Should().Be("/etc/hosts");
        path.IsRoot.Should().BeFalse();
        path.ToString().Should().Be("nginx -> /etc/hosts");
    }

    [Fact]
    public void Parse_ShouldReturnEmptyContainer_WhenPathIsEmpty()
    {
        // Act
        var path = DockerPath.Parse(string.Empty);

        // Assert
        path.Container.Should().BeNull();
        path.LocalPath.Should().BeEmpty();
        path.IsRoot.Should().BeFalse();
    }
}
