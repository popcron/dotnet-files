using System;
using System.IO;
using System.Text;

namespace DotNetFiles.Tests;

public static class Samples
{
    public static string Get(string name)
    {
        using Stream stream = GetResourceStream(name);
        Span<byte> buffer = stackalloc byte[(int)stream.Length];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }

    public static void Get(string name, StringBuilder builder)
    {
        using Stream stream = GetResourceStream(name);
        Span<byte> buffer = stackalloc byte[(int)stream.Length];
        stream.ReadExactly(buffer);
        builder.Append(Encoding.UTF8.GetString(buffer));
    }

    public static Stream GetResourceStream(string name)
    {
        ThrowIfResourceDoesntExist(name);
        int index = IndexOfResource(name);
        string resourceName = typeof(Tests).Assembly.GetManifestResourceNames()[index];
        return typeof(Tests).Assembly.GetManifestResourceStream(resourceName)!;
    }

    private static int IndexOfResource(string name)
    {
        string[] resourceNames = typeof(Tests).Assembly.GetManifestResourceNames();
        for (int i = 0; i < resourceNames.Length; i++)
        {
            string resourceName = resourceNames[i];
            for (int c = 0; c < resourceName.Length; c++)
            {
                char resourceNameCharacter = resourceName[resourceName.Length - 1 - c];
                char nameCharacter = name[name.Length - 1 - c];
                if (resourceNameCharacter != nameCharacter)
                {
                    if (resourceNameCharacter == '.' && nameCharacter == '/')
                    {
                        continue;
                    }
                    else if (resourceNameCharacter == '_' && nameCharacter == ' ')
                    {
                        continue;
                    }

                    break;
                }

                if (c == name.Length - 1)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static void ThrowIfResourceDoesntExist(string name)
    {
        if (IndexOfResource(name) == -1)
        {
            throw new NullReferenceException($"Resource `{name}` does not exist");
        }
    }
}