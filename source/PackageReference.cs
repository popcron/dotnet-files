using DotNetFiles.Extensions;
using System;
using XML;

namespace DotNetFiles;

public readonly struct PackageReference
{
    private readonly XMLNode node;

    public readonly ReadOnlySpan<char> Include
    {
        get => node.GetInclude();
        set => node.SetInclude(value);
    }

    public readonly SemanticVersion? Version
    {
        get
        {
            if (node.TryGetAttribute(nameof(Version), out ReadOnlySpan<char> versionText))
            {
                return SemanticVersion.Parse(versionText);
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (value is SemanticVersion version)
            {
                node.SetOrAddAttribute(nameof(Version), version.ToString());
            }
            else
            {
                node.TryRemoveAttribute(nameof(Version));
            }
        }
    }

    public readonly Assets? IncludeAssets
    {
        get => GetAssets(nameof(IncludeAssets));
        set => SetAssets(nameof(IncludeAssets), value);
    }

    public readonly Assets? ExcludeAssets
    {
        get => GetAssets(nameof(ExcludeAssets));
        set => SetAssets(nameof(ExcludeAssets), value);
    }

    public readonly Assets? PrivateAssets
    {
        get => GetAssets(nameof(PrivateAssets));
        set => SetAssets(nameof(PrivateAssets), value);
    }

    internal PackageReference(XMLNode node)
    {
        this.node = node;
    }

    public readonly override string ToString()
    {
        return $"{Include} {Version}";
    }

    private Assets? GetAssets(ReadOnlySpan<char> name)
    {
        if (node.TryGetAttributeOrChild(name, out ReadOnlySpan<char> assetsText))
        {
            if (assetsText.IndexOf(';') != -1)
            {
                int start = 0;
                int index = 0;
                Assets includeAssets = default;
                while (index < assetsText.Length)
                {
                    char c = assetsText[index];
                    if (c == ';')
                    {
                        ReadOnlySpan<char> part = assetsText[start..index];
                        if (Enum.TryParse(part, true, out Assets partAssets))
                        {
                            includeAssets |= partAssets;
                        }
                        else
                        {
                            throw new FormatException($"Invalid value `{part.ToString()}`");
                        }

                        start = index + 1;
                    }
                    else if (index == assetsText.Length - 1)
                    {
                        ReadOnlySpan<char> part = assetsText[start..];
                        if (Enum.TryParse(part, true, out Assets partAssets))
                        {
                            includeAssets |= partAssets;
                        }
                        else
                        {
                            throw new FormatException($"Invalid value `{part.ToString()}`");
                        }
                    }

                    index++;
                }

                return includeAssets;
            }
            else
            {
                if (Enum.TryParse(assetsText, true, out Assets includeAssets))
                {
                    return includeAssets;
                }
                else
                {
                    throw new FormatException($"Invalid value `{assetsText.ToString()}`");
                }
            }
        }
        else
        {
            return null;
        }
    }

    private void SetAssets(ReadOnlySpan<char> name, Assets? value)
    {
        if (node.ContainsAttribute(name))
        {
            if (value is Assets notNullValue)
            {
                node.SetOrAddAttribute(name, notNullValue.ToString().Replace(", ", ";"));
            }
            else
            {
                node.TryRemoveAttribute(name);
            }
        }
        else
        {
            if (value is Assets notNullValue)
            {
                if (!node.TryGetFirstChild(name, out XMLNode assetsChild))
                {
                    assetsChild = new XMLNode(name.ToString());
                    node.AddChild(assetsChild);
                }

                assetsChild.Content.CopyFrom(notNullValue.ToString().Replace(", ", ";"));
            }
            else
            {
                node.TryRemoveChild(name);
            }
        }
    }
}