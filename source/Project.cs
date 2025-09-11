using System;
using System.Collections.Generic;
using Unmanaged;
using XML;

namespace DotNetFiles;

public struct Project : IDisposable, ISerializable
{
    private XMLNode rootNode;
    private Collections.Generic.List<TargetFramework> targetFrameworks;

    public readonly Sdk Sdk
    {
        get
        {
            if (rootNode.TryGetAttribute(nameof(Sdk), out ReadOnlySpan<char> sdkValue))
            {
                if (Sdk.TryParse(sdkValue, out Sdk sdk))
                {
                    return sdk;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid SDK `{sdkValue.ToString()}`");
                }
            }
            else
            {
                throw new InvalidOperationException("No SDK attribute found in the project file");
            }
        }
    }

    public readonly ReadOnlySpan<TargetFramework> TargetFrameworks => targetFrameworks.AsSpan();

    public readonly LangVersion? LangVersion
    {
        get
        {
            ReadOnlySpan<char> condition = default;
            if (TryGetProperty(nameof(LangVersion), condition, out XMLNode langVersion))
            {
                if (DotNetFiles.LangVersion.TryParse(langVersion.Content.AsSpan(), out LangVersion result))
                {
                    return result;
                }
            }

            return null;
        }
        set
        {
            ReadOnlySpan<char> condition = default;
            if (TryGetProperty(nameof(LangVersion), condition, out XMLNode langVersion))
            {
                if (value is LangVersion nonNullValue)
                {
                    langVersion.Content.CopyFrom(nonNullValue.ToString());
                }
                else
                {
                    langVersion.Content.Clear();
                }
            }
            else
            {
                if (value is LangVersion nonNullValue)
                {
                    XMLNode propertyGroup = GetPropertyGroup(condition);
                    propertyGroup.Add(new XMLNode(nameof(LangVersion), nonNullValue.ToString()));
                }
            }
        }
    }

