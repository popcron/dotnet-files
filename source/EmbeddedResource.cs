using DotNetFiles.Extensions;
using System;
using XML;

namespace DotNetFiles;

public readonly struct EmbeddedResource
{
    private readonly XMLNode node;

    public readonly ReadOnlySpan<char> Include
    {
        get => node.GetInclude();
        set => node.SetInclude(value);
    }

    internal EmbeddedResource(XMLNode node)
    {
        this.node = node;
    }

    public readonly override string ToString()
    {
        return Include.ToString();
    }
}
