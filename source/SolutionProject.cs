using System;
using XML;

namespace DotNetFiles;

public readonly struct SolutionProject
{
    private readonly XMLNode node;

    public readonly ReadOnlySpan<char> Path
    {
        get => node.GetAttribute(nameof(Path));
        set => node.SetOrAddAttribute(nameof(Path), value);
    }

    internal SolutionProject(XMLNode node)
    {
        this.node = node;
    }

    public readonly override string ToString()
    {
        return Path.ToString();
    }
}