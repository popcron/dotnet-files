using DotNetFiles.Extensions;
using System;
using XML;

namespace DotNetFiles;

public readonly struct ProjectReference
{
    private readonly XMLNode node;

    public readonly ReadOnlySpan<char> Include
    {
        get => node.GetInclude();
        set => node.SetInclude(value);
    }

    public readonly OutputItemType? OutputItemType
    {
        get => node.GetEnum<OutputItemType>(nameof(OutputItemType));
        set => node.SetEnum(nameof(OutputItemType), value);
    }

    public readonly bool? ReferenceOutputAssembly
    {
        get => node.GetBoolean(nameof(ReferenceOutputAssembly));
        set => node.SetBoolean(nameof(ReferenceOutputAssembly), value);
    }

    internal ProjectReference(XMLNode node)
    {
        this.node = node;
    }

    public readonly override string ToString()
    {
        return Include.ToString();
    }
}
