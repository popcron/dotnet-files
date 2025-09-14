using DotNetFiles.Extensions;
using System;
using XML;

namespace DotNetFiles;

public readonly struct Content
{
    private readonly XMLNode node;

    public readonly ReadOnlySpan<char> Include
    {
        get => node.GetInclude();
        set => node.SetInclude(value);
    }

    public readonly bool? Pack
    {
        get => node.GetBoolean(nameof(Pack));
        set => node.SetBoolean(nameof(Pack), value);
    }

    public readonly ReadOnlySpan<char> PackagePath
    {
        get
        {
            if (node.TryGetAttribute(nameof(PackagePath), out ReadOnlySpan<char> packagePath))
            {
                return packagePath;
            }

            return default;
        }
        set => node.SetOrAddAttribute(nameof(PackagePath), value);
    }

    public readonly bool? Visible
    {
        get => node.GetBoolean(nameof(Visible));
        set => node.SetBoolean(nameof(Visible), value);
    }

    internal Content(XMLNode node)
    {
        this.node = node;
    }

    public readonly override string ToString()
    {
        return Include.ToString();
    }
}
