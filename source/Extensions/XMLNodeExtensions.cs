using System;
using XML;

namespace DotNetFiles.Extensions;

internal static class XMLNodeExtensions
{
    public const string IncludeAttribute = "Include";

    public static ReadOnlySpan<char> GetInclude(this XMLNode node)
    {
        if (node.TryGetAttribute(IncludeAttribute, out ReadOnlySpan<char> include))
        {
            return include;
        }

        return default;
    }

    public static void SetInclude(this XMLNode node, ReadOnlySpan<char> include)
    {
        if (include.IsEmpty)
        {
            node.TryRemoveAttribute(IncludeAttribute);
        }
        else
        {
            node.SetAttribute(IncludeAttribute, include);
        }
    }

    public static bool TryGetAttributeOrChild(this XMLNode node, ReadOnlySpan<char> name, out ReadOnlySpan<char> found)
    {
        // check attribute first
        if (node.TryGetAttribute(name, out ReadOnlySpan<char> attributeValue))
        {
            found = attributeValue;
            return true;
        }

        // then child node
        if (node.TryGetFirst(name, out XMLNode childNode))
        {
            found = childNode.Content.AsSpan();
            return true;
        }

        found = default;
        return false;
    }

    public static bool? GetBoolean(this XMLNode node, ReadOnlySpan<char> name)
    {
        if (node.TryGetAttributeOrChild(name, out ReadOnlySpan<char> value))
        {
            if (value.SequenceEqual("true"))
            {
                return true;
            }
            else if (value.SequenceEqual("false"))
            {
                return false;
            }
            else
            {
                throw new FormatException($"Invalid boolean value `{value.ToString()}`");
            }
        }

        return null;
    }

    public static void SetBoolean(this XMLNode node, ReadOnlySpan<char> name, bool? value)
    {
        if (node.ContainsAttribute(name))
        {
            if (value is bool nonNullValue)
            {
                node.SetAttribute(name, nonNullValue ? "true" : "false");
            }
            else
            {
                node.TryRemoveAttribute(name);
            }
        }
        else
        {
            if (node.TryGetFirst(name, out XMLNode childNode))
            {
                if (value is bool notNullValue)
                {
                    childNode.Content.CopyFrom(notNullValue ? "true" : "false");
                }
                else
                {
                    node.TryRemove(childNode);
                }
            }
            else
            {
                if (value is bool notNullValue)
                {
                    // prefer attribute
                    node.SetAttribute(name, notNullValue ? "true" : "false");
                }
            }
        }
    }

    public static T? GetEnum<T>(this XMLNode node, ReadOnlySpan<char> name) where T : struct, Enum
    {
        if (node.TryGetAttributeOrChild(name, out ReadOnlySpan<char> value))
        {
            if (Enum.TryParse<T>(value.ToString(), out T result))
            {
                return result;
            }
            else
            {
                throw new FormatException($"Invalid {typeof(T).Name} value `{value.ToString()}`");
            }
        }

        return null;
    }

    public static void SetEnum<T>(this XMLNode node, ReadOnlySpan<char> name, T? value) where T : struct, Enum
    {
        // try to assign as attribute first, then as a child node
        if (node.ContainsAttribute(name))
        {
            if (value is T notNullValue)
            {
                node.SetAttribute(name, notNullValue.ToString());
            }
            else
            {
                node.TryRemoveAttribute(name);
            }
        }
        else
        {
            if (node.TryGetFirst(name, out XMLNode childNode))
            {
                if (value is T notNullValue)
                {
                    childNode.Content.CopyFrom(notNullValue.ToString());
                }
                else
                {
                    node.TryRemove(childNode);
                }
            }
            else
            {
                if (value is T notNullValue)
                {
                    // prefer attribute
                    node.SetAttribute(name, notNullValue.ToString());
                }
            }
        }
    }
}
