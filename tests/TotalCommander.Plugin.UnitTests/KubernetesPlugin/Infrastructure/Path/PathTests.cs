using FluentAssertions;
using Xunit;
using K8sPath = TotalCommander.KubernetesPlugin.Infrastructure.Path.Path;

namespace TotalCommander.Plugin.UnitTests.KubernetesPlugin.Infrastructure.Path;

public class PathTests
{
    [Fact]
    public void Parse_ShouldReturnContextOnly_WhenOnlyContextIsProvided()
    {
        // Act
        var path = K8sPath.Parse(@"\kind-cluster");

        // Assert
        path.Context.Should().NotBeNull();
        path.Context!.Name.Should().Be("kind-cluster");
        path.Namespace.Should().BeNull();
        path.Pod.Should().BeNull();
        path.LocalPath.Should().BeNull();
        path.IsRoot.Should().BeTrue();
        path.IsFull.Should().BeFalse();
        path.Segment.Should().BeNull();
    }

    [Fact]
    public void Parse_ShouldReturnPodRoot_WhenContextNamespaceAndPodAreProvided()
    {
        // Act
        var path = K8sPath.Parse(@"\kind-cluster\default\api-123");

        // Assert
        path.Context!.Name.Should().Be("kind-cluster");
        path.Namespace!.Name.Should().Be("default");
        path.Pod!.Name.Should().Be("api-123");
        path.LocalPath.Should().Be("/");
        path.IsRoot.Should().BeTrue();
        path.IsFull.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldReturnFullLinuxPath_WhenLocalPathIsProvided()
    {
        // Act
        var path = K8sPath.Parse(@"\kind-cluster\default\api-123\var\log\app.log");

        // Assert
        path.Context!.Name.Should().Be("kind-cluster");
        path.Namespace!.Name.Should().Be("default");
        path.Pod!.Name.Should().Be("api-123");
        path.LocalPath.Should().Be("/var/log/app.log");
        path.Segment.Should().Be("app.log");
        path.IsRoot.Should().BeFalse();
        path.IsFull.Should().BeTrue();
        path.ToString().Should().Be("kind-cluster / default / api-123 -> /var/log/app.log");
    }
}
