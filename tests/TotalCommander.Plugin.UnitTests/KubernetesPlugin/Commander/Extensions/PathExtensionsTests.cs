using System;
using FluentAssertions;
using TotalCommander.KubernetesPlugin.Commander.Extensions;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using Xunit;
using K8sPath = TotalCommander.KubernetesPlugin.Infrastructure.Path.Path;

namespace TotalCommander.Plugin.UnitTests.KubernetesPlugin.Commander.Extensions;

public class PathExtensionsTests
{
    [Fact]
    public void Execute_ShouldReturnKubectlExecArguments()
    {
        // Arrange
        var path = new K8sPath(new Context("kind"), new Namespace("default"), new Pod("api"), "/app");

        // Act
        var args = path.Execute();

        // Assert
        args.Should().Equal("--context", "kind", "--namespace", "default", "exec", "api", "--");
    }

    [Fact]
    public void Execute_ShouldThrow_WhenPathIsNotFull()
    {
        // Arrange
        var path = new K8sPath(null, null, null, null);

        // Act
        var act = () => path.Execute();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