    public readonly bool? IsTestProject
    {
        get
        {
            if (TryGetProperty(nameof(IsTestProject), default, out XMLNode isTestProject))
            {
                return isTestProject.Content.AsSpan().Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            return null;
        }
        set
        {
            if (TryGetProperty(nameof(IsTestProject), default, out XMLNode isTestProject))
            {
                if (value is bool nonNullValue)
                {
                    isTestProject.Content.CopyFrom(nonNullValue ? "True" : "False");
                }
                else
                {
                    isTestProject.Content.Clear();
                }
            }
            else
            {
                if (value is bool nonNullValue)
                {
                    XMLNode propertyGroup = GetPropertyGroup(default);
                    propertyGroup.Add(new XMLNode(nameof(IsTestProject), nonNullValue ? "True" : "False"));
                }
            }
        }
    }

    public Project(ReadOnlySpan<byte> xmlBytes)
    {
        using ByteReader byteReader = new(xmlBytes);
        rootNode = byteReader.ReadObject<XMLNode>();
        targetFrameworks = new();
        LoadTargetFrameworks();
    }

    public readonly void Dispose()
    {
        targetFrameworks.Dispose();
        rootNode.Dispose();
    }

    public readonly override string ToString()
    {
        SerializationSettings settings = SerializationSettings.PrettyPrinted;
        settings.flags |= SerializationSettings.Flags.RootSpacing;
        settings.flags |= SerializationSettings.Flags.SkipEmptyNodes;
        settings.flags |= SerializationSettings.Flags.SpaceBeforeClosingNode;
        return rootNode.ToString(settings);
    }

    readonly void ISerializable.Write(ByteWriter byteWriter)
    {
        SaveTargetFrameworks();
        byteWriter.WriteObject(rootNode);
    }

    void ISerializable.Read(ByteReader byteReader)
    {
        rootNode = byteReader.ReadObject<XMLNode>();
        targetFrameworks = new();
        LoadTargetFrameworks();
    }

    private readonly void LoadTargetFrameworks()
    {
        ReadOnlySpan<char> condition = default;
        if (TryGetProperty("TargetFramework", condition, out XMLNode single))
        {
            targetFrameworks.Add(TargetFramework.Parse(single.Content.AsSpan()));
        }
        else if (TryGetProperty("TargetFrameworks", condition, out XMLNode multiple))
        {
            Span<char> content = multiple.Content.AsSpan();
            int start = 0;
            int index = 0;
            while (index < content.Length)
            {
                char c = content[index];
                if (c == ';')
                {
                    ReadOnlySpan<char> part = content[start..index];
                    targetFrameworks.Add(TargetFramework.Parse(part));
                    start = index + 1;
                }
                else if (index == content.Length - 1)
                {
                    ReadOnlySpan<char> part = content[start..];
                    targetFrameworks.Add(TargetFramework.Parse(part));
                }

                index++;
            }
        }
    }

    private readonly void SaveTargetFrameworks()
    {
        XMLNode propertyGroup = GetPropertyGroup(default);
        if (targetFrameworks.Count == 0)
        {
            // remove both TargetFramework and TargetFrameworks if exist
            if (propertyGroup.TryGetFirst("TargetFramework", out XMLNode single))
            {
                propertyGroup.TryRemove(single);
            }
            else if (propertyGroup.TryGetFirst("TargetFrameworks", out XMLNode multiple))
            {
                propertyGroup.TryRemove(multiple);
            }
        }
        else if (targetFrameworks.Count == 1)
        {
            // just one tfm
            if (propertyGroup.TryGetFirst("TargetFramework", out XMLNode single))
            {
                single.Content.CopyFrom(targetFrameworks[0].ToString());
            }
            else
            {
                // replace TargetFrameworks if exist
                if (propertyGroup.TryGetFirst("TargetFrameworks", out XMLNode multi))
                {
                    propertyGroup.TryRemove(multi);
                }

                propertyGroup.Add(new XMLNode("TargetFramework", targetFrameworks[0].ToString()));
            }
        }
        else
        {
            // more than 1 tfm
            using Text value = new();
            for (int i = 0; i < targetFrameworks.Count; i++)
            {
                if (i > 0)
                {
                    value.Append(';');
                }

                value.Append(targetFrameworks[i].ToString());
            }

            if (propertyGroup.TryGetFirst("TargetFrameworks", out XMLNode multi))
            {
                multi.Content.CopyFrom(value.AsSpan());
            }
            else
            {
                if (propertyGroup.TryGetFirst("TargetFramework", out XMLNode single))
                {
                    propertyGroup.TryRemove(single);
                }

                propertyGroup.Add(new XMLNode("TargetFrameworks", value.AsSpan()));
            }
        }
    }

    public readonly void ClearTargetFrameworks()
    {
        targetFrameworks.Clear();
        SaveTargetFrameworks();
    }

    public readonly bool AddTargetFramework(TargetFramework targetFramework)
    {
        Span<TargetFramework> span = targetFrameworks.AsSpan();
        if (!span.Contains(targetFramework))
        {
            targetFrameworks.Add(targetFramework);
            SaveTargetFrameworks();
            return true;
        }

        return false;
    }

    public readonly bool ContainsTargetFramework(TargetFramework targetFramework)
    {
        Span<TargetFramework> span = targetFrameworks.AsSpan();
        return span.Contains(targetFramework);
    }

    public readonly bool? Optimize(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("Optimize", condition);
    }

    public readonly void SetOptimize(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("Optimize", condition, value);
    }

    public readonly bool? TreatWarningsAsErrors(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("TreatWarningsAsErrors", condition);
    }

    public readonly void SetTreatWarningsAsErrors(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("TreatWarningsAsErrors", condition, value);
    }

    public readonly bool? IsTrimmable(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("IsTrimmable", condition);
    }

    public readonly void SetTrimmable(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("IsTrimmable", condition, value);
    }

    public readonly bool? IsAotCompatible(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("IsAotCompatible", condition);
    }

    public readonly void SetAotCompatible(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("IsAotCompatible", condition, value);
    }

    public readonly bool? GenerateDocumentationFile(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("GenerateDocumentationFile", condition);
    }

    public readonly void SetGenerateDocumentationFile(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("GenerateDocumentationFile", condition, value);
    }

    public readonly bool? AllowUnsafeBlocks(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("AllowUnsafeBlocks", condition);
    }

    public readonly void SetAllowUnsafeBlocks(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("AllowUnsafeBlocks", condition, value);
    }

    public readonly bool? IsPackable(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("IsPackable", condition);
    }

    public readonly void SetPackable(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("IsPackable", condition, value);
    }

    public readonly Nullable? Nullable(ReadOnlySpan<char> condition = default)
    {
        return GetEnum<Nullable>("Nullable", condition);
    }

    public readonly void SetNullable(Nullable? value, ReadOnlySpan<char> condition = default)
    {
        SetEnum("Nullable", condition, value);
    }

    public readonly bool? ImplicitUsings(ReadOnlySpan<char> condition = default)
    {
        return GetBoolean("ImplicitUsings", condition);
    }

    public readonly void SetImplicitUsings(bool? value, ReadOnlySpan<char> condition = default)
    {
        SetBoolean("ImplicitUsings", condition, value);
    }

    public readonly void GetEmbeddedResources(IList<EmbeddedResource> embeddedResources)
    {
        foreach (XMLNode itemNode in GetItems(nameof(EmbeddedResource)))
        {
            embeddedResources.Add(new EmbeddedResource(itemNode));
        }
    }

    public readonly void AddEmbeddedResource(ReadOnlySpan<char> include)
    {
        XMLNode newNode = new(nameof(EmbeddedResource));
        EmbeddedResource newEmbeddedResource = new(newNode);
        newEmbeddedResource.Include = include;

        if (TryGetItemGroupWith(nameof(EmbeddedResource), out XMLNode itemGroupNode))
        {
            itemGroupNode.Add(newNode);
        }
        else
        {
            XMLNode newItemGroup = new("ItemGroup");
            newItemGroup.Add(newNode);
            rootNode.Add(newItemGroup);
        }
    }

    public readonly bool RemoveEmbeddedResource(ReadOnlySpan<char> include)
    {
        return RemoveItemWithNameAndAttribute(nameof(EmbeddedResource), "Include", include);
    }

    public readonly void ClearEmbeddedResources()
    {
        RemoveItemsWithName(nameof(EmbeddedResource));
    }

    public readonly void GetProjectReferences(IList<ProjectReference> projectReferences)
    {
        foreach (XMLNode itemNode in GetItems(nameof(ProjectReference)))
        {
            projectReferences.Add(new ProjectReference(itemNode));
        }
    }

    public readonly void AddProjectReference(ReadOnlySpan<char> include)
    {
        XMLNode newNode = new(nameof(ProjectReference));
        ProjectReference newProjectReference = new(newNode);
        newProjectReference.Include = include;

        if (TryGetItemGroupWith(nameof(ProjectReference), out XMLNode itemGroupNode))
        {
            itemGroupNode.Add(newNode);
        }
        else
        {
            XMLNode newItemGroup = new("ItemGroup");
            newItemGroup.Add(newNode);
            rootNode.Add(newItemGroup);
        }
    }

    public readonly bool RemoveProjectReference(ReadOnlySpan<char> include)
    {
        return RemoveItemWithNameAndAttribute(nameof(ProjectReference), "Include", include);
    }

    public readonly void ClearProjectReferences()
    {
        RemoveItemsWithName(nameof(ProjectReference));
    }

    public readonly void GetPackageReferences(IList<PackageReference> packageReferences)
    {
        foreach (XMLNode itemNode in GetItems(nameof(PackageReference)))
        {
            packageReferences.Add(new PackageReference(itemNode));
        }
    }

    public readonly void AddPackageReference(ReadOnlySpan<char> include, SemanticVersion version)
    {
        XMLNode newNode = new(nameof(PackageReference));
        PackageReference newPackageReference = new(newNode);
        newPackageReference.Include = include;
        newPackageReference.Version = version;

        if (TryGetItemGroupWith(nameof(PackageReference), out XMLNode itemGroupNode))
        {
            itemGroupNode.Add(newNode);
        }
        else
        {
            XMLNode newItemGroup = new("ItemGroup");
            newItemGroup.Add(newNode);
            rootNode.Add(newItemGroup);
        }
    }

    public readonly bool RemovePackageReference(ReadOnlySpan<char> include)
    {
        return RemoveItemWithNameAndAttribute(nameof(PackageReference), "Include", include);
    }

    public readonly void ClearPackageReferences()
    {
        RemoveItemsWithName(nameof(PackageReference));
    }

    public readonly void GetAnalyzers(IList<Analyzer> analyzers)
    {
        foreach (XMLNode itemNode in GetItems(nameof(Analyzer)))
        {
            analyzers.Add(new Analyzer(itemNode));
        }
    }

    public readonly void AddAnalyzer(ReadOnlySpan<char> include)
    {
        XMLNode newNode = new(nameof(Analyzer));
        Analyzer newAnalyzer = new(newNode);
        newAnalyzer.Include = include;

        if (TryGetItemGroupWith(nameof(Analyzer), out XMLNode itemGroupNode))
        {
            itemGroupNode.Add(newNode);
        }
        else
        {
            XMLNode newItemGroup = new("ItemGroup");
            newItemGroup.Add(newNode);
            rootNode.Add(newItemGroup);
        }
    }

    public readonly bool RemoveAnalyzer(ReadOnlySpan<char> include)
    {
        return RemoveItemWithNameAndAttribute(nameof(Analyzer), "Include", include);
    }

    public readonly void ClearAnalyzers()
    {
        RemoveItemsWithName(nameof(Analyzer));
    }

    public readonly void GetContent(IList<Content> content)
    {
        foreach (XMLNode itemNode in GetItems(nameof(Content)))
        {
            content.Add(new Content(itemNode));
        }
    }

    public readonly void AddContent(ReadOnlySpan<char> include, bool? pack = true, ReadOnlySpan<char> packagePath = default, bool? visible = null)
    {
        XMLNode newNode = new(nameof(Content));
        Content newContent = new(newNode);
        newContent.Include = include;
        newContent.Pack = pack;
        newContent.PackagePath = packagePath;
        newContent.Visible = visible;

        if (TryGetItemGroupWith(nameof(Content), out XMLNode itemGroupNode))
        {
            itemGroupNode.Add(newNode);
        }
        else
        {
            XMLNode newItemGroup = new("ItemGroup");
            newItemGroup.Add(newNode);
            rootNode.Add(newItemGroup);
        }
    }

    public readonly bool RemoveContent(ReadOnlySpan<char> include)
    {
        return RemoveItemWithNameAndAttribute(nameof(Content), "Include", include);
    }

    public readonly void ClearContent()
    {
        RemoveItemsWithName(nameof(Content));
    }

    private readonly void RemoveItemsWithName(ReadOnlySpan<char> itemName)
    {
        if (TryGetItemGroupWith(itemName, out XMLNode itemGroupNode))
        {
            for (int i = itemGroupNode.Children.Length - 1; i >= 0; i--)
            {
                XMLNode itemNode = itemGroupNode.Children[i];
                if (NameEquals(itemNode, itemName))
                {
                    itemGroupNode.RemoveAt(i, out _);
                }
            }

            if (itemGroupNode.Children.Length == 0)
            {
                rootNode.TryRemove(itemGroupNode);
            }
        }
    }

    private readonly bool RemoveItemWithNameAndAttribute(ReadOnlySpan<char> itemName, ReadOnlySpan<char> attributeName, ReadOnlySpan<char> attributeValue)
    {
        if (TryGetItemGroupWith(itemName, out XMLNode itemGroupNode))
        {
            for (int i = 0; i < itemGroupNode.Children.Length; i++)
            {
                XMLNode itemNode = itemGroupNode.Children[i];
                if (NameEquals(itemNode, itemName))
                {
                    if (itemNode.TryGetAttribute(attributeName, out ReadOnlySpan<char> value) && value.SequenceEqual(attributeValue))
                    {
                        itemGroupNode.RemoveAt(i, out _);
                        return true;
                    }
                }
            }

            if (itemGroupNode.Children.Length == 0)
            {
                rootNode.TryRemove(itemGroupNode);
            }
        }

        return false;
    }

    private readonly bool TryGetItemGroupWith(ReadOnlySpan<char> itemName, out XMLNode itemGroupNode)
    {
        foreach (XMLNode itemGroup in rootNode.Children)
        {
            if (itemGroup.Name.Equals("ItemGroup"))
            {
                foreach (XMLNode item in itemGroup.Children)
                {
                    if (item.Name.AsSpan().Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        itemGroupNode = itemGroup;
                        return true;
                    }
                }
            }
        }

        itemGroupNode = default;
        return false;
    }

    private readonly IEnumerable<XMLNode> GetItems(ASCIIText32 itemName)
    {
        for (int i = 0; i < rootNode.Children.Length; i++)
        {
            XMLNode itemGroup = rootNode.Children[i];
            if (itemGroup.Name.Equals("ItemGroup"))
            {
                for (int j = 0; j < itemGroup.Children.Length; j++)
                {
                    XMLNode item = itemGroup.Children[j];
                    if (NameEquals(item, itemName))
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    private readonly bool NameEquals(XMLNode node, ASCIIText32 other)
    {
        Span<char> buffer = stackalloc char[other.Length];
        other.CopyTo(buffer);
        return buffer.Equals(node.Name.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    private readonly bool? GetBoolean(ReadOnlySpan<char> propertyName, ReadOnlySpan<char> condition)
    {
        if (TryGetProperty(propertyName, condition, out XMLNode propertyNode))
        {
            return propertyNode.Content.AsSpan().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return null;
    }

    private readonly void SetBoolean(ReadOnlySpan<char> propertyName, ReadOnlySpan<char> condition, bool? value)
    {
        if (TryGetProperty(propertyName, condition, out XMLNode propertyNode))
        {
            if (value is bool nonNullValue)
            {
                propertyNode.Content.CopyFrom(nonNullValue ? "True" : "False");
            }
            else
            {
                propertyNode.Content.Clear();
            }
        }
        else
        {
            if (value is bool nonNullValue)
            {
                XMLNode propertyGroup = GetPropertyGroup(condition);
                propertyGroup.Add(new XMLNode(propertyName, nonNullValue ? "True" : "False"));
            }
        }
    }

    private readonly T? GetEnum<T>(ReadOnlySpan<char> propertyName, ReadOnlySpan<char> condition) where T : struct, Enum
    {
        if (TryGetProperty(propertyName, condition, out XMLNode propertyNode))
        {
            if (Enum.TryParse(propertyNode.Content.AsSpan(), true, out T result))
            {
                return result;
            }
        }

        return null;
    }

    private readonly void SetEnum<T>(ReadOnlySpan<char> propertyName, ReadOnlySpan<char> condition, T? value) where T : struct, Enum
    {
        if (TryGetProperty(propertyName, condition, out XMLNode propertyNode))
        {
            if (value is T nonNullValue)
            {
                propertyNode.Content.CopyFrom(Enum.GetName(nonNullValue) ?? string.Empty);
            }
            else
            {
                propertyNode.Content.Clear();
            }
        }
        else
        {
            if (value is T nonNullValue)
            {
                XMLNode propertyGroup = GetPropertyGroup(condition);
                propertyGroup.Add(new XMLNode(propertyName, Enum.GetName(nonNullValue) ?? string.Empty));
            }
        }
    }

    private readonly XMLNode GetPropertyGroup(ReadOnlySpan<char> condition)
    {
        foreach (XMLNode property in rootNode.Children)
        {
            if (property.Name.Equals("PropertyGroup"))
            {
                if (property.TryGetAttribute("Condition", out ReadOnlySpan<char> propertyGroupCondition))
                {
                    if (condition.SequenceEqual(propertyGroupCondition))
                    {
                        return property;
                    }
                }
                else if (condition.IsEmpty)
                {
                    return property;
                }
            }
        }

        throw new InvalidOperationException("No PropertyGroup found in the project file");
    }

    private readonly bool TryGetProperty(ReadOnlySpan<char> name, ReadOnlySpan<char> condition, out XMLNode foundProperty)
    {
        XMLNode propertyGroup = GetPropertyGroup(condition);
        foreach (XMLNode property in propertyGroup.Children)
        {
            if (property.Name.Equals(name))
            {
                foundProperty = property;
                return true;
            }
        }

        foundProperty = default;
        return false;
    }

    private readonly XMLNode GetProperty(ReadOnlySpan<char> name, ReadOnlySpan<char> condition)
    {
        XMLNode propertyGroup = GetPropertyGroup(condition);
        foreach (XMLNode property in propertyGroup.Children)
        {
            if (property.Name.Equals(name))
            {
                return property;
            }
        }

        throw new InvalidOperationException($"No property `{name.ToString()}` found in the project file");
    }
}