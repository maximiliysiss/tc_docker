using System.Linq;
using TotalCommander.KubernetesPlugin.Plugin.Models;

namespace TotalCommander.KubernetesPlugin.Infrastructure.Path;

public record struct Environment(Context[] Contexts, Namespace[] Namespaces, Pod[] Pods)
{
    public bool Has(Path path)
    {
        return Contexts.Any(c => c.Name == path.Context?.Name) ||
               Namespaces.Any(n => n.Name == path.Namespace?.Name) ||
               Pods.Any(p => p.Name == path.Pod?.Name);
    }
}
