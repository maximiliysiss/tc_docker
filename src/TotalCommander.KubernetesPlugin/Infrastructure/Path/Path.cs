using System;
using System.Diagnostics.CodeAnalysis;
using TotalCommander.KubernetesPlugin.Plugin.Models;
using TotalCommander.Plugin.Infrastructure.Path;

namespace TotalCommander.KubernetesPlugin.Infrastructure.Path;

public sealed record Path(Context? Context, Namespace? Namespace, Pod? Pod, string? LocalPath)
{
    public override string ToString() =>
        $"{Context?.Name ?? "<empty context>"} / {Namespace?.Name ?? "<empty namespace>"} / {Pod?.Name ?? "<empty pod>"} -> {LocalPath ?? "<empty path>"}";

    private const StringSplitOptions Options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    public static Path Parse(string path)
    {
        var parts = path.Split(System.IO.Path.DirectorySeparatorChar, Options);

        Context? context = null;
        if (parts is [var ctx, ..])
            context = new Context(ctx);

        Namespace? ns = null;
        if (parts is [_, var namespaces, ..])
            ns = new Namespace(namespaces);

        Pod? pod = null;
        string? localPath = null;

        if (parts is [_, _, var pods, ..])
        {
            pod = new Pod(pods);
            localPath = "/";

            if (parts is [_, _, _, .. var local] && local.Length > 0)
                localPath += LinuxOs.PathAs(string.Join(System.IO.Path.DirectorySeparatorChar, local));
        }

        return new Path(context, ns, pod, localPath);
    }

    public string? Segment => LocalPath?.Split(Pod is null ? System.IO.Path.DirectorySeparatorChar : '/', Options)[^1];

    [MemberNotNullWhen(true, nameof(Context), nameof(Namespace), nameof(Pod), nameof(LocalPath), nameof(Segment))]
    public bool IsFull => Context is not null && Namespace is not null && Pod is not null && LocalPath is not null;
}
