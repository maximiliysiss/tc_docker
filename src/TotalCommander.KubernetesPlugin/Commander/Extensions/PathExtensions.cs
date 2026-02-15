using System;
using System.Collections.Generic;
using TotalCommander.KubernetesPlugin.Infrastructure.Path;

namespace TotalCommander.KubernetesPlugin.Commander.Extensions;

internal static class PathExtensions
{
    public static IEnumerable<string> Execute(this Path path)
    {
        ArgumentNullException.ThrowIfNull(path.Context);
        ArgumentNullException.ThrowIfNull(path.Pod);
        ArgumentNullException.ThrowIfNull(path.Namespace);

        return
        [
            "--context",
            path.Context.Name,
            "--namespace",
            path.Namespace.Name,
            "exec",
            path.Pod.Name,
            "--",
        ];
    }
}
