using Collections.Generic;
using System;
using Unmanaged;
using XML;

namespace DotNetFiles;

public struct Solution : IDisposable, ISerializable
{
    private XMLNode rootNode;

    [Obsolete("Default constructor not supported", true)]
    public Solution() { }

    public Solution(ReadOnlySpan<byte> xmlBytes)
    {
        using ByteReader byteReader = new(xmlBytes);
        rootNode = byteReader.ReadObject<XMLNode>();
    }

    public readonly void Dispose()
    {
        rootNode.Dispose();
    }

    public readonly override string ToString()
    {
        SerializationSettings settings = SerializationSettings.PrettyPrinted;
        settings.flags |= SerializationSettings.Flags.SkipEmptyNodes;
        settings.flags |= SerializationSettings.Flags.SpaceBeforeClosingNode;
        return rootNode.ToString(settings);
    }

    readonly void ISerializable.Write(ByteWriter byteWriter)
    {
        byteWriter.WriteObject(rootNode);
    }

    void ISerializable.Read(ByteReader byteReader)
    {
        rootNode = byteReader.ReadObject<XMLNode>();
    }

    public readonly void ClearProjects()
    {
        ReadOnlySpan<XMLNode> children = rootNode.Children;
        for (int i = children.Length - 1; i >= 0; i--)
        {
            if (children[i].Name.Equals("Project"))
            {
                rootNode.RemoveChildAt(i);
            }
        }
    }

    public readonly bool ContainsProject(ReadOnlySpan<char> path)
    {
        foreach (XMLNode projectNode in rootNode.Children)
        {
            if (projectNode.Name.Equals("Project"))
            {
                SolutionProject project = new(projectNode);
                if (project.Path.SequenceEqual(path))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public readonly void AddProject(ReadOnlySpan<char> path)
    {
        XMLNode projectNode = new("Project");
        SolutionProject project = new(projectNode);
        project.Path = path;
        rootNode.AddChild(projectNode);
    }

    public readonly void GetProjects(List<SolutionProject> projects)
    {
        foreach (XMLNode projectNode in rootNode.Children)
        {
            if (projectNode.Name.Equals("Project"))
            {
                projects.Add(new SolutionProject(projectNode));
            }
        }
    }
}
